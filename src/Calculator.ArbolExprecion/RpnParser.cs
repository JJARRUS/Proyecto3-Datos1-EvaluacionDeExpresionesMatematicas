using System;
using System.Collections.Generic;
using System.Globalization;

namespace Calculator.ArbolExprecion
{
public class RpnParser
{
    // Precedencia y asociatividad
    private static readonly Dictionary<string, int> Precedence = new()
    {
        { "~", 5 },              // not (unario)
        { "**", 4 },             // potencia
        { "*", 3 }, { "/", 3 }, { "%", 3 },
        { "+", 2 }, { "-", 2 },
        { "&", 1 },              // and
        { "^", 1 },              // xor
        { "|", 0 }               // or
    };

    private static readonly HashSet<string> RightAssociative = new() { "**", "~" };
    private static readonly HashSet<string> UnaryOperators = new() { "~" };

    public List<string> ConvertToPostfix(string expression)
    {
        var output = new List<string>();
        var operators = new Stack<string>();

        var tokens = Tokenize(expression);

        foreach (var token in tokens)
        {
            if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                output.Add(token);
            }
            else if (IsOperator(token))
            {
                while (operators.Count > 0 && IsOperator(operators.Peek()))
                {
                    string top = operators.Peek();
                    bool higher = Precedence[top] > Precedence[token];
                    bool equalAndLeft = Precedence[top] == Precedence[token] && !RightAssociative.Contains(token);

                    if (higher || equalAndLeft)
                        output.Add(operators.Pop());
                    else
                        break;
                }

                operators.Push(token);
            }
            else if (token == "(")
            {
                operators.Push(token);
            }
            else if (token == ")")
            {
                while (operators.Count > 0 && operators.Peek() != "(")
                    output.Add(operators.Pop());

                if (operators.Count == 0)
                    throw new ArgumentException("Paréntesis desbalanceados");

                operators.Pop(); // descartar '('
            }
            else
            {
                throw new ArgumentException($"Token inválido: {token}");
            }
        }

        while (operators.Count > 0)
        {
            var op = operators.Pop();
            if (op == "(")
                throw new ArgumentException("Paréntesis desbalanceados");
            output.Add(op);
        }

        ValidatePostfix(output);
        return output;
    }

    private List<string> Tokenize(string expr)
    {
        var tokens = new List<string>();
        int i = 0;

        while (i < expr.Length)
        {
            char c = expr[i];

            if (char.IsWhiteSpace(c)) { i++; continue; }

            // Número (permitir decimal con '.')
            if (char.IsDigit(c) || c == '.')
            {
                int start = i;
                while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) i++;
                tokens.Add(expr.Substring(start, i - start));
                continue;
            }

            // Palabras clave (and, or, xor, not)
            if (char.IsLetter(c))
            {
                int start = i;
                while (i < expr.Length && char.IsLetter(expr[i])) i++;
                string word = expr.Substring(start, i - start).ToLowerInvariant();
                tokens.Add(word switch
                {
                    "and" => "&",
                    "or"  => "|",
                    "xor" => "^",
                    "not" => "~",
                    _ => throw new ArgumentException($"Token inválido: {word}")
                });
                continue;
            }

            // Operadores multi-caracter (**)
            if (c == '*' && i + 1 < expr.Length && expr[i + 1] == '*')
            {
                tokens.Add("**");
                i += 2;
                continue;
            }

            // Operadores simples y paréntesis
            if (IsOperator(c.ToString()) || c == '(' || c == ')')
            {
                tokens.Add(c.ToString());
                i++;
                continue;
            }

            throw new ArgumentException($"Carácter inválido: {c}");
        }

        return tokens;
    }

    private bool IsOperator(string token) => Precedence.ContainsKey(token);

    // Validación de la expresión postfija (considera operadores unarios y binarios)
    private void ValidatePostfix(List<string> postfix)
    {
        int stackCount = 0;

        foreach (var token in postfix)
        {
            if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                stackCount++;
            }
            else if (IsOperator(token))
            {
                if (UnaryOperators.Contains(token))
                {
                    if (stackCount < 1) throw new ArgumentException("Expresión postfija inválida");
                    // neto 0
                }
                else
                {
                    stackCount -= 2;
                    if (stackCount < 0) throw new ArgumentException("Expresión postfija inválida");
                    stackCount += 1;
                }
            }
            else
            {
                throw new ArgumentException("Token inválido en expresión postfija");
            }
        }

        if (stackCount != 1)
            throw new ArgumentException("Expresión postfija inválida");
    }

    /// Construye y devuelve la raíz del árbol desde una expresión RPN (tokens separados por espacio).
    public static Node Parse(string rpn)
    {
        if (rpn == null) throw new ArgumentNullException(nameof(rpn));

        var tokens = TokenizeRpn(rpn);
        var tree = new ExpressionTree();
        tree.BuildFromPostfix(tokens);

        if (tree.Root == null)
            throw new ArgumentException("Expresión postfija inválida", nameof(rpn));

        return tree.Root;
    }

    private static List<string> TokenizeRpn(string rpn)
    {
        var list = new List<string>();
        foreach (var tok in rpn.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            list.Add(tok);
        return list;
    }
}
}
