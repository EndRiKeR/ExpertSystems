using ExpertSystems.lab2.Graph;

namespace ExpertSystems.lab2;

public class SearchInWidthFromData
{
    private List<Condition> _closedConditions;  // список закрытых вершин
    
    private List<Rule> _openRules;      // список недоказанных проавил
    private List<Rule> _confirmedRules;    // список доказанных правил
    private List<Rule> _deniedRules;    // список запрещенных правил

    private bool _hasAnswer = false;    // true = решение есть; false = пока неизвествно
    private bool _hasNoAnswer = false;  // true = решения нет; false = пока неизвествно

    private Condition _endCondition;      // конечная вершина

    private Dictionary<Condition, Rule> _conditionToRule; // правило, которым впервые доказано условие, для восстановления решения

    public bool HasAnswer => _hasAnswer;    // найдено ли решение
    public IReadOnlyList<Rule> ConfirmedRules => _confirmedRules; // все доказанные правила
    
    public List<Condition> Start(List<Rule> openRules, List<Condition> closedConditions, Condition endCondition)
    {
        _hasAnswer = false;
        _hasNoAnswer = false;

        // Инициализация списков начальными значениями
        _closedConditions = closedConditions;
        _openRules        = openRules;
        _confirmedRules   = new List<Rule>();
        _deniedRules      = new List<Rule>();
        _conditionToRule  = new Dictionary<Condition, Rule>();

        // Инициализация конечной вершины
        _endCondition = endCondition;

        if (_closedConditions.Contains(_endCondition))
        {
            _hasAnswer = true;
            return _closedConditions;
        }

        // Запуск метода поиска решения
        SearchPath();
        
        // Возвращаем решение
        return _closedConditions;
    }
    
    private void SearchPath()
    {
        while (!_hasAnswer && !_hasNoAnswer) // пока нет информации о том, если ли решение или нет...
        {
            var g = Descendants(); // находим потомков
            if (_hasAnswer) // если нашли решение...
                return;     // ...выход
            
            // если нет правил, которые можно закрыть через закрытые вершины,...
            if (g == 0)
            {
                // ...то нет решений, запоминаем этот вариант
                _hasNoAnswer = true; // помечаем флаг, что РЕШЕНИЯ НЕТ
                return;
            }
        }
    }
        
    private int Descendants()
    {
        int g = 0; // считает количество потомков
        
        foreach (var rule in _openRules) // проходимся по всем правилам
        {
            bool isRuleConfirmed = true; // предполагаем, что правило доказано
            foreach (var ruleCondition in rule.In) // для проверки пройдемся по всем входным вершинам
            {
                if (_closedConditions.Contains(ruleCondition)) // если она закрыта, то все хорошо, продолжаем проверку
                    continue;
                
                isRuleConfirmed = false; // если она не закрыта, то мы не можем доказать правило, пропускаем его
                break;
            }

            if (!isRuleConfirmed) // если правило не было доказано - пропуск
                continue;

            g++; // иначе мы нашли потомка
            
            rule.Mark = RuleMark.Confirmed; // пометим правило как подтвержденное
            _confirmedRules.Add(rule); // запишем его в соответствующий список
            
            if (rule.Out.Mark != ConditionMark.Closed) // если вершина вывода не была до этого помечена...
            {
                rule.Out.Mark = ConditionMark.Closed; // закрываем ее
                _closedConditions.Add(rule.Out); // добавляем в соответствующий список
                _conditionToRule[rule.Out] = rule; // запоминаем, каким правилом доказано условие
            }
            
            if (rule.Out == _endCondition) // если вершина вывода искомая...
                _hasAnswer = true; // .. то есть решение
        }
        
        // по правилам массивов, мы не можем исключать из списка эдлементы в цикле, потому делаем это отдельно
        _openRules = _openRules.Where(x => x.Mark == RuleMark.None).ToList(); 
        
        // возвращаем g
        return g;
    }
    
    
    
    
    

    // собираем решение: правила, которые действительно понадобились для доказательства цели
    public List<Rule> BuildSolution()
    {
        var result = new List<Rule>();

        if (!_hasAnswer) // если решения нет, то и смысл строить его тоже нет
            return result;

        // идём от цели к начальным фактам по словарю "условие -> правило"
        var visited = new HashSet<Condition>();
        var stack = new Stack<Condition>();
        stack.Push(_endCondition);

        while (stack.Count > 0)
        {
            var condition = stack.Pop();
            if (!visited.Add(condition))
                continue;

            // начальный факт - доказывать нечего
            if (!_conditionToRule.TryGetValue(condition, out var rule))
                continue;

            result.Add(rule);
            foreach (var ruleCondition in rule.In)
                stack.Push(ruleCondition);
        }

        // порядок применения правил - в порядке их доказательства
        return result.OrderBy(r => _confirmedRules.IndexOf(r)).ToList();
    }
}
