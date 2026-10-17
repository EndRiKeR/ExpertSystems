namespace ExpertSystems.lab2.Graph;

public enum ConditionMark
{
    Denied = -1,
    Opened = 0,
    Closed = 1,
}

public class Condition
{
    public int Num;
    public ConditionMark Mark = ConditionMark.Opened;
}