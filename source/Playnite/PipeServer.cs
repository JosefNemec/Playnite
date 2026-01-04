using CoreWCF.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.WebUI;

namespace Playnite
{
    public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs args);

    public class CommandExecutedEventArgs : EventArgs
    {
        public CmdlineCommand Command { get; set; }
        public string Args { get; set; }
        public CommandExecutedEventArgs() { }
        public CommandExecutedEventArgs(CmdlineCommand command, string args)
        {
            Command = command;
            Args = args;
        }
    }

    // 1. Ambiguity Fix: Use CoreWCF for Server Contract
    [CoreWCF.ServiceContract]
    public interface IPipeService
    {
        [CoreWCF.OperationContract(IsOneWay = true)]
        void InvokeCommand(CmdlineCommand command, string args);
    }

    [CoreWCF.ServiceBehavior(ConcurrencyMode = CoreWCF.ConcurrencyMode.Multiple, InstanceContextMode = CoreWCF.InstanceContextMode.Single)]
    public class PipeService : IPipeService
    {
        private readonly SynchronizationContext syncContext;
        public event CommandExecutedEventHandler CommandExecuted;

        public PipeService()
        {
            syncContext = SynchronizationContext.Current;
        }

        public void InvokeCommand(CmdlineCommand command, string args)
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                syncContext?.Post(_ => CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, args)), null);
            });
        }
    }

    public class PipeServer
    {
        private string endpoint;
        private IHost host; // Replaces ServiceHost

        public PipeServer(string endpoint)
        {
            this.endpoint = endpoint;
        }

        public async void StartServer(IPipeService service)
        {
            var builder = WebApplication.CreateBuilder();

            // 2. Configure Named Pipe Transport
            builder.WebHost.UseNetNamedPipe(options =>
            {
                options.Listen(endpoint);
            });

            // 3. Register service instance via Dependency Injection
            builder.Services.AddServiceModelServices();
            builder.Services.AddSingleton<IPipeService>(service);

            var app = builder.Build();

            // 4. Map the WCF Endpoint
            app.UseServiceModel(serviceBuilder =>
            {
                serviceBuilder.AddService<IPipeService>();
                serviceBuilder.AddServiceEndpoint<IPipeService, IPipeService>(
                    new CoreWCF.NetNamedPipeBinding(),
                    "/PlayniteService"
                );
            });

            host = app;
            await host.StartAsync(); // Starts host in background
        }

        public async void StopServer()
        {
            if (host != null)
            {
                await host.StopAsync();
                host.Dispose();
            }
        }
    }

    // 5. CLIENT SIDE: Keep using System.ServiceModel
    public class PipeClient : ClientBase<IPipeService>
    {
        public PipeClient(string endpoint)
            : base(new System.ServiceModel.NetNamedPipeBinding(),
                   new EndpointAddress(endpoint.TrimEnd('/') + @"/PlayniteService"))
        {
        }

        public void InvokeCommand(CmdlineCommand command, string args)
        {
            Channel.InvokeCommand(command, args);
        }
    }
}