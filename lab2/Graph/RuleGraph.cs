namespace ExpertSystems.lab2.Graph;

public class RuleGraph
{
    public Condition[] Conditions { get; }          // все условия, Num = 0..N-1
    public Rule[] Rules { get; }                    // все правила
    public Condition[] ClosedConditions { get; }    // начальные факты
    public Condition EndCondition { get; }          // цель

    public RuleGraph(
        Condition[] conditions,
        Rule[] rules,
        Condition[] closedConditions,
        Condition endCondition)
    {
        Conditions = conditions;
        Rules = rules;
        ClosedConditions = closedConditions;
        EndCondition = endCondition;
    }
}