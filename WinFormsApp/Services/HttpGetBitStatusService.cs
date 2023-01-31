using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WinFormsApp.Services
{
    internal class HttpGetBitStatusService<T>
    {
        private readonly int _port;
        private Func<string, T> _callBack;
        private readonly HttpListener _listener = new HttpListener();

        public HttpGetBitStatusService(int port)
        {
            _port = port;
        }

        public void Start(Func<string, T> callBack)
        {
            _callBack = callBack;
            _listener.Prefixes.Add($"http://localhost:{_port}/");
            _listener.Start();
            _listener.GetContextAsync().ContinueWith(ProcessRequest);
        }

        private void ProcessRequest(Task<HttpListenerContext> task)
        {
            try
            {
                var context = task.Result;
                var request = context.Request;
                var response = context.Response;
                var port = request.QueryString["port"];
                var result = _callBack.Invoke(port);
                var json = JsonConvert.SerializeObject(result);
                response.StatusCode = 200;
                response.ContentType = "application/json";
                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch
            {
                var response = task.Result.Response;
                response.StatusCode = 500;
                response.ContentType = "application/json";
                var buffer = Encoding.UTF8.GetBytes("{\"error\":\"An error occurred\"}");
                response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            finally
            {
                _listener.GetContextAsync().ContinueWith(ProcessRequest);
            }
        }
    }
}