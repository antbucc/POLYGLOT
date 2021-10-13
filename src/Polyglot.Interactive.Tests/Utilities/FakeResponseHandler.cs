//using Polyglot.Core;
//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Polyglot.Interactive.Tests
//{
//    internal record HttpContentBody(string gameId, string playerId, Dictionary<string, object> data);

//    internal class FakeResponseHandler : DelegatingHandler
//    {
//        private readonly Dictionary<Uri, Func<string, HttpResponseMessage>> _fakeResponses = new();

//        public void AddFakeResponse(Uri uri, Func<string, HttpResponseMessage> responseMessageGenerator)
//        {
//            _fakeResponses.Add(uri, responseMessageGenerator);
//        }

//        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            if (_fakeResponses.ContainsKey(request.RequestUri))
//            {
//                var content = request.Content?.ReadAsStringAsync().Result ?? "";

//                return Task.FromResult(_fakeResponses[request.RequestUri].Invoke(content) ?? new HttpResponseMessage(HttpStatusCode.InternalServerError));
//            }
//            else
//            {
//                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request });
//            }
//        }
//    }
//}
