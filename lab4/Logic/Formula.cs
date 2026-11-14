namespace ExpertSystems.lab4.Logic;

// виды узлов формулы; число = приоритет связки (чем больше, тем сильнее связывает)
public enum FormulaKind
{
    Equiv = 1,   // эквивалентность  <->
    Implies = 2, // импликация       ->
    Or = 3,      // дизъюнкция       v
    And = 4,     // конъюнкция       &
    Not = 5,     // отрицание        ~
    Atom = 6,    // атом (простое высказывание)
}

// формула логики высказываний в виде дерева
public class Formula
{
    public FormulaKind Kind; // вид узла
    public string Name = ""; // имя атома (только для Kind = Atom)
    public Formula? Left;    // левая часть (для отрицания - единственная)
    public Formula? Right;   // правая часть

    public Formula(string name)
    {
        Kind = FormulaKind.Atom;
        Name = name;
    }

    public Formula(FormulaKind kind, Formula left, Formula? right = null)
    {
        Kind = kind;
        Left = left;
        Right = right;
    }

    public override string ToString()
    {
        if (Kind == FormulaKind.Atom)
            return Name;

        if (Kind == FormulaKind.Not)
            return $"~{Wrap(Left!)}";

        var sign = Kind switch
        {
            FormulaKind.And => "&",
            FormulaKind.Or => "v",
            FormulaKind.Implies => "->",
            _ => "<->",
        };

        return $"{Wrap(Left!)} {sign} {Wrap(Right!)}";
    }

    // часть формулы берем в скобки, если ее связка слабее нашей (или такая же, но не & и не v)
    private string Wrap(Formula part)
    {
        var sameAssociative = part.Kind == Kind && (Kind == FormulaKind.And || Kind == FormulaKind.Or);
        var needBrackets = part.Kind < Kind || (part.Kind == Kind && !sameAssociative && Kind != FormulaKind.Not);
        return needBrackets ? $"({part})" : part.ToString();
    }
}
