using Polyglot.Interactive.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Polyglot.Interactive
{
    class FakeExerciseValidator
    {
        private int _currentLevel = 0;

        private static FieldStructure _base = new FieldStructure(new VariableStructure("_base", "float"), new[] { "private" });
        private static FieldStructure _height = new FieldStructure(new VariableStructure("_height", "float"), new[] { "private" });
        private static ConstructorStructure _constructor = new(new[] { new VariableStructure("base", "float"), new VariableStructure("height", "float") });
        private static MethodStructure _calculateArea = new MethodStructure("calculateArea", "float", new[] { "public" }, Array.Empty<VariableStructure>());

        private Dictionary<int, ClassStructure> _expectedResults = new()
        {
            // step 1: class declaration
            { 0, new ClassStructure("Triangle", new[] { "public" }, Array.Empty<FieldStructure>(), Array.Empty<MethodStructure>(), Array.Empty<ConstructorStructure>()) },     
            // step 2: field declaration
            { 1, new ClassStructure("Triangle", new[] { "public" }, new[] { _base, _height }, Array.Empty<MethodStructure>(), Array.Empty<ConstructorStructure>()) },          
            // step 3: constructor
            { 2, new ClassStructure("Triangle", new[] { "public" }, new[] { _base, _height }, Array.Empty<MethodStructure>(), new[] { _constructor }) },          
            // step 4: Area method
            { 3, new ClassStructure("Triangle", new[] { "public" }, new[] { _base, _height }, new[] { _calculateArea }, new[] { _constructor }) },                
        };

        public Task<HttpResponseMessage> PostAsync(Dictionary<string, object> data)
        {
            CheckSubmission(data["classesStructure"] as ClassStructure[]);
            
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }

        private bool CheckSubmission(ClassStructure[] data)
        {
            if (_currentLevel == 4) return false;
            if (data is null || data.Length == 0) return false;

            var expected = _expectedResults[_currentLevel];

            if (expected.Equals(data[0]))
            {
                _currentLevel++;
                return true;
            }
            return false;
        }

        public Task<GameStateReport> GetReportAsync()
        {
            return Task.FromResult(new GameStateReport(
                    _currentLevel.ToString(),
                    _currentLevel * 10,
                    _currentLevel,
                    0,
                    0
                ));
        }
    }
}
