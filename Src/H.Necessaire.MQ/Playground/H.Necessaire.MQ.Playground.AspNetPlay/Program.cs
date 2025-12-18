using H.Necessaire.MQ.Playground.AspNetPlay.UseCases;
using H.Necessaire.Runtime.Integration.NetCore;
using H.Necessaire.Runtime.Integration.NetCore.Concrete;
using H.Necessaire.Runtime.Integration.NetCore.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace H.Necessaire.MQ.Playground.AspNetPlay
{
    public class Program
    {
        public static App App;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            OverwriteRuntimConfigFromEnvironmentVariables(builder.Configuration);

            App = App ?? new App(
                new AppWireup()
                .With(x => x.Register<ImAConfigProvider>(() => new NetCoreConfigProvider(builder.Configuration)))
                .WithEverything()
            );

            builder.Services.AddHNecessaireDependenciesToNetCore(App.Wireup.DependencyRegistry);
            builder.Services.AddNetCoreDependenciesToHNecessaire(App.Wireup.DependencyRegistry);
            builder.Services.AddControllers().AddApplicationPart(typeof(Runtime.Integration.NetCore.Controllers.PingController).Assembly).AddControllersAsServices();
            builder.Services.AddRouting();

            var app = builder.Build();

            app.Use((ctx, next) => { ctx.Request.EnableBuffering(); return next(); });
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseRouting();
            app.MapControllers();

            app.MapGet("/", () => "Hello World!");
            app.MapGet("/health", () => $"Alive @ {DateTime.UtcNow.PrintTimeStampAsIdentifier()}");
            app.MapPost("/enqueue", async ([FromBody] QdAction qdAction) => await app.Services.GetRequiredService<QdActionUseCase>().Enqueue(qdAction));

            app.Run();
        }

        static void OverwriteRuntimConfigFromEnvironmentVariables(ConfigurationManager configurationManager)
        {
            configurationManager["SqlConnections:DefaultConnectionString"] = Environment.GetEnvironmentVariable("SqlServerConnectionString");
            configurationManager["SqlConnections:DatabaseNames:Core"] = Environment.GetEnvironmentVariable("DefaultDatabaseName");

            configurationManager["QdActions:MaxProcessingAttempts"] = Environment.GetEnvironmentVariable("QdActions.MaxProcessingAttempts");

            configurationManager["QdActions:RabbitMQ:HostName"] = Environment.GetEnvironmentVariable("QdActions.RabbitMQ.HostName");
            configurationManager["QdActions:RabbitMQ:VirtualHost"] = Environment.GetEnvironmentVariable("QdActions.RabbitMQ.VirtualHost");
            configurationManager["QdActions:RabbitMQ:UserName"] = Environment.GetEnvironmentVariable("QdActions.RabbitMQ.UserName");
            configurationManager["QdActions:RabbitMQ:Password"] = Environment.GetEnvironmentVariable("QdActions.RabbitMQ.Password");
        }
    }
}
