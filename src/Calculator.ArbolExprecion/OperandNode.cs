public class OperandNode : Node
{
    public double Value { get; }

    public OperandNode(double value) : base(NodeType.Operand)
    {
        Value = value;
    }
}
