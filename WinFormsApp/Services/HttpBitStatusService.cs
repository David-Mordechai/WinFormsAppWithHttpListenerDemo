using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WinFormsApp.Dtos;

namespace WinFormsApp.Services
{
    internal class HttpBitStatusService
    {
        private readonly string _url;
        private Func<string, BitStatusDto> _powerBitCallBack;
        private Func<string, BitStatusDto> _continuousBitCallBack;
        private readonly HttpListener _listener = new HttpListener();
        private readonly BlockingCollection<HttpListenerContext> _queue = new BlockingCollection<HttpListenerContext>();

        public HttpBitStatusService(string url)
        {
            _url = url;
        }

        public void Start(Func<string, BitStatusDto> powerBitCallBack, Func<string, BitStatusDto> continuousBitCallBack)
        {
            _powerBitCallBack = powerBitCallBack;
            _continuousBitCallBack = continuousBitCallBack;
            _listener.Prefixes.Add($"{_url}/powerBit/");
            _listener.Prefixes.Add($"{_url}/continuousBit/");
            _listener.Start();
            Task.Factory.StartNew(ProcessRequests);
            _listener.BeginGetContext(GetContextCallback, null);
        }

        private void GetContextCallback(IAsyncResult asyncResult)
        {
            var context = _listener.EndGetContext(asyncResult);
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

                    string json;
                    var port = request.QueryString["port"];

                    switch (request.Url.AbsolutePath)
                    {
                        case "/powerBit/":
                            json = JsonConvert.SerializeObject(_powerBitCallBack.Invoke(port));
                            break;
                        case "/continuousBit/":
                            json = JsonConvert.SerializeObject(_continuousBitCallBack.Invoke(port));
                            break;
                        default:
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            response.Close();
                            continue;
                    }
                    
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