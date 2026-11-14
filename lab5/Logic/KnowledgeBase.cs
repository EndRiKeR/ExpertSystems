namespace ExpertSystems.lab5.Logic;

public class KnowledgeBase
{
    public Disjunct[] Axioms { get; }       // дизъюнкты аксиом (посылок)
    public Disjunct[] NegativeGoal { get; } // дизъюнкты отрицания цели (может быть пустым)

    public KnowledgeBase(Disjunct[] axioms, Disjunct[] negativeGoal)
    {
        Axioms = axioms;
        NegativeGoal = negativeGoal;
    }
}
