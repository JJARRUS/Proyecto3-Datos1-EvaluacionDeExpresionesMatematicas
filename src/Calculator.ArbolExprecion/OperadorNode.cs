#nullable enable

using System;

namespace Calculator.ArbolExprecion;

public class OperatorNode : Node
{
    public char Operator { get; }

    public OperatorNode(char op) : base(NodeType.Operator)
    {
        Operator = op;
    }

    // Evaluación recursiva: evalúa subárboles y aplica el operador
    public override double Evaluate()
    {
        if (Left == null || Right == null)
            throw new InvalidOperationException("Nodo operador debe tener ambos hijos");

        // 1. Evaluar recursivamente el subárbol izquierdo
        double leftValue = Left.Evaluate();

        // 2. Evaluar recursivamente el subárbol derecho
        double rightValue = Right.Evaluate();

        // 3. Aplicar el operador a los valores obtenidos
        return ApplyOperator(leftValue, rightValue);
    }

    private double ApplyOperator(double left, double right)
    {
        return Operator switch
        {
            '+' => left + right,
            '-' => left - right,
            '*' => left * right,
            '/' => right != 0 ? left / right : throw new DivideByZeroException("División por cero"),
            '%' => left % right,
            _ => throw new ArgumentException($"Operador no soportado: {Operator}")
        };
    }
}
