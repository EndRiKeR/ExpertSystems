namespace ExpertSystems.lab4.Logic;

public class KnowledgeBase
{
    public Formula[] Axioms { get; }    // посылки (аксиомы)
    public Formula Goal { get; }        // заключение (цель), которое надо доказать

    public KnowledgeBase(Formula[] axioms, Formula goal)
    {
        Axioms = axioms;
        Goal = goal;
    }
}
