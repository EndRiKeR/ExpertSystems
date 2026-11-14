namespace ExpertSystems.lab5.Logic;

public class Disjunct
{
    public int Num;                              // номер дизъюнкта в множестве
    public List<Atom> Atoms = new List<Atom>();  // атомы дизъюнкта; пустой список = пустой дизъюнкт (ложь)
    public string Source = "";                   // откуда взялся: аксиома, отрицание цели или резольвента

    public Disjunct? Parent1;       // первый родитель (только у резольвенты), для восстановления доказательства
    public Disjunct? Parent2;       // второй родитель (только у резольвенты)
    public string Substitution = "";// подстановка, с которой получена резольвента

    public bool IsEmpty => Atoms.Count == 0; // пустой дизъюнкт

    // копия дизъюнкта со своими переменными: унификация меняет флаги термов, а исходный дизъюнкт портить нельзя
    public Disjunct Copy()
    {
        var copy = new Disjunct { Num = Num, Source = Source };
        var variables = new Dictionary<Term, Term>(); // старая переменная -> ее копия (одна на весь дизъюнкт)

        foreach (var atom in Atoms)
        {
            var newAtom = new Atom(atom.Name, atom.Sign);
            foreach (var term in atom.Terms)
            {
                if (!variables.TryGetValue(term, out var newTerm))
                {
                    newTerm = new Term(term.Name, term.IsConstant);
                    variables[term] = newTerm;
                }
                newAtom.Terms.Add(newTerm);
            }
            copy.Atoms.Add(newAtom);
        }

        return copy;
    }

    // тавтология: в дизъюнкте есть атом и его отрицание с теми же аргументами (P(x) v ~P(x))
    public bool IsTautology()
    {
        return Atoms.Any(a => Atoms.Any(b => a.IsContrary(b) && a.HasSameTerms(b)));
    }

    // такой же дизъюнкт с точностью до имен переменных: P(x1) v Q(x1) и P(z5) v Q(z5) - одно и то же
    public bool IsSame(Disjunct other)
    {
        return Canonical() == other.Canonical();
    }

    // запись дизъюнкта, в которой переменные названы по порядку появления: _1, _2, ...
    private string Canonical()
    {
        var names = new Dictionary<Term, string>();
        var atoms = new List<string>();

        foreach (var atom in Atoms)
        {
            var terms = new List<string>();
            foreach (var term in atom.Terms)
            {
                if (term.IsConstant)
                {
                    terms.Add(term.Name);
                    continue;
                }

                if (!names.ContainsKey(term))
                    names[term] = $"_{names.Count + 1}";
                terms.Add(names[term]);
            }
            atoms.Add($"{(atom.Sign ? "" : "~")}{atom.Name}({string.Join(",", terms)})");
        }

        return string.Join(" v ", atoms);
    }

    public override string ToString()
    {
        return IsEmpty ? "[] (пустой дизъюнкт)" : string.Join(" v ", Atoms);
    }
}
