namespace Tria.Compiler.Lexer
{
    /// <summary>
    /// Represents an atomic lexical unit (Token) emitted by the scanner.
    /// </summary>
    /// <param name="Type">The formal category of the token.</param>
    /// <param name="Lexeme">The exact source text matched.</param>
    /// <param name="Literal">Evaluated runtime value (int, double, string value).</param>
    /// <param name="Line">1-based line index in the source file.</param>
    /// <param name="Column">1-based column index in the source file.</param>
    public record Token(TokenType Type, string Lexeme, object? Literal, int Line, int Column)
    {
        public override string ToString()
        {
            string loc = $"{Line}:{Column}";
            string literalDisplay = Literal is not null ? $" -> {Literal}" : string.Empty;
            return $"{loc,-8} {Type,-16} \"{Escape(Lexeme)}\"{literalDisplay}";
        }

        private static string Escape(string text) =>
            text.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }
}
