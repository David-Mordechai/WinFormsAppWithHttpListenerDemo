using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WinFormsApp.Services
{
    internal class HttpGetBitStatusService<T>
    {
        private readonly int _port;
        private Func<string, T> _powerBitCallBack;
        private Func<string, T> _continuousBitCallBack;
        private readonly HttpListener _listener = new HttpListener();
        private readonly BlockingCollection<HttpListenerContext> _queue = new BlockingCollection<HttpListenerContext>();

        public HttpGetBitStatusService(int port)
        {
            _port = port;
        }

        public void Start(Func<string, T> powerBitCallBack, Func<string, T> continuousBitCallBack)
        {
            _powerBitCallBack = powerBitCallBack;
            _continuousBitCallBack = continuousBitCallBack;
            _listener.Prefixes.Add($"http://localhost:{_port}/powerBit/");
            _listener.Prefixes.Add($"http://localhost:{_port}/continuousBit/");
            _listener.Start();
            Task.Factory.StartNew(ProcessRequests);
            _listener.BeginGetContext(GetContextCallback, null);
        }

        private void GetContextCallback(IAsyncResult ar)
        {
            var context = _listener.EndGetContext(ar);
            _queue.Add(context);
            _listener.BeginGetContext(GetContextCallback, null);
        }

        private void ProcessRequests()
        {
            foreach (var context in _queue.GetConsumingEnumerable())
            {
                try
                {
                    var request = context.Request;
                    var response = context.Response;
                    if (!request.Url.AbsolutePath.EndsWith("/"))
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Close();
                        continue;
                    }
                    var port = request.QueryString["port"];
                    T result;
                    switch (request.Url.AbsolutePath)
                    {
                        case "/powerBit/":
                            result = _powerBitCallBack.Invoke(port);
                            break;
                        case "/continuousBit/":
                            result = _continuousBitCallBack.Invoke(port);
                            break;
                        default:
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            response.Close();
                            continue;
                    }
                    var json = JsonConvert.SerializeObject(result);
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.ContentType = "application/json";
                    var buffer = Encoding.UTF8.GetBytes(json);
                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                catch
                {
                    var response = context.Response;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Close();
                }
            }
        }
    }
}