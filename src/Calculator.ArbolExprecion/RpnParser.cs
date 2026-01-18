using System;
using System.Collections.Generic;

namespace Calculator.ArbolExprecion
{
public class RpnParser
{
    private static readonly Dictionary<char, int> Precedence = new()
    {
        { '+', 1 },
        { '-', 1 },
        { '*', 2 },
        { '/', 2 }
    };

    public List<string> ConvertToPostfix(string expression)
    {
        var output = new List<string>();
        var operators = new Stack<char>();

        int i = 0;

        while (i < expression.Length)
        {
            char current = expression[i];

            if (char.IsWhiteSpace(current))
            {
                i++;
                continue;
            }

            // Operando (número)
            if (char.IsDigit(current))
            {
                string number = "";

                while (i < expression.Length &&
                       (char.IsDigit(expression[i]) || expression[i] == '.'))
                {
                    number += expression[i];
                    i++;
                }

                output.Add(number);
                continue;
            }

            // Operador
            if (IsOperator(current))
            {
                while (operators.Count > 0 &&
                       IsOperator(operators.Peek()) &&
                       Precedence[operators.Peek()] >= Precedence[current])
                {
                    output.Add(operators.Pop().ToString());
                }

                operators.Push(current);
                i++;
                continue;
            }

            // Paréntesis izquierdo
            if (current == '(')
            {
                operators.Push(current);
                i++;
                continue;
            }

            // Paréntesis derecho
            if (current == ')')
            {
                while (operators.Count > 0 && operators.Peek() != '(')
                {
                    output.Add(operators.Pop().ToString());
                }

                if (operators.Count == 0)
                    throw new ArgumentException("Paréntesis desbalanceados");

                operators.Pop(); // quitar '('
                i++;
                continue;
            }

            throw new ArgumentException($"Carácter inválido: {current}");
        }

        while (operators.Count > 0)
        {
            if (operators.Peek() == '(')
                throw new ArgumentException("Paréntesis desbalanceados");

            output.Add(operators.Pop().ToString());
        }

        ValidatePostfix(output);
        return output;
    }

    private bool IsOperator(char c)
    {
        return c == '+' || c == '-' || c == '*' || c == '/';
    }

    // Validación de la expresión postfija
    private void ValidatePostfix(List<string> postfix)
    {
        int stackCount = 0;

        foreach (var token in postfix)
        {
            if (double.TryParse(token, out _))
            {
                stackCount++;
            }
            else if (token.Length == 1 && IsOperator(token[0]))
            {
                stackCount -= 2;
                if (stackCount < 0)
                    throw new ArgumentException("Expresión postfija inválida");

                stackCount++;
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
