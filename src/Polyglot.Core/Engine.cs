using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;

namespace Polyglot.Core
{
    public class Engine : IDisposable
    {
        private readonly ConcurrentDictionary<string, Kernel> _kernels = new();
        private readonly ConcurrentDictionary<Type, LanguageEngine> _languageEngines = new();
        private static Engine _instance;

        public static void Reset()
        {
            GameEngineClient.Reset();
            _instance?.Dispose();
            _instance = null;
        }

        public void RegisterKernel(Kernel kernel)
        {
            _kernels.AddOrUpdate(
                kernel.Name, 
                AddValueFactory,
                UpdateValueFactory);

            Kernel UpdateValueFactory(string kernelName, Kernel oldKernel)
            {
                foreach (var languageEngine in _languageEngines.Values)
                {
                    languageEngine.TryInstallForAsync(kernel).Wait(TimeSpan.FromSeconds(5));
                }

                return kernel;

            }

            Kernel AddValueFactory(string kernelName)
            {
                foreach (var languageEngine in _languageEngines.Values)
                {
                    languageEngine.TryInstallForAsync(kernel).Wait(TimeSpan.FromSeconds(5));
                }

                return kernel;
            }
        }

        public static Engine Instance { get; } = _instance??= new();
        
        private Engine()
        {
            
        }

        public async Task InstallLanguageEngineAsync(LanguageEngine languageEngine)
        {
            if (languageEngine == null)
            {
                throw new ArgumentNullException(nameof(languageEngine));
            }
         
            _languageEngines.AddOrUpdate(
                languageEngine.GetType(),
                AddValueFactory,
                UpdateValueFactory
            );

            LanguageEngine UpdateValueFactory(Type key, LanguageEngine oldLanguageEngine)
            {
                foreach (var kernel in _kernels.Values)
                {
                    languageEngine.TryInstallForAsync(kernel).Wait(TimeSpan.FromSeconds(5));
                }
                return languageEngine;
            }

            LanguageEngine AddValueFactory(Type key)
            {
                foreach (var kernel in _kernels.Values)
                {
                    languageEngine.TryInstallForAsync(kernel).Wait(TimeSpan.FromSeconds(5));
                }

                return languageEngine;
            }
        }

        public void Dispose()
        {
            _kernels.Clear();
            _languageEngines.Clear();
        }
    }
}