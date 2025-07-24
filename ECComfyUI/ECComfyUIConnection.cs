using ECComfyUI.Models;
using ECComfyUI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ECComfyUI
{
    public class ECComfyUIConnection : IDisposable
    {
        #region EvenHandlers

        public delegate void ECCUIEventConnected(ECCUIStatus _statusMessage);
        public event ECCUIEventConnected OnConnected;

        public delegate void ECCUIEventStatusUpdate(ECCUIStatus _statusMessage);
        public event ECCUIEventStatusUpdate OnStatusUpdate;

        public delegate void ECCUIEventExecutionStart(ECCUIExecutionStart _statusMessage);
        public event ECCUIEventExecutionStart OnExecutionStart;

        public delegate void ECCUIEventExecutionCached(ECCUIExecutionCached _statusMessage);
        public event ECCUIEventExecutionCached OnExecutionCached;

        public delegate void ECCUIEventExecuted(ECCUIExecuted _statusMessage);
        public event ECCUIEventExecuted OnExecuted;

        public delegate void ECCUIEventExecutionSuccess(ECCUIExecutionSuccess _statusMessage);
        public event ECCUIEventExecutionSuccess OnExecutionSuccess;

        public delegate void ECCUIEventExecuting(ECCUIExecuting _statusMessage);
        public event ECCUIEventExecuting OnExecuting;

        public delegate void ECCUIEventProgress(ECCUIProgress _statusMessage);
        public event ECCUIEventProgress OnProgress;

        #endregion

        #region PrivateEnums
        public enum ComfyUIRoutes
        {
            // GET routes
            View,                               // GET /view
            Prompt_Get,                         // GET /prompt
            Queue,                              // GET /queue
            History_Get,                        // GET /history
            History_PromptId,                   // GET /history/{prompt_id}
            /*
            Embeddings,                         // GET /embeddings
            Extensions,                         // GET /extensions
            Workflow_Templates,                 // GET /workflow_templates            
            View_Metadata,                      // GET /view_metadata
            System_Stats,                       // GET /system_stats            
            Object_Info,                        // GET /object_info
            Object_Info_NodeClass,              // GET /object_info/{node_class}
            */

            // POST routes
            Prompt_Post,                        // POST /prompt
            Upload_Image,                        // POST /upload/image
            Queue_Post,                         // POST /queue
            /*
            Upload_Mask,                        // POST /upload/mask
            History_Post,                       // POST /history
            Interrupt,                          // POST /interrupt
            Free                                // POST /free
            */
        }
        #endregion

        #region Privates

        private enum ECCUIHTTPMethod { GET, POST }
        private enum ECCUIEndpoints { }

        private Timer timer;
        private ClientWebSocket webSocket;
        private HttpClient httpClient;
        private int interval = 500;
        private string httpProtocol;
        private string wsProtocol;
        private string hostAndPort;

        #endregion

        public string Sid { get; private set; }

        public ECCUIStatus Connect(string _hostAndPort, bool _useSSL = false)
        {
            wsProtocol = _useSSL ? "wss" : "ws";
            httpProtocol = _useSSL ? "https" : "http";
            hostAndPort = _hostAndPort;

            Uri hostUri = new Uri(String.Format("{0}://{1}/ws", wsProtocol, _hostAndPort));
            webSocket = new ClientWebSocket();
            webSocket.ConnectAsync(hostUri, default).Wait();
            ECCUIStatus status = JsonConvert.DeserializeObject<ECCUIStatus>(receiveFromWebSocket());

            Sid = status.Data.Sid;
            OnConnected?.Invoke(status);

            timer = new Timer(WebSocketTimerTick, null, interval, Timeout.Infinite);

            return status;
        }

        public ECCUIPrompt PromptFromWorkflowFile(string _apiWorkflowFilePath)
        {
            string workflowJSON = File.ReadAllText(_apiWorkflowFilePath); 
            return Prompt(workflowJSON);
        }

        public ECCUIPrompt Prompt(string _apiWorkflowJSON)
        {
            var data = new
            {
                client_id = Sid,
                prompt = JToken.Parse(_apiWorkflowJSON)
            };
            string response = sendHTTPRequest(ECCUIHTTPMethod.POST, ComfyUIRoutes.Prompt_Post, JsonConvert.SerializeObject(data));
            return JsonConvert.DeserializeObject<ECCUIPrompt>(response);
        }

        public ECCUIQueue GetQueue()
        {
            string response = sendHTTPRequest(ECCUIHTTPMethod.GET, ComfyUIRoutes.Queue);
            var ret = ECCUIQueue.Parse(response);
            return ret;
        }

        public void CancelQueue(string _promptId)
        {
            CancelQueue(new string[] { _promptId }.ToList());
        }

        public void CancelQueue(List<string> _promptId)
        {
            string entries = _promptId.Select(id => $"\"{id}\"").Aggregate((current, next) => current + "," + next);
            string data = "{\"delete\": [ENTRIES]}";
            data = data.Replace("ENTRIES", entries);
            sendHTTPRequest(ECCUIHTTPMethod.POST, ComfyUIRoutes.Queue, data);
        }

        public void CancelAllQueued()
        {
            string data = "{\"clear\": \"true\"}";
            sendHTTPRequest(ECCUIHTTPMethod.POST, ComfyUIRoutes.Queue, data);
        }

        public string GetFileAsBase64(string _filename)
        {
            string response = sendHTTPRequest(ECCUIHTTPMethod.GET, ComfyUIRoutes.View, null, "?filename=" + _filename);
            return response;
        }

        public Stream GetFileAsStream(string _filename)
        {
            return new MemoryStream(GetFileAsBytes(_filename));
        }

        public byte[] GetFileAsBytes(string _filename)
        {
            return Convert.FromBase64String(GetFileAsBase64(_filename));
        }

        public void SaveFile(string _filename, string _targetDirectory, string _newFileName = null)
        {
            string filename = _newFileName != null ? _newFileName : _filename;
            using (Stream stream = GetFileAsStream(_filename))
            {
                using (FileStream fileStream = File.Create(Path.Combine(_targetDirectory, _filename)))
                {
                    stream.CopyTo(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
        }

        public void Disconnect()
        {
            this.Dispose();
        }

        #region HTTPCommunication

        private string sendHTTPRequest(ECCUIHTTPMethod _method, ComfyUIRoutes _route, string _data = null, string _subRoute = "")
        {
            string url = $"{httpProtocol}://{hostAndPort}{GetRoutePath(_route, _subRoute)}";
            using (var request = new HttpRequestMessage(new HttpMethod(_method.ToString()), url))
            {
                if (_data != null)
                {
                    request.Content = new StringContent(_data, Encoding.UTF8, "application/json");
                }
                using (httpClient = new HttpClient())
                {
                    var response = httpClient.SendAsync(request).Result;
                    if (response.StatusCode == HttpStatusCode.OK && response.Content.Headers.ContentType != null)
                    {
                        if (!response.Content.Headers.ContentType.ToString().Contains("json"))
                        {
                            byte[] imageContent = response.Content.ReadAsByteArrayAsync().Result; 
                            return Convert.ToBase64String(imageContent);
                        }
                        return response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        ECCUIErrorMessage errorMessage;
                        try
                        {
                            string resp = response.Content.ReadAsStringAsync().Result;
                            errorMessage = JsonConvert.DeserializeObject<ECCUIErrorMessage>(resp);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        throw new ECCUIException("ComfyUI internal error", errorMessage);
                    }
                }
            }
        }

        #endregion

        #region WebSocketCommunication

        private string receiveFromWebSocket()
        {
            var buffer = new byte[1024];
            var result = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), default).Result;
            if (result.MessageType == WebSocketMessageType.Close)
            {
                this.Dispose();
                return null;
            }
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        private void WebSocketTimerTick(object state)
        {
            var bytes = new byte[1024];
            var result = webSocket.ReceiveAsync(new ArraySegment<byte>(bytes), default).Result;

            if (result.MessageType == WebSocketMessageType.Close)
                return;

            string res = Encoding.UTF8.GetString(bytes, 0, result.Count);
            if (res != "")
            {
                JObject resultJson = JObject.Parse(res);
                switch (resultJson.SelectToken("type").ToString())
                { 
                    case "status":
                        ECCUIStatus status = resultJson.ToObject<ECCUIStatus>();
                        if (status != null)
                        {
                            OnStatusUpdate?.Invoke(status);
                        }
                        break;
                    case "execution_start":
                        ECCUIExecutionStart executionStart = resultJson.ToObject<ECCUIExecutionStart>();
                        if (executionStart != null)
                        {
                            OnExecutionStart?.Invoke(executionStart);
                        }
                        break;
                    case "execution_cached":
                        ECCUIExecutionCached executionCached = resultJson.ToObject<ECCUIExecutionCached>();
                        if (executionCached != null)
                        {
                            OnExecutionCached?.Invoke(executionCached);
                        }
                        break;
                    case "executed":
                        ECCUIExecuted executed = resultJson.ToObject<ECCUIExecuted>();
                        if (executed != null)
                        {
                            OnExecuted?.Invoke(executed);
                        }
                        break;
                    case "execution_success":
                        ECCUIExecutionSuccess executionSuccess = resultJson.ToObject<ECCUIExecutionSuccess>();
                        if (executionSuccess != null)
                        {
                            OnExecutionSuccess?.Invoke(executionSuccess);
                        }
                        break;
                    case "executing":
                        ECCUIExecuting executing = resultJson.ToObject<ECCUIExecuting>();
                        if (executing != null)
                        {
                            OnExecuting?.Invoke(executing);
                        }
                        break;
                    case "progress":
                        ECCUIProgress progress = resultJson.ToObject<ECCUIProgress>();
                        if (progress != null)
                        {
                            OnProgress?.Invoke(progress);
                        }
                        break;
                    default:
                        throw new ECCUIException("Unknown message type received from ComfyUI WebSocket", 
                            new ECCUIErrorMessage { Error = new ErrorDetail { Message = "Unknown message type: " + resultJson.SelectToken("type")?.ToString() } });
                }
            }
            timer?.Change(interval, Timeout.Infinite);
        }

        #endregion

        #region Interface

        public void Dispose()
        {
            webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None).Wait();

            webSocket?.Dispose();
            httpClient?.Dispose();
            timer.Dispose();
        }

        #endregion

        #region Helpers

        public static string GetRoutePath(ComfyUIRoutes route, string _subRoute = "")
        {
            return route switch
            {
                ComfyUIRoutes.View => "/view" + _subRoute,
                ComfyUIRoutes.Prompt_Get => "/prompt",
                ComfyUIRoutes.History_Get => "/history",
                ComfyUIRoutes.History_PromptId => "/history/" + _subRoute,
                ComfyUIRoutes.Queue => "/queue",
                ComfyUIRoutes.Upload_Image => "/upload/image",
                ComfyUIRoutes.Prompt_Post => "/prompt",

                /*
                ComfyUIRoutes.Slash => "/",
                ComfyUIRoutes.Embeddings => "/embeddings",
                ComfyUIRoutes.Extensions => "/extensions",
                ComfyUIRoutes.Workflow_Templates => "/workflow_templates",
                ComfyUIRoutes.View_Metadata => "/view_metadata",
                ComfyUIRoutes.System_Stats => "/system_stats",
                ComfyUIRoutes.Object_Info => "/object_info",
                ComfyUIRoutes.Object_Info_NodeClass => "/object_info/"+_subRoute",
                ComfyUIRoutes.Upload_Mask => "/upload/mask",
                ComfyUIRoutes.History_Post => "/history",
                ComfyUIRoutes.Interrupt => "/interrupt",
                ComfyUIRoutes.Free => "/free",
                */

                _ => throw new ArgumentOutOfRangeException(nameof(route), route, null)
            };
        }
        
        #endregion
    }
}
