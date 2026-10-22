namespace ExpertSystems.lab2.Graph;

public enum RuleMark
{
    Denied = -1,
    None = 0,
    Confirmed = 1,
}

public class Rule
{
    public int Num;
    public RuleMark Mark = RuleMark.None;

    public Condition[] In;
    public Condition Out;

    public Rule(int num, RuleMark mark, Condition[] ins, Condition outs)
    {
        Num = num;
        Mark = mark;
        In = ins;
        Out = outs;
    }
}
