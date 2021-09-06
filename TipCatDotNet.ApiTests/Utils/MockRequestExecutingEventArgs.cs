using System.Net.Http;

namespace TipCatDotNet.ApiTests.Utils
{
    public class MockRequestExecutingEventArgs
    {
        public MockRequestExecutingEventArgs(HttpRequestMessage message)
        {
            RequestMessage = message;
        }


        public HttpRequestMessage RequestMessage { get; }
        public object Result { get; set; }
    }
}
