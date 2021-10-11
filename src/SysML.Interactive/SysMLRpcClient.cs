using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SysML.Interactive
{
    public class SysMLRpcClient : ISysMLKernel
    {
        private ISysMLKernel jsonRpcServer { get; set; }

        public SysMLRpcClient(Stream toServerStream, Stream fromServerStream)
        {
            var jsonFormatter = new JsonMessageFormatter();
            jsonFormatter.JsonSerializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonFormatter.JsonSerializer.Converters.Add(new StringEnumConverter());
            var messageHandler = new NewLineDelimitedMessageHandler(toServerStream, fromServerStream, jsonFormatter);
            jsonRpcServer = JsonRpc.Attach<ISysMLKernel>(messageHandler);
        }

        public Task<SysMLInteractiveResult> EvalAsync(string input)
        {
            return jsonRpcServer.EvalAsync(input);
        }

        public Task<string> GetSvgAsync(IEnumerable<string> names, IEnumerable<string> views, IEnumerable<string> styles, IEnumerable<string> help)
        {
            return jsonRpcServer.GetSvgAsync(names, views, styles, help);
        }
    }
}
