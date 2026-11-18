namespace Tria.Compiler.Lexer
{
    /// <summary>
    /// Categorías léxicas (Tokens) del lenguaje de programación TRIA (.tr).
    /// </summary>
    public enum TokenType
    {
        // ==========================================
        // 1. DELIMITADORES Y SÍMBOLOS DE AGRUPACIÓN
        // ==========================================
        LeftParen,      // (
        RightParen,     // )
        LeftBrace,      // {
        RightBrace,     // }
        LeftBracket,    // [
        RightBracket,   // ]
        Semicolon,      // ;
        Comma,          // ,
        Colon,          // :
        Dot,            // . (acceso a miembros/propiedades como name.key)

        // ==========================================
        // 2. OPERADORES ARITMÉTICOS
        // ==========================================
        Plus,           // +
        Increment,      // ++
        Minus,          // -
        Multiply,       // *
        Power,          // ** (potencia en TRIA)
        Divide,         // /
        Modulo,         // %% (módulo / residuo en TRIA)

        // ==========================================
        // 3. OPERADORES RELACIONALES Y ASIGNACIÓN
        // ==========================================
        Equal,          // =
        EqualEqual,     // ==
        Arrow,          // => (retorno de función en TRIA)
        Bang,           // !
        BangEqual,      // !=
        Less,           // <
        LessEqual,      // <=
        Greater,        // >
        GreaterEqual,   // >=

        // ==========================================
        // 4. OPERADORES LÓGICOS Y CONTROL
        // ==========================================
        And,            // &&
        Or,             // ||
        Question,       // ? (inicio de bifurcación condicional en TRIA)

        // ==========================================
        // 5. PALABRAS RESERVADAS (KEYWORDS) DE TRIA
        // ==========================================
        Var,            // var
        Const,          // const
        Fun,            // fun
        Return,         // return
        Repeat,         // repeat (sustituto formal de while)
        Foreach,        // foreach
        In,             // in
        End,            // end (sustituto formal de break)
        Write,          // write (salida a consola)
        Read,           // read (entrada desde consola)

        // Tipos primitivos y compuestos
        Int,            // int
        StringType,     // string
        Numeric,        // numeric
        Dict,           // dict (diccionarios tipados)

        // ==========================================
        // 6. IDENTIFICADORES Y LITERALES
        // ==========================================
        Identifier,     // nombres de variables, funciones, etc.
        Number,         // literales numéricos (enteros y flotantes)
        String,         // literales de cadena ("..." o '...')

        // ==========================================
        // 7. CONTROL DE FLUJO DEL LEXER
        // ==========================================
        EndOfFile       // Fin de archivo de código fuente
    }
}
