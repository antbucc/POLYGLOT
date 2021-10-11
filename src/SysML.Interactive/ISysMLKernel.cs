using StreamJsonRpc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SysML.Interactive
{
    public interface ISysMLKernel
    {
        [JsonRpcMethod("eval")]
        public Task<SysMLInteractiveResult> EvalAsync(string input);

        [JsonRpcMethod("getSvg")]
        public Task<string> GetSvgAsync(IEnumerable<string> names, IEnumerable<string> views, IEnumerable<string> styles, IEnumerable<string> help);
    }
}
