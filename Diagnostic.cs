using System;
using System.Text;

namespace Tria.Compiler.Lexer
{
    public enum DiagnosticSeverity
    {
        Error,
        Warning,
        Note
    }

    /// <summary>
    /// Represents a standardized compiler diagnostic adhering to modern compiler standards (Rust / Clang / Python 3.12+).
    /// </summary>
    public record Diagnostic(
        string Code,
        DiagnosticSeverity Severity,
        string Message,
        string FileName,
        int Line,
        int Column,
        string? SourceLine = null,
        string? Hint = null)
    {
        public override string ToString() => Format(useColor: false);

        /// <summary>
        /// Formats the diagnostic with line gutter and caret pointers.
        /// </summary>
        public string Format(bool useColor = false)
        {
            var sb = new StringBuilder();
            string severityLabel = Severity.ToString().ToLowerInvariant();

            // Header line: error[TRIA-E1001]: unexpected character '@'
            sb.AppendLine($"{severityLabel}[{Code}]: {Message}");

            // File position indicator: --> file.tr:line:col
            sb.AppendLine($"  --> {FileName}:{Line}:{Column}");

            if (!string.IsNullOrEmpty(SourceLine))
            {
                string lineStr = Line.ToString();
                int gutterWidth = Math.Max(lineStr.Length, 3);
                string blankGutter = new string(' ', gutterWidth);

                // Code frame with gutter '|'
                sb.AppendLine($" {blankGutter} |");
                sb.AppendLine($" {lineStr.PadLeft(gutterWidth)} | {SourceLine}");

                // Caret line:    |             ^ hint here
                int caretOffset = Math.Max(0, Column - 1);
                string caretPadding = new string(' ', caretOffset);
                string hintSuffix = string.IsNullOrEmpty(Hint) ? "^" : $"^ {Hint}";

                sb.Append($" {blankGutter} | {caretPadding}{hintSuffix}");
            }

            return sb.ToString();
        }
    }
}
