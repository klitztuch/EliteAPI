using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EliteAPI.Abstractions;
using EliteAPI.Dashboard.Logging.WebSockets;
using EliteAPI.Dashboard.Plugins.Installer;
using EliteAPI.Dashboard.WebSockets.Message;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EliteAPI.Dashboard.WebSockets.Handler
{
    public enum WebSocketType
    {
        FrontEnd,
        Client,
        Plugin
    }

    public class WebSocketHandler
    {
        private readonly ILogger<WebSocketHandler> _log;
        private readonly IEliteDangerousApi _api;
        private readonly PluginInstaller _pluginInstaller;

        private readonly List<WebSocket> _frontendWebSockets;
        private readonly List<WebSocket> _clientWebSockets;
        private readonly List<WebSocket> _pluginWebSockets;

        private readonly List<WebSocketMessage> _frontendCatchupMessages;
        private readonly List<WebSocketMessage> _clientCatchupMessages;
        private readonly List<WebSocketMessage> _pluginCatchupMessages;

        public IReadOnlyList<string> OpenPlugins { get; private set; }
        
        public WebSocketHandler(ILogger<WebSocketHandler> log, IEliteDangerousApi api, PluginInstaller pluginInstaller)
        {
            _log = log;
            _api = api;
            _pluginInstaller = pluginInstaller;

            OpenPlugins = new List<string>();
            
            _pluginInstaller.OnStart += async (sender, e) =>
            {
                await Broadcast(new WebSocketMessage("Plugin.Start", e));
            };
            
            _pluginInstaller.OnDownloadProgress += async (sender, e) =>
            {
                await Broadcast(new WebSocketMessage("Plugin.Progress.Download", e));
            };
            
            _pluginInstaller.OnInstallProgress += async (sender, e) =>
            {
                await Broadcast(new WebSocketMessage("Plugin.Progress.Install", e));
            };

            _pluginInstaller.OnFinished += async (sender, e) =>
            {
                await Broadcast(new WebSocketMessage("Plugin.Finished", e));
            };
            
            _pluginInstaller.OnError += async (sender, e) =>
            {
                await Broadcast(new WebSocketMessage("Plugin.Error", e));
            };

            _frontendWebSockets = new List<WebSocket>();
            _clientWebSockets = new List<WebSocket>();
            _pluginWebSockets = new List<WebSocket>();
            
            _frontendCatchupMessages = new List<WebSocketMessage>();
            _clientCatchupMessages = new List<WebSocketMessage>();
            _pluginCatchupMessages = new List<WebSocketMessage>();

            WebSocketLogs.OnLog += async (sender, e) =>
                await Broadcast(new WebSocketMessage("Log", JsonConvert.SerializeObject(e)), WebSocketType.FrontEnd,
                    true, false);
        }

        public async Task Handle(WebSocket socket, WebSocketType type)
        {
            switch (type)
            {
                case WebSocketType.Client:
                    // Add socket to list of client WebSockets
                    _clientWebSockets.Add(socket);
                    break;

                case WebSocketType.FrontEnd:
                    // Add socket to list of frontend WebSockets
                    _frontendWebSockets.Add(socket);
                    break;
                
                case WebSocketType.Plugin:
                    // Add socket to list of plugin WebSockets
                    _pluginWebSockets.Add(socket);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid WebSocket type");
            }

            // Keep connection to socket open
            await ListenTo(socket, type);
        }

        public async Task Broadcast(WebSocketMessage message, bool useDuringCatchup = false, bool onlySaveLatestForCatchup = false)
        {
            await Broadcast(message, WebSocketType.FrontEnd, useDuringCatchup, onlySaveLatestForCatchup);
            await Broadcast(message, WebSocketType.Client, useDuringCatchup, onlySaveLatestForCatchup);
            await Broadcast(message, WebSocketType.Plugin, useDuringCatchup, onlySaveLatestForCatchup);
        }

        public async Task Broadcast(WebSocketMessage message, WebSocketType type, bool useDuringCatchup, bool onlySaveLatestForCatchup)
        {
            switch (type)
            {
                case WebSocketType.Client:
                    // Store in catchup
                    if (useDuringCatchup)
                    {
                        if (onlySaveLatestForCatchup)
                        {
                            // Replace of same type
                            _clientCatchupMessages.RemoveAll(x =>
                                string.Equals(x.Type, message.Type, StringComparison.InvariantCultureIgnoreCase));
                        }

                        // Add
                        _clientCatchupMessages.Add(message);
                    }

                    // Broadcast to client WebSockets
                    foreach (var clientWebSocket in _clientWebSockets)
                    {
                        await SendTo(clientWebSocket, message);
                    }
                    break;

                case WebSocketType.FrontEnd:
                    // Store in catchup
                    if (useDuringCatchup)
                    {
                        if (onlySaveLatestForCatchup)
                        {
                            // Replace of same type
                            _frontendCatchupMessages.RemoveAll(x =>
                                string.Equals(x.Type, message.Type, StringComparison.InvariantCultureIgnoreCase));
                        }

                        // Add
                        _frontendCatchupMessages.Add(message);
                    }

                    // Broadcast to frontend WebSockets
                    foreach (var frontendWebSocket in _frontendWebSockets)
                    {
                        await SendTo(frontendWebSocket, message);
                    }
                    break;

                case WebSocketType.Plugin:
                    // Store in catchup
                    if (useDuringCatchup)
                    {
                        if (onlySaveLatestForCatchup)
                        {
                            // Replace of same type
                            _pluginCatchupMessages.RemoveAll(x =>
                                string.Equals(x.Type, message.Type, StringComparison.InvariantCultureIgnoreCase));
                        }

                        // Add
                        _pluginCatchupMessages.Add(message);
                    }

                    // Broadcast to frontend WebSockets
                    foreach (var pluginWebSocket in _pluginWebSockets)
                    {
                        await SendTo(pluginWebSocket, message);
                    }
                    break;
                
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid WebSocket type");
            }
        }

        private async Task SendTo(WebSocket socket, WebSocketMessage message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var compressed = Compressor.Compress(message);

            var bytes = Encoding.UTF8.GetBytes(compressed);

            var arraySegment = new ArraySegment<byte>(bytes);
            _log.LogInformation($"Sending {message.Type}:'{message.Value}'");
            
            await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task ListenTo(WebSocket socket, WebSocketType type)
        {
            await using var memory = new MemoryStream();
            bool isAuthenticated = false;

            List<string> plugins;
            string name = "";

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    // Read message
                    var message = await GetMessage(socket, memory);

                    _log.LogInformation("WebSocket request ({Type}): {Json}", message.Type, message.Value);

                    // Check authentication
                    if (!isAuthenticated)
                    {
                        _log.LogDebug("Unauthenticated socket, checking authentication");
                        (isAuthenticated, name) = await CheckAuthentication(message, type);

                        if (type == WebSocketType.FrontEnd)
                        {
                            foreach (var openPlugin in OpenPlugins)
                            {
                                await Broadcast(new WebSocketMessage("Plugin.Connected", openPlugin), WebSocketType.FrontEnd, false, false);
                            }
                        }

                        // Break connection if still not authenticated
                        if (!isAuthenticated)
                        {
                            _log.LogDebug("Did not pass authentication, kicking");
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Unauthenticated",
                                CancellationToken.None);
                            return;
                        }

                        // Start listening from next message
                        _log.LogDebug("Passed authentication, sending catchup");

                        plugins = OpenPlugins.ToList();
                        plugins.Add(name);
                        OpenPlugins = plugins;
                        await Broadcast(new WebSocketMessage("Plugin.Connected", name), WebSocketType.FrontEnd, false, false);

                        // Send EliteAPI information
                        await SendTo(socket, new WebSocketMessage("EliteAPI", $"{{\"Version\": \"{_api.Version}\"}}"));
                        await SendTo(socket, new WebSocketMessage("UserProfile", UserProfile.Get()));

                        
                        switch (type)
                        {
                            // Send catchup messages to frontend
                            case WebSocketType.FrontEnd:
                                await Catchup(socket, _frontendCatchupMessages);
                                break;

                            // Send catchup messages to client
                            case WebSocketType.Client:
                                await Catchup(socket, _clientCatchupMessages);
                                break;

                            // Send catchup messages to client
                            case WebSocketType.Plugin:
                                await Catchup(socket, _pluginCatchupMessages);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid WebSocket type");
                        }
                    }


                    // Process message
                    switch (message.Type.ToLower())
                    {
                        case "userprofile.get":
                            await SendTo(socket, new WebSocketMessage("UserProfile", UserProfile.Get()));
                            break;

                        case "userprofile.set":
                            UserProfile.Set(message.Value);
                            await SendTo(socket, new WebSocketMessage("UserProfile", UserProfile.Get()));
                            break;
                    }

                    // Install & Uninstall plugins
                    foreach (var plugin in await _pluginInstaller.GetPlugins())
                    {
                        if (message.Type.Equals($"Plugin.Install", StringComparison.InvariantCultureIgnoreCase) &&
                            message.Value.Equals(plugin.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _log.LogInformation("Installing plugin {Plugin}", plugin.Name);
                            await _pluginInstaller.Install(plugin);
                            await Broadcast(new WebSocketMessage("UserProfile", UserProfile.Get()), true);
                        }

                        if (message.Type.Equals($"Plugin.Uninstall", StringComparison.InvariantCultureIgnoreCase) &&
                            message.Value.Equals(plugin.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _log.LogInformation("Uninstalling plugin {Plugin}", plugin.Name);
                            await _pluginInstaller.Uninstall(plugin);
                            await Broadcast(new WebSocketMessage("UserProfile", UserProfile.Get()), true);
                        }
                    }
                }
            } catch(Exception e)
            {
                _log.LogWarning(e, "Error processing socket");
                
                if (!string.IsNullOrWhiteSpace(name))
                {
                    await Broadcast(new WebSocketMessage("Plugin.Disconnected", name), WebSocketType.FrontEnd, false, false);
                    plugins = OpenPlugins.ToList();
                    plugins.Remove(name);
                    OpenPlugins = plugins;
                }
            }


            if (!string.IsNullOrWhiteSpace(name))
            {
                await Broadcast(new WebSocketMessage("Plugin.Disconnected", name), WebSocketType.FrontEnd, false, false);
                plugins = OpenPlugins.ToList();
                plugins.Remove(name);
                OpenPlugins = plugins;
            }
        }

        private async Task Catchup(WebSocket socket, List<WebSocketMessage> messages)
        {
            await SendTo(socket, new WebSocketMessage("CatchupStart", messages.Count));
            foreach (var webSocketMessage in messages)
            {
                await SendTo(socket, webSocketMessage);
                await Task.Delay(30);
            }
            await SendTo(socket, new WebSocketMessage("CatchupEnd"));
        }

        private Task<(bool success, string name)> CheckAuthentication(WebSocketMessage message, WebSocketType type)
        {
            // Type must be auth
            if (!string.Equals(message.Type, "auth", StringComparison.InvariantCultureIgnoreCase))
                return Task.FromResult((false, ""));

            // Value must the unique plugin name
            if(type == WebSocketType.Plugin && OpenPlugins.Contains(message.Value, StringComparer.InvariantCultureIgnoreCase))
                return Task.FromResult((false, ""));
            
            return Task.FromResult((true, message.Value));
        }

        private async Task<WebSocketMessage> GetMessage(WebSocket socket, MemoryStream memory)
        {
            try
            {
                // Read received message
                WebSocketReceiveResult result;
                do
                {
                    var messageBuffer = WebSocket.CreateClientBuffer(1024, 16);
                    result = await socket.ReceiveAsync(messageBuffer, CancellationToken.None);
                    memory.Write(messageBuffer.Array ?? Array.Empty<byte>(), messageBuffer.Offset, messageBuffer.Count);
                } while (!result.EndOfMessage);

                // Process the received message
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    memory.Seek(0, SeekOrigin.Begin);
                    memory.Position = 0;

                    WebSocketMessage message;

                    string textMessage = Encoding.UTF8.GetString(memory.ToArray());
                    _log.LogCritical("Message: {Json}", textMessage);
                    
                    try
                    {
                        message = Compressor.Decompress(textMessage);
                    }
                    catch (JsonException ex)
                    {
                        _log.LogWarning(ex, "Invalid WebSocket message ({Message})", textMessage);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "Could not process WebSocket request");
                        return null;
                    }

                    return message;
                }

                _log.LogWarning("Could not process WebSocket request, message type is not text");
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Could not read WebSocket message");
            }

            memory.Seek(0, SeekOrigin.Begin);
            memory.Position = 0;
            return null;
        }
    }
}