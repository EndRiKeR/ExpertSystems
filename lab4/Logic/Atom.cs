namespace ExpertSystems.lab4.Logic;

public class Atom
{
    public string Name; // имя атома (высказывания): A, B, C...
    public bool Sign;   // знак: true = атом без отрицания; false = с отрицанием (~A)

    public Atom(string name, bool sign)
    {
        Name = name;
        Sign = sign;
    }

    // тот же самый атом: совпадают и имя, и знак
    public bool IsSame(Atom other)
    {
        return Name == other.Name && Sign == other.Sign;
    }

    // контрарная пара: имя одно, а знаки разные (A и ~A)
    public bool IsContrary(Atom other)
    {
        return Name == other.Name && Sign != other.Sign;
    }

    public override string ToString()
    {
        return Sign ? Name : $"~{Name}";
    }
}
