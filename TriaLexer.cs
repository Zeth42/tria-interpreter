using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tria.Compiler.Lexer
{
    /// <summary>
    /// Lexical Analyzer for the TRIA programming language (.tr).
    /// Scans a character stream into a sequence of strongly-typed Tokens.
    /// </summary>
    public class TriaLexer
    {
        private readonly string _source;
        private readonly string _fileName;
        private readonly string[] _sourceLines;
        private readonly List<Token> _tokens = new();
        private readonly List<Diagnostic> _diagnostics = new();

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
        private int _column = 1;
        private int _startColumn = 1;

        // Reserved keywords table for TRIA
        private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.Ordinal)
        {
            ["var"] = TokenType.Var,
            ["const"] = TokenType.Const,
            ["fun"] = TokenType.Fun,
            ["return"] = TokenType.Return,
            ["repeat"] = TokenType.Repeat,
            ["foreach"] = TokenType.Foreach,
            ["in"] = TokenType.In,
            ["end"] = TokenType.End,
            ["write"] = TokenType.Write,
            ["read"] = TokenType.Read,
            ["int"] = TokenType.Int,
            ["string"] = TokenType.StringType,
            ["numeric"] = TokenType.Numeric,
            ["dict"] = TokenType.Dict
        };

        public TriaLexer(string source, string fileName = "source.tr")
        {
            _source = source ?? string.Empty;
            _fileName = fileName;
            _sourceLines = _source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// Scans the entire source code and returns tokens and diagnostic reports.
        /// </summary>
        public (List<Token> Tokens, List<Diagnostic> Diagnostics) Tokenize()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                _startColumn = _column;
                ScanToken();
            }

            // Sentinel End-Of-File token for the Parser
            _tokens.Add(new Token(TokenType.EndOfFile, string.Empty, null, _line, _column));
            return (_tokens, _diagnostics);
        }

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                // ==========================================
                // 1. DELIMITERS AND PUNCTUATION (Single-char)
                // ==========================================
                case '(': AddToken(TokenType.LeftParen); break;
                case ')': AddToken(TokenType.RightParen); break;
                case '{': AddToken(TokenType.LeftBrace); break;
                case '}': AddToken(TokenType.RightBrace); break;
                case '[': AddToken(TokenType.LeftBracket); break;
                case ']': AddToken(TokenType.RightBracket); break;
                case ';': AddToken(TokenType.Semicolon); break;
                case ',': AddToken(TokenType.Comma); break;
                case ':': AddToken(TokenType.Colon); break;
                case '.': AddToken(TokenType.Dot); break;
                case '?': AddToken(TokenType.Question); break;

                // ==========================================
                // 2. ARITHMETIC OPERATORS & LOOKAHEAD
                // ==========================================
                case '+':
                    AddToken(Match('+') ? TokenType.Increment : TokenType.Plus);
                    break;

                case '-':
                    AddToken(TokenType.Minus);
                    break;

                case '*':
                    AddToken(Match('*') ? TokenType.Power : TokenType.Multiply);
                    break;

                case '%':
                    if (Match('%'))
                    {
                        AddToken(TokenType.Modulo);
                    }
                    else
                    {
                        ReportDiagnostic(
                            "TRIA-E1002",
                            "incomplete modulo operator '%'",
                            hint: "did you mean '%%'? In TRIA modulo requires '%%'");
                    }
                    break;

                // ==========================================
                // 3. RELATIONAL, ASSIGNMENT & ARROW '=>'
                // ==========================================
                case '=':
                    if (Match('='))
                        AddToken(TokenType.EqualEqual);      // ==
                    else if (Match('>'))
                        AddToken(TokenType.Arrow);           // =>
                    else
                        AddToken(TokenType.Equal);           // =
                    break;

                case '!':
                    AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang); // != vs !
                    break;

                case '<':
                    AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less); // <= vs <
                    break;

                case '>':
                    AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); // >= vs >
                    break;

                // ==========================================
                // 4. LOGICAL OPERATORS (&&, ||)
                // ==========================================
                case '&':
                    if (Match('&'))
                    {
                        AddToken(TokenType.And);
                    }
                    else
                    {
                        ReportDiagnostic(
                            "TRIA-E1003",
                            "unexpected character '&'",
                            hint: "expected '&&' for logical AND");
                    }
                    break;

                case '|':
                    if (Match('|'))
                    {
                        AddToken(TokenType.Or);
                    }
                    else
                    {
                        ReportDiagnostic(
                            "TRIA-E1004",
                            "unexpected character '|'",
                            hint: "expected '||' for logical OR");
                    }
                    break;

                // ==========================================
                // 5. DIVISION VS COMMENTS (// and /// ... ///)
                // ==========================================
                case '/':
                    if (Match('/'))
                    {
                        if (Match('/'))
                        {
                            ScanBlockComment(); // TRIA block comment: /// ... ///
                        }
                        else
                        {
                            ScanLineComment();  // Single-line comment: // ...
                        }
                    }
                    else
                    {
                        AddToken(TokenType.Divide); // Simple division operator
                    }
                    break;

                // ==========================================
                // 6. WHITESPACE & NEWLINES
                // ==========================================
                case ' ':
                case '\r':
                case '\t':
                    // Non-semantic whitespace: ignore
                    break;

                case '\n':
                    _line++;
                    _column = 1;
                    break;

                // ==========================================
                // 7. STRING LITERALS ("..." or '...')
                // ==========================================
                case '"':
                case '\'':
                    ScanString(c);
                    break;

                // ==========================================
                // 8. IDENTIFIERS, KEYWORDS & NUMBERS
                // ==========================================
                default:
                    if (char.IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifierOrKeyword();
                    }
                    else
                    {
                        ReportDiagnostic(
                            "TRIA-E1001",
                            $"unexpected character '{c}'",
                            hint: "character not recognized in TRIA grammar");
                    }
                    break;
            }
        }

        // ==========================================================
        // SCAN IDENTIFIERS AND KEYWORDS
        // ==========================================================

        private void ScanIdentifierOrKeyword()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            string text = _source[_start.._current];
            if (Keywords.TryGetValue(text, out TokenType keywordType))
            {
                AddToken(keywordType);
            }
            else
            {
                AddToken(TokenType.Identifier, text);
            }
        }

        // ==========================================================
        // SCAN NUMERIC LITERALS (Integer and Floating-point)
        // ==========================================================

        private void ScanNumber()
        {
            while (char.IsDigit(Peek()))
            {
                Advance();
            }

            // Look for a fractional decimal part (e.g. 3.14)
            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                Advance(); // Consume '.'

                while (char.IsDigit(Peek()))
                {
                    Advance();
                }
            }

            string numText = _source[_start.._current];
            if (double.TryParse(numText, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                AddToken(TokenType.Number, value);
            }
            else
            {
                ReportDiagnostic(
                    "TRIA-E1007",
                    $"invalid numeric literal '{numText}'",
                    hint: "number format cannot be parsed");
            }
        }

        // ==========================================================
        // SCAN STRING LITERALS
        // ==========================================================

        private void ScanString(char quoteChar)
        {
            int startLine = _line;
            int startCol = _startColumn;

            while (Peek() != quoteChar && !IsAtEnd())
            {
                if (Peek() == '\n')
                {
                    _line++;
                    _column = 0;
                }

                // Handle escaped quotes: \" or \' or \\
                if (Peek() == '\\' && (PeekNext() == quoteChar || PeekNext() == '\\'))
                {
                    Advance(); // consume '\'
                }

                Advance();
            }

            if (IsAtEnd())
            {
                ReportDiagnostic(
                    "TRIA-E1005",
                    "unterminated string literal",
                    startLine,
                    startCol,
                    hint: $"expected closing quote '{quoteChar}' before end of file");
                return;
            }

            Advance(); // Consume closing quote

            // Unquoted string content
            string content = _source[(_start + 1)..(_current - 1)];
            AddToken(TokenType.String, content);
        }

        // ==========================================================
        // SCAN COMMENTS
        // ==========================================================

        private void ScanLineComment()
        {
            while (Peek() != '\n' && !IsAtEnd())
            {
                Advance();
            }
            // Discard comment content: no token emitted to parser
        }

        private void ScanBlockComment()
        {
            int openLine = _line;
            int openCol = _startColumn;

            while (!IsAtEnd())
            {
                if (Peek() == '\n')
                {
                    _line++;
                    _column = 0;
                }

                // Check for TRIA triple slash closing delimiter: '///'
                if (Peek() == '/' && PeekNext() == '/' && PeekNext(2) == '/')
                {
                    Advance();
                    Advance();
                    Advance();
                    return; // Successfully closed
                }

                Advance();
            }

            ReportDiagnostic(
                "TRIA-E1006",
                "unterminated block comment",
                openLine,
                openCol,
                hint: "expected '///' delimiter before end of file");
        }

        // ==========================================================
        // LOOKAHEAD AND CURSOR UTILITIES
        // ==========================================================

        private bool IsAtEnd() => _current >= _source.Length;

        private char Advance()
        {
            char current = _source[_current++];
            _column++;
            return current;
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            _column++;
            return true;
        }

        private char Peek() => IsAtEnd() ? '\0' : _source[_current];

        private char PeekNext(int offset = 1)
        {
            if (_current + offset >= _source.Length) return '\0';
            return _source[_current + offset];
        }

        private static bool IsAlpha(char c) =>
            (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

        private static bool IsAlphaNumeric(char c) =>
            IsAlpha(c) || char.IsDigit(c);

        private void AddToken(TokenType type, object? literal = null)
        {
            string text = _source[_start.._current];
            _tokens.Add(new Token(type, text, literal, _line, _startColumn));
        }

        private void ReportDiagnostic(string code, string message, string? hint = null) =>
            ReportDiagnostic(code, message, _line, _startColumn, hint);

        private void ReportDiagnostic(string code, string message, int line, int col, string? hint = null)
        {
            string? lineContent = (line >= 1 && line <= _sourceLines.Length)
                ? _sourceLines[line - 1]
                : null;

            _diagnostics.Add(new Diagnostic(
                code,
                DiagnosticSeverity.Error,
                message,
                _fileName,
                line,
                col,
                lineContent,
                hint));
        }
    }
}
