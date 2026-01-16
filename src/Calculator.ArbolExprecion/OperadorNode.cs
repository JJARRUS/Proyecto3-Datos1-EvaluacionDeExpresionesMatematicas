public class OperatorNode : Node
{
    public char Operator { get; }

    public OperatorNode(char op) : base(NodeType.Operator)
    {
        Operator = op;
    }
}
