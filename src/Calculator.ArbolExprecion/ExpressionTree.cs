public class ExpressionTree
{
    public Node? Root { get; private set; }

    public ExpressionTree()
    {
        Root = null;
    }

    public ExpressionTree(Node root)
    {
        Root = root;
    }

    public void SetRoot(Node root)
    {
        Root = root;
    }

    public bool IsEmpty()
    {
        return Root == null;
    }

    // Acceso al nodo raíz
    public Node? GetRoot()
    {
        return Root;
    }

    // Asignar hijos a un nodo existente
    public void SetChildren(Node parent, Node? left, Node? right)
    {
        parent.Left = left;
        parent.Right = right;
    }

    // Verificar si un nodo es hoja
    public bool IsLeaf(Node node)
    {
        return node.Left == null && node.Right == null;
    }
}
