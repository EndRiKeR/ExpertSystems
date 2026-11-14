namespace ExpertSystems.lab5.Logic;

// атом (предикат): имя, знак и аргументы
public class Atom
{
    public string Name;                         // имя предиката
    public bool Sign;                           // знак: true = без отрицания; false = с отрицанием (~P)
    public List<Term> Terms = new List<Term>(); // аргументы предиката (их количество = Terms.Count)

    public Atom(string name, bool sign)
    {
        Name = name;
        Sign = sign;
    }

    // у атомов одинаковые аргументы на всех местах
    public bool HasSameTerms(Atom other)
    {
        if (Terms.Count != other.Terms.Count)
            return false;

        for (var i = 0; i < Terms.Count; i++)
            if (!Terms[i].IsSame(other.Terms[i]))
                return false;

        return true;
    }

    // тот же самый атом: совпадают имя, знак и аргументы
    public bool IsSame(Atom other)
    {
        return Name == other.Name && Sign == other.Sign && HasSameTerms(other);
    }

    // кандидаты в контрарную пару: имя одно, а знаки разные; аргументы еще надо унифицировать
    public bool IsContrary(Atom other)
    {
        return Name == other.Name && Sign != other.Sign;
    }

    public override string ToString()
    {
        return $"{(Sign ? "" : "~")}{Name}({string.Join(", ", Terms)})";
    }
}
