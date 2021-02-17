namespace Polyglot.Interactive.Tests.Utilities
{
    public static class LanguageExtensions
    {
        public static string LanguageName(this Language language)
        {
            return language switch
            {
                Language.CSharp => "csharp",
                Language.FSharp => "fsharp",
                Language.PowerShell => "pwsh",
                _ => null
            };
        }
    }
}