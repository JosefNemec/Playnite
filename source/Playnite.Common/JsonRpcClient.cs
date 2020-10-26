using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class JsonRpcResponse<TResult> : RpcMessage where TResult : class
    {
        [JsonProperty(PropertyName = "result")]
        public new TResult Result;
    }

    public class JsonRpcResponse : RpcMessage
    {
    }

    public class JsonRpcRequest : RpcMessage
    {
        private static Random rndGen = new Random();

        public JsonRpcRequest()
        {
        }

        public JsonRpcRequest(string method) : this(method, null)
        {
        }

        public JsonRpcRequest(string method, object parameter)
        {
            Method = method;
            Params = parameter;
            Id = rndGen.Next();
        }
    }

    public class JsonRpcNotification : JsonRpcRequest
    {
        public JsonRpcNotification()
        {
        }

        public JsonRpcNotification(string method) : this(method, null)
        {
        }

        public JsonRpcNotification(string method, object parameter) : base(method, parameter)
        {
            Id = null;
        }
    }
    public class JsonRpcResponseError
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
        public JsonRpcResponseError Error;

        public TParams GetParams<TParams>() where TParams : class
        {
            return Serialization.FromJson<TParams>(Params.ToString());
        }
    }

    public class JsonRpcNotificationEventArgs : EventArgs
    {
        public JsonRpcNotification Notification { get; set; }
    }

    public class JsonRpcRequestEventArgs : EventArgs
    {
        public JsonRpcRequest Request { get; set; }
    }

    public class JsonRpcException : Exception
    {
        public int ErrorCode { get; set; }

        public JsonRpcException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class JsonRpcClient : IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly Socket socket;
        private ConcurrentDictionary<int, string> requestResponses = new ConcurrentDictionary<int, string>();

        public event EventHandler<JsonRpcRequestEventArgs> RequestReceived;
        public event EventHandler<JsonRpcNotificationEventArgs> NotificationReceived;

        public JsonRpcClient(string address)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var split = address.Split(new char[] { ':' });
            socket.Connect(split[0], Convert.ToInt32(split[1]));
            StartListening();
        }

        private Task StartListening()
        {
            return Task.Run(async () =>
            {
                byte[] dataBuffer = new byte[1024];
                var procData = string.Empty;
                TaskCompletionSource<bool> complSource = null;
                var args = new SocketAsyncEventArgs();
                args.SetBuffer(dataBuffer, 0, 1024);
                args.Completed += (_, __) => complSource.SetResult(true);

                while (true)
                {
                    if (!socket.Connected)
                    {
                        break;
                    }

                    complSource = new TaskCompletionSource<bool>();
                    if (socket.ReceiveAsync(args))
                    {
                        await complSource.Task;
                    }

                    if (args.BytesTransferred > 0)
                    {
                        var data = encoding.GetString(dataBuffer, 0, args.BytesTransferred);
                        procData += data;
                        var termIndex = procData.IndexOf('\n');
                        if (termIndex == -1)
                        {
                            continue;
                        }

                        var datas = procData.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        if (datas.Count() == 1)
                        {
                            DataReceived(datas[0]);
                            procData = string.Empty;
                        }
                        else
                        {
                            if (procData[procData.Length - 1] == '\n')
                            {
                                foreach (var dta in datas)
                                {
                                    DataReceived(dta);
                                }

                                procData = string.Empty;
                            }
                            else
                            {
                                for (int i = 0; i < datas.Count() - 2; i++)
                                {
                                    DataReceived(datas[i]);
                                }

                                procData = datas[datas.Count() - 1];
                            }
                        }
                    }
                }
            });
        }

        private void DataReceived(string data)
        {
            var request = Serialization.FromJson<RpcMessage>(data);
            if (request.Id != null && string.IsNullOrEmpty(request.Method))
            {
                // Response
                requestResponses.TryAdd(request.Id.Value, data);
            }
            else if (request.Id != null && !string.IsNullOrEmpty(request.Method))
            {
                // Request
                RequestReceived?.BeginInvoke(this, new JsonRpcRequestEventArgs()
                {
                    Request = Serialization.FromJson<JsonRpcRequest>(data)
                }, EventEndCallback<JsonRpcRequestEventArgs>, null);
            }
            else if (request.Id == null)
            {
                // Notification
                NotificationReceived?.BeginInvoke(this, new JsonRpcNotificationEventArgs()
                {
                    Notification = Serialization.FromJson<JsonRpcNotification>(data)
                }, EventEndCallback<JsonRpcNotificationEventArgs>, null);
            }
            else
            {
                logger.Error("Recevied invalid RPC message:");
                logger.Error(data);
            }
        }

        private void EventEndCallback<T>(IAsyncResult result)
        {
            var ar = (System.Runtime.Remoting.Messaging.AsyncResult)result;
            var invokedMethod = (EventHandler<T>)ar.AsyncDelegate;

            try
            {
                invokedMethod.EndInvoke(result);
            }
            catch (Exception e)
            {
                logger.Error(e, $"JsonRpcClient event listener failed.");
            }
        }

        public void Dispose()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            RequestReceived = null;
            NotificationReceived = null;
        }

        private string SendRpcRequest(int requestId, string data)
        {
            SendData(data);
            while (true)
            {
                if (requestResponses.TryGetValue(requestId, out var response))
                {
                    return response;
                }

                Thread.Sleep(50);
            }

            throw new JsonRpcException(0, "");
        }

        private int SendData(string data)
        {
            byte[] msg = encoding.GetBytes(data + '\n');
            return socket.Send(msg);
        }

        public void SendRequest(string method)
        {
            SendRequest<Dictionary<string, object>>(method, new Dictionary<string, object>());
        }

        public void SendRequest(string method, object parameters)
        {
            SendRequest<Dictionary<string, object>>(method, parameters);
        }

        public TResult SendRequest<TResult>(string method) where TResult : class
        {
            return SendRequest<TResult>(method, new Dictionary<string, object>());
        }

        public TResult SendRequest<TResult>(string method, object parameters) where TResult : class
        {
            var request = new JsonRpcRequest(method, parameters);
            var strResponse = SendRpcRequest(request.Id.Value, Serialization.ToJson(request));
            var response = Serialization.FromJson<JsonRpcResponse<TResult>>(strResponse);
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
            var str = Serialization.ToJson(new JsonRpcNotification(method, parameters));
            SendData(str);
        }

        public void SendResponse(JsonRpcRequest request, object response)
        {
            var str = Serialization.ToJson(new JsonRpcResponse
            {
                Id = request.Id,
                Result = response
            });

            SendData(str);
        }
    }
}
