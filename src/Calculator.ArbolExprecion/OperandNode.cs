public class OperandNode : Node
{
    public double Value { get; }

    public OperandNode(double value) : base(NodeType.Operand)
    {
        Value = value;
    }

    // Evaluación de nodo hoja: retorna el valor directamente
    public override double Evaluate()
    {
        return Value;
    }
}
