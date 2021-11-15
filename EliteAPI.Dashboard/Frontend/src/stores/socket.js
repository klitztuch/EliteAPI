import {connectionState, isConnected, userProfile, loadingPlugins, openPlugins} from "./stores";

export default {
    socket: null,

    send(type, data) {
        send(this.socket, type, data);
    },
    
    connect() {
        this.socket = new WebSocket("ws://localhost:51555/ws", 'EliteAPI-app');
        connectionState.set("Connecting to EliteAPI");
        isConnected.set(false);

        this.socket.onmessage = (e) => {
            let message = decompress(e.data);
            console.log(message);

            if(message.type === "CatchupStart") {
                connectionState.set("Catching up with EliteAPI");
            }

            if(message.type === "CatchupEnd") {
                isConnected.set(true);
            }

            if (message.type === "UserProfile") {
                userProfile.set(message.value);
            }

            if(message.type == "Plugin.Start") {
                loadingPlugins.update(e => [...e, message.value]);
            }

            if(message.type == "Plugin.Finished") {
                loadingPlugins.update(e => e.filter(x => x !== message.value));
            }

            if(message.type == "Plugin.Error") {
                loadingPlugins.update(e => e.filter(x => x !== message.value.value1));
            }

            if(message.type == "Plugin.Connected") {
                openPlugins.update(e => [...e, message.value]);
            }

            if(message.type == "Plugin.Disconnected") {
                openPlugins.update(e => e.filter(x => x !== message.value));
            }
        }

        this.socket.onclose = (e) => {
            isConnected.set(false);
            connectionState.set("Could not connect to EliteAPI");
            setTimeout(() => {
                this.connect();
            }, 3000)
        }

        this.socket.onopen = (e) => {
            connectionState.set("Connected to EliteAPI");
            send(this.socket, 'auth', 'frontend')
        }

        this.socket.onerror = (e) => {
            isConnected.set(false);
            connectionState.set("Error connecting to EliteAPI");
        }
    },
}

function send(socket, type, data) {
    let message = {
        type: type,
        value: data
    };

    let compressed = compress(message);

    console.log("Sending", compressed);

    socket.send(compressed);
}

function compress(message) {
    return JSON.stringify(message);
}

function decompress(message) {
    let object = JSON.parse(message);

    if (object.value && (object.value.startsWith("{") || object.value.startsWith("["))) {
        object.value = JSON.parse(object.value);
    }

    return object;
}
