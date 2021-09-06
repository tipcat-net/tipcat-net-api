using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace TipCatDotNet.ApiTests.Utils
{
    public class MockHttpProvider : IHttpProvider
    {
        public void Dispose() { }


        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            => Task.Run(() =>
            {
                var key = "GET:" + request.RequestUri;
                var response = new HttpResponseMessage();

                if (OnRequestExecuting != null)
                {
                    var args = new MockRequestExecutingEventArgs(request);
                    OnRequestExecuting.Invoke(this, args);

                    if (args.Result != null)
                        response.Content = new StringContent(Serializer.SerializeObject(args.Result));
                }

                if (Responses.ContainsKey(key) && response.Content == null)
                    response.Content = new StringContent(Serializer.SerializeObject(Responses[key]));

                return response;
            });


        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken) 
            => SendAsync(request);

        
 
        public event EventHandler<MockRequestExecutingEventArgs> OnRequestExecuting;

        public TimeSpan OverallTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public Dictionary<string, object> Responses { get; set; } = new();
        public ISerializer Serializer { get; } = new Serializer();
    }
}
