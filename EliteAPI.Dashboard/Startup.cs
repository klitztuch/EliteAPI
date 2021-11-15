using EliteAPI.Dashboard.Plugins.Installer;
using EliteAPI.Dashboard.WebSockets;
using EliteAPI.Services.Variables;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EliteAPI.Dashboard
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add CORS
            services.AddCors();

            // Add EliteAPI
            services.AddEliteAPI();

            // Add WebSocket handlers
            services.AddWebSocketHandshake();

            // Add controllers
            services.AddControllers();

            services.AddHttpClient();


            // Variable service
            services.AddTransient<VariablesService>();
            services.AddSingleton<PluginInstaller>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Show exceptions if we're developing
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            // Redirect to Https
            //app.UseHttpsRedirection();

            // Allow for CORS   
            app.UseCors();

            // Allow and handle websockets
            app.UseWebSockets();
            app.UseWebSocketHandshake();
        }
    }
}