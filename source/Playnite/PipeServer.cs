using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite
{
    public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs args);

    public class CommandExecutedEventArgs : EventArgs
    {
        public CmdlineCommand Command
        {
            get; set;
        }

        public string Args
        {
            get; set;
        }

        public CommandExecutedEventArgs()
        {
        }

        public CommandExecutedEventArgs(CmdlineCommand command, string args)
        {
            Command = command;
            Args = args;
        }
    }

    [ServiceContract]
    public interface IPipeService
    {
        [OperationContract(IsOneWay = true)]
        void InvokeCommand(CmdlineCommand command, string args);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
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
            // We don't want to block this call because it causes issues if some sync operation that shuts down server is also called.
            // For example, mode switch or instance shutdown calls are stopping server,
            // which results in serviceHost.Close() timeout, since server would be still waiting for InvokeCommand to finish.
            Task.Run(async () =>
            {
                await Task.Delay(100);
                syncContext.Post(_ => CommandExecuted?.Invoke(this, new CommandExecutedEventArgs(command, args)), null);
            });
        }
    }

    public class PipeServer
    {
        private string endpoint;
        private ServiceHost serviceHost;

        public PipeServer(string endpoint)
        {
            this.endpoint = endpoint;
        }

        public void StartServer(IPipeService service)
        {
            serviceHost = new ServiceHost(service, new Uri[] { new Uri(endpoint) });
            serviceHost.AddServiceEndpoint(typeof(IPipeService), new NetNamedPipeBinding(), "PlayniteService");
            serviceHost.Open();
        }

        public void StopServer()
        {
            serviceHost.Close();
        }
    }

    public class PipeClient : ClientBase<IPipeService>
    {
        public PipeClient(string endpoint)
            : base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IPipeService)), new NetNamedPipeBinding(), new EndpointAddress(endpoint.TrimEnd('/') + @"/PlayniteService")))
        {
        }

        public void InvokeCommand(CmdlineCommand command, string args)
        {
            Channel.InvokeCommand(command, args);
        }
    }
}
