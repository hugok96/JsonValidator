using System;
using System.Diagnostics;

namespace hugok96.JsonValidator
{
    class Program
    {
        static readonly string[] literals = new string[] { "true", "false", "null" };
        static readonly char[] whitespace = new char[] { WHITESPACE_SPACE, '\t', '\r', '\n' };
        static readonly char[] escapeSequences = new char[] { '"', '\\', '/', 'b', 'f', 'n', 'r', 't' };

        const char WHITESPACE_SPACE = ' ';
        const char TOKEN_ARRAY_OPEN = '[';
        const char TOKEN_ARRAY_CLOSE = ']';
        const char TOKEN_OBJECT_OPEN = '{';
        const char TOKEN_OBJECT_CLOSE = '}';
        const char TOKEN_COLON = ':';
        const char TOKEN_COMMA = ',';
        const char TOKEN_UNICODE_ESCAPE = 'u';
        const char TOKEN_QUOTE = '"';
        const char TOKEN_ESCAPE = '\\';
        const char TOKEN_NUMBER_NEGATIVE = '-';
        const char TOKEN_NUMBER_POSITIVE = '+';
        const char TOKEN_DECIMAL_DIVIDER = '.';
        const char TOKEN_EXP_LOWERCASE = 'e';
        const char TOKEN_EXP_UPPERCASE = 'E';

        static void Main(string[] args)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            string input = "{\"hello\": \"world\", \"test\": [1,2,3,4,5,\"6\", {\"7\": 8}, 9], \"test_2\": [{}, {}, {}]}";
            bool result = ValidateJson(input);
            s.Stop();
            Console.WriteLine(result);
            Console.WriteLine("Operation took " + s.Elapsed.TotalMilliseconds + "ms");
        }

        private static void SkipOverWhitespace(string input, ref int position)
        {
            while(position < input.Length)
            {
                char c = input[position];
                bool whitespaceFound = false;
                foreach (char ws in whitespace)
                {
                    if (ws == c)
                    {
                        whitespaceFound = true;
                        break;
                    }
                }

                if (!whitespaceFound)
                {
                    return;
                }

                position++;
            }            
        }

        static bool ValidateJson(string input)
        {
            int position = 0;
            SkipOverWhitespace(input, ref position);
            if (false == ValidateValue(input, ref position)) {
                return false;
            }
            SkipOverWhitespace(input, ref position);
            return position >= input.Length;
        }

        private static bool ValidateValue(string input, ref int position)
        {
            foreach (string literal in literals)
            {
                if (position + literal.Length <= input.Length && input.Substring(position, literal.Length) == literal)
                {
                    position += literal.Length;
                    return true;
                }
            }

            if(ValidateString(input, ref position))
            {
                return true;
            }

            if (ValidateNumber(input, ref position))
            {
                return true;
            }

            if (ValidateArray(input, ref position))
            {
                return true;
            }

            if (ValidateObject(input, ref position))
            {
                return true;
            }

            return false;
        }

        private static bool ValidateNumber(string input, ref int position)
        {
            char c = input[position];
            if(c == TOKEN_NUMBER_NEGATIVE)
            {
                position++;
                return ValidateNumber(input, ref position);
            }

            int digitsFound = 0;
            if (c >= '1' && c <= '9')
            {
                position++;
                digitsFound++;
                while (position < input.Length)
                {
                    c = input[position];
                    if (false == (c >= '0' && c <= '9'))
                    {
                        break;
                    }
                    position++;
                    digitsFound++;
                }
            }
            else if(c == '0')
            {
                position++;
                digitsFound++;
            }

            if(digitsFound == 0)
            {
                return false;
            }

            if (position >= input.Length) 
            {
                return true;
            }

            c = input[position];
            if(c == TOKEN_DECIMAL_DIVIDER) 
            {
                position++;
                digitsFound = 0;
                while (position < input.Length)
                {
                    c = input[position];
                    if (false == (c >= '0' && c <= '9'))
                    {
                        break;
                    }
                    digitsFound++;
                    position++;
                }
                if (digitsFound == 0)
                {
                    return false; // no digits found
                }

                if (position >= input.Length)
                {
                    return true;
                }
            }

            c = input[position];
            if(c != TOKEN_EXP_LOWERCASE && c != TOKEN_EXP_UPPERCASE)
            {
                return true;
            }

            position++;
            c = input[position];
            if (c == TOKEN_NUMBER_NEGATIVE || c == TOKEN_NUMBER_POSITIVE)
            {
                position++;
            }

            position++;
            digitsFound = 0;
            while (position < input.Length)
            {
                c = input[position];
                if (false == (c >= '0' && c <= '9'))
                {
                    break;
                }
                digitsFound++;
                position++;
            }
            return digitsFound > 0;
        }

        private static bool ValidateArray(string input, ref int position)
        {
            char c = input[position];
            if(c != TOKEN_ARRAY_OPEN)
            {
                return false;
            }
            position++;

            SkipOverWhitespace(input, ref position);
            while (position < input.Length)
            {
                c = input[position];
                if(c == TOKEN_ARRAY_CLOSE)
                {
                    position++;
                    return true;
                }

                if(false == ValidateValue(input, ref position))
                {
                    return false;
                }
                SkipOverWhitespace(input, ref position);

                c = input[position];
                if (c == TOKEN_COMMA)
                {
                    position++;
                }
                SkipOverWhitespace(input, ref position);
            }

            return false;
        }

        private static bool ValidateObject(string input, ref int position)
        {
            char c = input[position];
            if (c != TOKEN_OBJECT_OPEN)
            {
                return false;
            }
            position++;

            SkipOverWhitespace(input, ref position);
            while (position < input.Length)
            {
                c = input[position];
                if (c == TOKEN_OBJECT_CLOSE)
                {
                    position++;
                    return true;
                }

                if (false == ValidateString(input, ref position))
                {
                    return false;
                }
                SkipOverWhitespace(input, ref position);

                if (position >= input.Length)
                {
                    return false;
                }

                c = input[position];
                if(c != TOKEN_COLON)
                {
                    return false;
                }
                position++;
                SkipOverWhitespace(input, ref position);

                if (false == ValidateValue(input, ref position))
                {
                    return false;
                }
                SkipOverWhitespace(input, ref position);

                c = input[position];
                if (c == TOKEN_COMMA)
                {
                    position++;
                }
                SkipOverWhitespace(input, ref position);
            }

            return false;
        }

        private static bool ValidateString(string input, ref int position)
        {
            char c = input[position];
            if (c != TOKEN_QUOTE)
            {
                return false;
            }
            position++;

            while (position < input.Length)
            {
                c = input[position];
                if((byte)c >= 0x00 && (byte)c <= 0x1F)
                {
                    //throw new Exception("Invalid control character found");
                    return false;
                }
                
                if (c != TOKEN_ESCAPE && c != TOKEN_QUOTE) 
                {
                    position++;
                    continue;
                }
                
                if(c == TOKEN_QUOTE)
                {
                    position++;
                    return true;
                }
                else if(c == TOKEN_ESCAPE)
                {
                    if(position + 1 < input.Length)
                    {
                        char c2 = input[position + 1];
                        bool found = false;
                        foreach(char es in escapeSequences)
                        {
                            if(c2 == es)
                            {
                                found = true;
                            }
                        }

                        if (found) 
                        {
                            position += 2;
                            continue;
                        }

                        if(c2 == TOKEN_UNICODE_ESCAPE)
                        {
                            if(position + 5 < input.Length)
                            {
                                for(int i = 0; i < 4; i++)
                                {
                                    char c3 = input[position + 2 + i];
                                    if(!(c3 >= '0' && c3 <= '9') && !(c3 >= 'A' && c3 <= 'F') && !(c3 >= 'a' && c3 <= 'f'))
                                    {
                                       // throw new Exception("Illegal unicode escape sequence");
                                        return false;
                                    }
                                }

                                position += 6;
                                continue;
                            }
                            else
                            {
                               // throw new Exception("Unexpected end of unicode escape sequence");
                                return false;
                            }
                        }

                       // throw new Exception("Unexpected escape sequence");
                        return false;

                    }
                    else
                    {
                        // throw new Exception("Unexpected end of escape sequence");
                        return false;
                    }
                }
            }

            return false;
        }
    }
}
