#nullable enable

namespace Calculator.ArbolExprecion;

public enum NodeType
{
    Operator,
    Operand
}

public abstract class Node
{
    public NodeType Type { get; }

    public Node? Left { get; set; }
    public Node? Right { get; set; }

    protected Node(NodeType type)
    {
        Type = type;
    }

    // Método abstracto para evaluación recursiva
    public abstract double Evaluate();
}
