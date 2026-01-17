using System;

namespace Calculator.ArbolExprecion;

public class ExpressionEvaluator
{
    private readonly ExpressionTree _tree;

    public ExpressionEvaluator(ExpressionTree tree)
    {
        _tree = tree ?? throw new ArgumentNullException(nameof(tree));
    }

    // 4. Retornar el resultado final de la evaluación completa del árbol
    public double Evaluate()
    {
        if (_tree.Root == null)
            throw new InvalidOperationException("El árbol de expresión está vacío");

        // Inicia la evaluación recursiva desde la raíz
        return _tree.Root.Evaluate();
    }
}
