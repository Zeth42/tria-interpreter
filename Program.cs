using System;
using System.IO;
using System.Text;

namespace Tria.Compiler.Lexer
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            PrintBanner();

            string? filePath = null;

            if (args.Length > 0)
            {
                filePath = args[0];
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Usage: tria-lexer <file.tr>");
                Console.ResetColor();
                Console.WriteLine("Enter the path to your .tr file (or press ENTER to run 'ejemplo.tr'):\n");

                Console.Write("File path: ");
                string? input = Console.ReadLine()?.Trim('"', ' ', '\'');

                if (string.IsNullOrWhiteSpace(input))
                {
                    filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ejemplo.tr");
                    if (!File.Exists(filePath))
                    {
                        filePath = "ejemplo.tr";
                    }
                }
                else
                {
                    filePath = input;
                }
            }

            if (!File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"error: file not found: '{filePath}'");
                Console.ResetColor();
                return 1;
            }

            if (!filePath.EndsWith(".tr", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"warning: file '{filePath}' does not have the standard '.tr' extension");
                Console.ResetColor();
            }

            string fileName = Path.GetFileName(filePath);
            string sourceCode = File.ReadAllText(filePath, Encoding.UTF8);

            // Execute Lexical Analyzer
            var lexer = new TriaLexer(sourceCode, fileName);
            var (tokens, diagnostics) = lexer.Tokenize();

            // Print Tokens Stream Table
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"{"LOC",-9} {"TOKEN TYPE",-16} {"LEXEME",-26} {"LITERAL"}");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.ResetColor();

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.EndOfFile)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(token.ToString());
                    Console.ResetColor();
                    continue;
                }

                Console.WriteLine(token.ToString());
            }

            Console.WriteLine("----------------------------------------------------------------------\n");

            // Diagnostic Reports
            if (diagnostics.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"success: tokenized {tokens.Count} tokens from '{fileName}' with 0 errors.");
                Console.ResetColor();
                return 0;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"error: scanning '{fileName}' failed with {diagnostics.Count} error(s):\n");
            Console.ResetColor();

            foreach (var diag in diagnostics)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(diag.Format());
                Console.ResetColor();
                Console.WriteLine();
            }

            return 1;
        }

        private static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("TRIA Language Lexical Analyzer (v0.2)");
            Console.WriteLine("Compiler Frontend Pipeline: Source -> Tokens\n");
            Console.ResetColor();
        }
    }
}
