using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class JsonRpcException : Exception
    {
        public int ErrorCode { get; set; }

        public JsonRpcException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 1024;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class JsonRpcClient : IDisposable
    {
        public class RpcResponseError
        {
            [JsonProperty(PropertyName = "code")]
            public int Code;

            [JsonProperty(PropertyName = "message")]
            public string Message;
        }

        public class RpcMessage
        {
            [JsonProperty(PropertyName = "jsonrpc")]
            public string JsonRpcProtocol = "2.0";

            [JsonProperty(PropertyName = "id")]
            public int? Id;

            [JsonProperty(PropertyName = "result")]
            public object Result;

            [JsonProperty(PropertyName = "method")]
            public string Method;

            [JsonProperty(PropertyName = "params")]
            public object Params;

            [JsonProperty(PropertyName = "error")]
            public RpcResponseError Error;
        }

        public class RpcResponse<TResult> : RpcMessage where TResult : class
        {
            [JsonProperty(PropertyName = "result")]
            public new TResult Result;
        }

        public class RpcRequest : RpcMessage
        {
            private static Random rndGen = new Random();

            public RpcRequest()
            {
            }

            public RpcRequest(string method) : this(method, null)
            {
            }

            public RpcRequest(string method, object parameter)
            {
                Method = method;                
                Params = parameter;
                Id = rndGen.Next();
            }
        }

        public class RpcNotification : RpcRequest
        {
            public RpcNotification(string method) : this(method, null)
            {
            }

            public RpcNotification(string method, object parameter) : base(method, parameter)
            {
                Id = null;
            }
        }

        private readonly Encoding encoding = Encoding.UTF8;
        private readonly Socket socket;
        private AutoResetEvent requestEvent = new AutoResetEvent(false);
        private string latestResponse = string.Empty;

        public JsonRpcClient(string address)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var split = address.Split(new char[] { ':' });
            socket.Connect(
                split[0],
                Convert.ToInt32(split[1]));

            StartListening();
        }

        private Task StartListening()
        {
            return Task.Run(async () =>
            {
                byte[] bytes = new byte[1024];
                var procData = new StringBuilder();
                while (true)
                {
                    if (!socket.Connected)
                    {
                        break;
                    }

                    var bytesRec = socket.Receive(bytes);
                    if (bytesRec > 0)
                    {
                        // TODO solve multiple objects in sigle buffer

                        procData.Append(encoding.GetString(bytes, 0, bytesRec));
                        if (procData.ToString().IndexOf('\n') > 0)
                        {
                            DataReceived(procData.ToString().TrimEnd('\n'));
                            procData.Clear();
                        }
                    }

                    await Task.Delay(20);
                }
            });
        }

        private void DataReceived(string data)
        {
            var request = Serialization.FromJson<RpcMessage>(data);
            if (request.Id != null && string.IsNullOrEmpty(request.Method))
            {
                // TODO: Add id correlation check
                latestResponse = data;
                requestEvent.Set();
            }
            else
            {
                Console.WriteLine(data);
            }
        }

        public void Dispose()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private string SendRpcRequest(string data)
        {
            requestEvent.Reset();
            byte[] msg = encoding.GetBytes(data + '\n');
            int bytesSent = socket.Send(msg);
            if (!requestEvent.WaitOne(5000))
            {
                throw new TimeoutException();
            }

            return latestResponse;
        }

        private void SendRpcNotification(string data)
        {
            byte[] msg = encoding.GetBytes(data + '\n');
            socket.Send(msg);
        }
        
        public void SendRequest(string method)
        {
            SendRequest<Dictionary<string, object>>(method, new Dictionary<string, object>());
        }

        public TResult SendRequest<TResult>(string method) where TResult : class
        {           
            return SendRequest<TResult>(method, new Dictionary<string, object>());
        }

        public TResult SendRequest<TResult>(string method, object parameters) where TResult : class
        {
            var request = new RpcRequest(method, parameters);
            var strResponse = SendRpcRequest(Serialization.ToJson(request));
            var response = Serialization.FromJson<RpcResponse<TResult>>(strResponse);
            if (response.Error != null)
            {
                throw new JsonRpcException(response.Error.Code, response.Error.Message);                    
            }
            else
            {
                return response.Result;
            }
        }

        public void SendNotification(string method)
        {
            SendNotification(method, new Dictionary<string, object>());
        }

        public void SendNotification(string method, object parameters)
        {
            var str = Serialization.ToJson(new RpcNotification(method, parameters));
            SendRpcNotification(str);
        }
    }
}
