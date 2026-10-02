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
    
    public List<Condition> Start(List<Rule> openRules, List<Condition> closedConditions, Condition endCondition)
    {
        _hasAnswer = false;
        _hasNoAnswer = false;

        // Инициализация списков начальными значениями
        _closedConditions = closedConditions;
        _openRules        = openRules;
        _confirmedRules   = new List<Rule>();
        _deniedRules      = new List<Rule>();

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
            var g = Descendants(); // находим потомков текущей вершины
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
        int g = 0;
        
        foreach (var rule in _openRules)
        {
            bool isRuleConfirmed = true;
            foreach (var ruleCondition in rule.In)
            {
                if (_closedConditions.Contains(ruleCondition))
                    continue;
                
                isRuleConfirmed = false;
                break;

            }

            if (!isRuleConfirmed)
                continue;

            g++;
            
            rule.Mark = RuleMark.Confirmed;
            _confirmedRules.Add(rule);
            
            if (rule.Out.Mark != ConditionMark.Closed)
            {
                rule.Out.Mark = ConditionMark.Closed;
                _closedConditions.Add(rule.Out);
            }
            
            if (rule.Out == _endCondition)
                _hasAnswer = true;
        }
        
        _openRules = _openRules.Where(x => x.Mark == RuleMark.None).ToList();
        
        return g;
    }

}