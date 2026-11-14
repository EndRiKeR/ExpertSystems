namespace ExpertSystems.lab4.Logic;

public class Disjunct
{
    public int Num;                              // номер дизъюнкта в множестве
    public List<Atom> Atoms = new List<Atom>();  // атомы дизъюнкта; пустой список = пустой дизъюнкт (ложь)
    public string Source = "";                   // откуда взялся: аксиома, отрицание цели или резольвента

    public Disjunct? Parent1;      // первый родитель (только у резольвенты), для восстановления доказательства
    public Disjunct? Parent2;      // второй родитель (только у резольвенты)
    public string? ContraryName;   // имя атома контрарной пары, по которой получена резольвента

    public bool IsEmpty => Atoms.Count == 0; // пустой дизъюнкт

    // тавтология: в дизъюнкте есть и атом, и его отрицание (A v ~A) - он всегда истинен и бесполезен
    public bool IsTautology()
    {
        return Atoms.Any(a => Atoms.Any(b => a.IsContrary(b)));
    }

    // такой же дизъюнкт: тот же набор атомов, порядок не важен
    public bool IsSame(Disjunct other)
    {
        return Atoms.Count == other.Atoms.Count && Atoms.All(a => other.Atoms.Any(b => a.IsSame(b)));
    }

    public override string ToString()
    {
        return IsEmpty ? "[] (пустой дизъюнкт)" : string.Join(" v ", Atoms);
    }
}
