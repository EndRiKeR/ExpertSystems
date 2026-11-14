namespace ExpertSystems.lab5.Logic;

// флаг терма, как в лекции
public enum TermFlag
{
    Free = -1,  // переменная без значения
    Linked = 0, // переменная связана с другой переменной
    Valued = 1, // терм имеет значение (константа или переменная, получившая значение)
}

// терм - аргумент предиката: переменная или константа
// договоренность: константы пишем с большой буквы (или с цифры), переменные - с маленькой
public class Term
{
    public string Name;      // имя терма
    public bool IsConstant;  // true = константа; false = переменная
    public TermFlag Flag;    // состояние терма
    public string? Value;    // значение (имя константы), если Flag = Valued
    public Term? Link;       // переменная, с которой связана эта переменная, если Flag = Linked

    public Term(string name, bool isConstant)
    {
        Name = name;
        IsConstant = isConstant;
        Flag = isConstant ? TermFlag.Valued : TermFlag.Free; // константа сразу имеет значение - саму себя
        Value = isConstant ? name : null;
    }

    // идем по связям до конца цепочки: там либо свободная переменная, либо терм со значением
    public Term End()
    {
        var term = this;
        while (term.Flag == TermFlag.Linked)
            term = term.Link!;

        return term;
    }

    // тот же терм: одна и та же переменная или одинаковые константы
    public bool IsSame(Term other)
    {
        return this == other || (IsConstant && other.IsConstant && Name == other.Name);
    }

    public override string ToString()
    {
        return Name;
    }
}
