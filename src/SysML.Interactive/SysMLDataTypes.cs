using System.Collections.Generic;

namespace SysML.Interactive
{
    public enum IssueSeverity { ERROR, WARNING, INFO, IGNORE };

    public enum CheckType { FAST, NORMAL, EXPENSIVE };

    public enum SysMLElementKind { PACKAGE, CLASS, NAMESPACE, ATTRIBUTE_DEFINITION, PART_DEFINITION, PART_USAGE, ATTRIBUTE_USAGE, ELEMENT };

    public record SysMLIssue(
            int Offset,
            int Length,
            int LineNumber,
            int Column,
            int LineNumberEnd,
            int ColumnEnd,
            string Code,
            string Message,
            bool IsSyntaxError,
            IssueSeverity Severity,
            CheckType Type,
            IEnumerable<string> Data
        );

    public record SysMLElement(
            string Name,
            SysMLElementKind Kind,
            IEnumerable<SysMLElement> OwnedElements,
            string Type
        );

    public record SysMLInteractiveResult(
            IEnumerable<SysMLIssue> Issues,
            IEnumerable<SysMLIssue> SyntaxErrors,
            IEnumerable<SysMLIssue> SemanticErrors,
            IEnumerable<SysMLIssue> Warnings,
            IEnumerable<SysMLElement> Content
        );
}