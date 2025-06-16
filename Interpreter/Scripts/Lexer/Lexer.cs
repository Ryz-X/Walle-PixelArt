using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Interpreter.Errores;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Interpreter.Lexer
{
    public class Lexer
    {
        private string input;
        private int current;
        private int start;
        private int lineNumber;
        private int column;
        private string? file;

        public Lexer(string input, string? file = null)
        {
            this.input = input;
            current = 0;
            lineNumber = 1;
            this.file = file;
        }

        public IEnumerable<Token> Tokenize()
        {
            // Implement logic to read characters and produce tokens
            // Example:
            while (current < input.Length)
            {
                char currentChar = input[current];
                column = start = current;

                if (char.IsWhiteSpace(currentChar))
                {
                    if (currentChar == '\n')
                    {
                        yield return new Token(TokenType.NewLine, "\n", new Localization(lineNumber, column, file));
                        lineNumber++;
                    }
                    current++;
                    continue;
                }

                // ... handle keywords, identifiers
                if (char.IsLetter(currentChar))
                {
                    string identifier = "";
                    while (current < input.Length && (char.IsLetterOrDigit(input[current]) || input[current] == '-'))
                    {
                        if (input[current] == '-' && IsKeyword(identifier)) yield return new Token(TokenType.Keyword, identifier, new Localization(lineNumber, column, file));
                        identifier += input[current];
                        current++;
                    }
                    if (IsKeyword(identifier))
                    {
                        yield return new Token(TokenType.Keyword, identifier, new Localization(lineNumber, column, file));
                    }
                    else
                    {
                        yield return new Token(TokenType.Identifier, identifier, new Localization(lineNumber, column, file));
                    }
                    continue;
                }
                else if (char.IsDigit(currentChar)) //Handle numbers
                {

                    while (char.IsDigit(LookAhead())) Advance();

                    if (LookAhead() == '.' && char.IsDigit(LookAhead(1)))
                    {
                        Advance();
                        while (char.IsDigit(LookAhead())) Advance();
                    }

                    yield return new Token(TokenType.Number, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                }
                else
                {
                    switch (currentChar) //Handle strings, operators, etc.
                    {

                        case '(': yield return new Token(TokenType.Left_Parentesis, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file)); break;
                        case ')': yield return new Token(TokenType.Right_Parentesis, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file)); break;
                        case '[': yield return new Token(TokenType.Left_Clasp, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file)); break;
                        case ']': yield return new Token(TokenType.Right_Clasp, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file)); break;

                        case ',': yield return new Token(TokenType.Coma, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file)); break;

                        case '-': yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file)); break;

                        case '+': yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file)); break;
                        case ';': yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file)); break;

                        case '*':
                            Match('*');
                            yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                            break;

                        case '!':
                            Match('=');
                            yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                            break;

                        case '=':
                            Match('=');
                            yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                            break;

                        case '<':
                            if (Match('='))
                            {
                                yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                            }
                            else
                            {
                                Match('-');
                                yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                            }
                            break;

                        case '>':
                            Match('=');
                            yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                            break;

                        case '&':
                            if (Match('&'))
                            {
                                yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                            }
                            else
                            {
                                ErrorList.errors.Add(new CompilingError(new Localization(lineNumber, start, file), ErrorCode.Invalid, "Expected && but got & at line"));
                            }
                            break;

                        case '|':
                            if (Match('|'))
                            {
                                yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                            }
                            else
                            {
                                ErrorList.errors.Add(new CompilingError(new Localization(lineNumber, start, file), ErrorCode.Invalid, "Expected || but got | at line"));
                            }
                            break;

                        //Operador {/} y {//} son necesarios??
                        //Ignorar Comentarios
                        case '/':
                            if (Match('/'))
                            {
                                while (!IsTheEnd() && LookAhead() != '\n') Advance();   //Retire LookAhead() != '\n' *(Mejorar)
                            }
                            else
                            {
                                yield return new Token(TokenType.Operator, input.Substring(start, current - start + 1), new Localization(lineNumber, column, file));
                            }
                            break;

                        case '"':
                            Tostring();
                            string val = input.Substring(start + 1, current - start);
                            Advance();
                            start++;
                            yield return new Token(TokenType.String, input.Substring(start, current - start), new Localization(lineNumber, column, file));
                            break;

                        default:
                            ErrorList.errors.Add(new CompilingError(new Localization(lineNumber, start, file), ErrorCode.Invalid, "Invalid character at line"));
                            break;
                    }
                
                }


                void Tostring()
                {
                    while (LookAhead() != '"' && !IsTheEnd())
                    {
                        if(LookAhead() == '\n') lineNumber++;
                        Advance();
                    }

                    if (IsTheEnd())
                    {
                        ErrorList.errors.Add(new CompilingError(new Localization(lineNumber, start, file), ErrorCode.Invalid, "Endless string at line"));
                    }
                }


                char LookAhead(int k = 0)
                {
                    if (IsTheEnd(1 + k)) return '\0';
                    char a = input[current + 1 + k];
                    return input[current + 1 + k];
                }

                bool IsTheEnd(int k = 0)
                {
                    return current + k >= input.Length;
                }

                bool Match(char expected)
                {
                    if (IsTheEnd(1)) return false;
                    if (expected != input[current + 1]) return false;

                    current++;
                    return true;
                }

                char Advance()
                {
                    current++;
                    return input[current - 1];
                }


                // ... more parsing rules for numbers, strings, operators
                current++; // Advance if not handled by specific rules
            }
            yield return new Token(TokenType.EOF, "", new Localization(lineNumber, column, file));
        }

        private bool IsKeyword(string value)
        {
            // Check if value is a reserved keyword like "Spawn", "Color", "GoTo", etc.
            return Enum.IsDefined(typeof(Keys), value); ;
        }
    }
}
