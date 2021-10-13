//using System.Collections.Concurrent;
//using System.Reflection;
//using Pocket;
//using Xunit.Sdk;

//namespace Polyglot.Interactive.Tests.Utilities
//{
//    internal class LogTestNamesToPocketLoggerAttribute : BeforeAfterTestAttribute
//    {
//        private static readonly ConcurrentDictionary<MethodInfo, OperationLogger> _operations = new ConcurrentDictionary<MethodInfo, OperationLogger>();

//        public override void Before(MethodInfo methodUnderTest)
//        {
//            if (methodUnderTest == null)
//            {
//                return;
//            }

//            _operations.TryAdd(
//                methodUnderTest,
//                Logger<LanguageKernelTestBase>.Log.OnEnterAndExit(name: $"{methodUnderTest?.DeclaringType?.Name}.{methodUnderTest.Name}"));
//        }

//        public override void After(MethodInfo methodUnderTest)
//        {
//            if (_operations.TryRemove(methodUnderTest, out var operation))
//            {
//                operation.Dispose();
//            }
//        }
//    }
//}