using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;

namespace Polyglot.Core
{
    public abstract class LanguageEngine
    {
        public abstract Task<bool> TryInstallForAsync(Kernel kernel);
    }
}