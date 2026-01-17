using System;
using System.Collections.Generic;

public class ExpressionTree
{
    public Node? Root { get; private set; }

    public void BuildFromPostfix(List<string> postfix)
    {
        var stack = new Stack<Node>();

        foreach (var token in postfix)
        {
            // Operando → nodo hoja
            if (double.TryParse(token, out double value))
            {
                stack.Push(new OperandNode(value));
            }
            // Operador → nodo interno
            else if (IsOperator(token))
            {
                if (token == "~")
                {
                    if (stack.Count < 1)
                        throw new ArgumentException("Expresión postfija inválida (operador unario)");

                    Node right = stack.Pop();
                    var operatorNode = new OperatorNode(token)
                    {
                        Right = right,
                        Left = null
                    };
                    stack.Push(operatorNode);
                }
                else
                {
                    if (stack.Count < 2)
                        throw new ArgumentException("Expresión postfija inválida");

                    Node right = stack.Pop();
                    Node left = stack.Pop();

                    var operatorNode = new OperatorNode(token)
                    {
                        Left = left,
                        Right = right
                    };

                    stack.Push(operatorNode);
                }
            }
            else
            {
                throw new ArgumentException("Token inválido en la expresión postfija");
            }
        }

        if (stack.Count != 1)
            throw new ArgumentException("Expresión postfija inválida");

        Root = stack.Pop();
    }

    private bool IsOperator(string token)
    {
        return token == "+" || token == "-" || token == "*" || token == "/" ||
               token == "%" || token == "**" || token == "&" || token == "|" ||
               token == "^" || token == "~";
    }
}
