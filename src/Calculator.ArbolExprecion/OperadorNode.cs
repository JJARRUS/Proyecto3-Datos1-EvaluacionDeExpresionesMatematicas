using System;

public class OperatorNode : Node
{
    public string Operator { get; }

    public OperatorNode(string op) : base(NodeType.Operator)
    {
        Operator = op;
    }

    // Evaluación recursiva: soporta operadores binarios y unarios (~)
    public override double Evaluate()
    {
        // Operador unario
        if (Operator == "~")
        {
            if (Right == null)
                throw new InvalidOperationException("Operador unario sin operando");
            double r = Right.Evaluate();
            return ApplyUnary(r);
        }

        // Operadores binarios
        if (Left == null || Right == null)
            throw new InvalidOperationException("Nodo operador debe tener ambos hijos");

        double leftValue = Left.Evaluate();
        double rightValue = Right.Evaluate();

        return ApplyBinary(leftValue, rightValue);
    }

    private double ApplyUnary(double value)
    {
        // not lógico: true->0, false->1 (negación)
        bool b = value != 0;
        return b ? 0d : 1d;
    }

    private double ApplyBinary(double left, double right)
    {
        return Operator switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            "/" => right != 0 ? left / right : throw new DivideByZeroException("División por cero"),
            "%" => left % right,
            "**" => Math.Pow(left, right),
            "&" => BoolToNumber(Bool(left) && Bool(right)),
            "|" => BoolToNumber(Bool(left) || Bool(right)),
            "^" => BoolToNumber(Bool(left) ^ Bool(right)),
            _ => throw new ArgumentException($"Operador no soportado: {Operator}")
        };
    }

    private static bool Bool(double v) => v != 0;
    private static double BoolToNumber(bool v) => v ? 1d : 0d;
}
