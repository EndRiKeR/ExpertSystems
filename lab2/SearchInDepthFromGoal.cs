using ExpertSystems.lab2.Graph;

namespace ExpertSystems.lab2;

public class SearchInDepthFromGoal
{
    private List<Condition> _closedConditions;  // список закрытых (доказанных) вершин
    private List<Condition> _deniedConditions;  // список запрещенных (недоказуемых) вершин
    private Stack<Condition> _openConditions;   // стек открытых вершин (подцелей)

    private List<Rule> _rules;          // все правила базы
    private Stack<Rule> _openRules;     // стек открытых правил
    private List<Rule> _confirmedRules; // список доказанных правил
    private List<Rule> _deniedRules;    // список запрещенных правил

    private bool _hasAnswer = false;    // true = решение есть; false = пока неизвествно
    private bool _hasNoAnswer = false;  // true = решения нет; false = пока неизвествно

    private Condition _endCondition;    // конечная вершина (цель)

    private Dictionary<Condition, Rule> _conditionToRule; // правило, которым доказано условие, для восстановления решения

    private int _step; // номер шага алгоритма, для лога

    public bool HasAnswer => _hasAnswer;    // найдено ли решение
    public IReadOnlyList<Rule> ConfirmedRules => _confirmedRules; // все доказанные правила
    public IReadOnlyList<Rule> DeniedRules => _deniedRules;       // все запрещенные правила
    public IReadOnlyList<Condition> DeniedConditions => _deniedConditions; // все запрещенные вершины

    public List<Condition> Start(List<Rule> rules, List<Condition> closedConditions, Condition endCondition)
    {
        _hasAnswer = false;
        _hasNoAnswer = false;

        // Инициализация списков начальными значениями
        _rules            = rules;
        _closedConditions = closedConditions;
        _deniedConditions = new List<Condition>();
        _openConditions   = new Stack<Condition>();
        _openRules        = new Stack<Rule>();
        _confirmedRules   = new List<Rule>();
        _deniedRules      = new List<Rule>();
        _conditionToRule  = new Dictionary<Condition, Rule>();
        _step             = 0;

        // Инициализация конечной вершины
        _endCondition = endCondition;

        if (_closedConditions.Contains(_endCondition))
        {
            Console.WriteLine($"Цель {_endCondition.Num} уже является фактом, доказывать нечего");
            _hasAnswer = true;
            return _closedConditions;
        }

        // цель - первая подцель, кладем ее на дно стека открытых вершин
        _openConditions.Push(_endCondition);
        Console.WriteLine($"Цель {_endCondition.Num} кладем на дно стека подцелей");
        LogStacks();

        // Запуск метода поиска решения
        SearchPath();

        // Возвращаем решение
        return _closedConditions;
    }

    private void SearchPath()
    {
        while (!_hasAnswer && !_hasNoAnswer) // пока нет информации о том, если ли решение или нет...
        {
            _step++; // для лога шагов

            // если на вершине стека правило, которому нужно выбрать следующую подцель...
            if (_openRules.Count == _openConditions.Count)
            {
                Labeling(); // ...то размечаем его входные вершины
                LogStacks();
                continue;
            }

            // иначе на вершине стека подцель, для которой нужно найти правило
            var g = Descendants(); // находим потомка

            // если нет правил, которыми можно доказать подцель,...
            if (g == 0)
                Backtracking(); // ...то откатываемся назад

            LogStacks();
        }
    }

    private int Descendants()
    {
        var subGoal = _openConditions.Peek(); // текущая подцель - вершина стека открытых вершин
        LogStep($"Descendants: Ищем правило для подцели {subGoal.Num}");

        foreach (var rule in _rules) // проходимся по всем правилам
        {
            if (rule.Out != subGoal) // если правило выводит не подцель - пропуск
                continue;

            if (rule.Mark != RuleMark.None) // если правило уже доказано или запрещено - пропуск
            {
                Log($"{RuleToString(rule)} - пропуск, правило уже {(rule.Mark == RuleMark.Denied ? "запрещено" : "доказано")}");
                continue;
            }

            if (_openRules.Contains(rule)) // если правило уже открыто - пропуск
            {
                Log($"{RuleToString(rule)} - пропуск, правило уже в стеке открытых правил");
                continue;
            }

            bool isRuleAvailable = true; // предполагаем, что правило можно использовать
            foreach (var ruleCondition in rule.In) // для проверки пройдемся по всем входным вершинам
            {
                // если вершина запрещена, то правило доказать невозможно
                // если вершина уже есть в стеке подцелей, то правило приведет к циклу
                if (ruleCondition.Mark == ConditionMark.Denied || _openConditions.Contains(ruleCondition))
                {
                    Log($"{RuleToString(rule)} - пропуск, вход {ruleCondition.Num} " +
                        (ruleCondition.Mark == ConditionMark.Denied ? "запрещен" : "уже в стеке подцелей (цикл)"));
                    isRuleAvailable = false; // в обоих случаях правило не подходит, пропускаем его
                    break;
                }
            }

            if (!isRuleAvailable) // если правило не подходит - пропуск
                continue;

            _openRules.Push(rule); // иначе мы нашли потомка, кладем правило на вершину стека открытых правил
            Log($"{RuleToString(rule)} - выбрано, кладем в стек открытых правил");
            return 1; // в глубину идем только по одному потомку
        }

        // потомков нет
        Log($"Подходящих правил для подцели {subGoal.Num} нет");
        return 0;
    }

    private void Labeling()
    {
        var rule = _openRules.Peek(); // текущее правило - вершина стека открытых правил
        LogStep($"Labeling: Проверяем входы правила {RuleToString(rule)}");

        foreach (var ruleCondition in rule.In) // пройдемся по всем входным вершинам правила
        {
            if (ruleCondition.Mark == ConditionMark.Closed) // если она закрыта, то все хорошо, продолжаем проверку
            {
                Log($"Вход {ruleCondition.Num} закрыт " +
                    (_conditionToRule.TryGetValue(ruleCondition, out var proofRule)
                        ? $"(доказан правилом R{_rules.IndexOf(proofRule)})"
                        : "(начальный факт)"));
                continue;
            }

            // если вход запрещен (его не удалось доказать в другой ветке), то и правило доказать невозможно
            if (ruleCondition.Mark == ConditionMark.Denied)
            {
                _openRules.Pop(); // убираем правило из стека открытых правил
                rule.Mark = RuleMark.Denied; // помечаем правило как запрещенное
                _deniedRules.Add(rule); // записываем его в соответствующий список
                Log($"Вход {ruleCondition.Num} запрещен - ОТКАТ: правило R{_rules.IndexOf(rule)} запрещено");
                Log($"Возвращаемся к подцели {_openConditions.Peek().Num}, для нее будет искаться другое правило");
                return;
            }

            // если она не закрыта, то это новая подцель, кладем ее на вершину стека открытых вершин...
            _openConditions.Push(ruleCondition);
            Log($"Вход {ruleCondition.Num} не доказан - новая подцель, уходим в глубину");
            return; // ...и уходим в глубину
        }

        // все входные вершины закрыты, значит правило доказано
        _openRules.Pop(); // убираем правило из стека открытых правил
        rule.Mark = RuleMark.Confirmed; // пометим правило как подтвержденное
        _confirmedRules.Add(rule); // запишем его в соответствующий список

        _openConditions.Pop(); // подцель доказана, убираем ее из стека открытых вершин
        rule.Out.Mark = ConditionMark.Closed; // закрываем ее
        _closedConditions.Add(rule.Out); // добавляем в соответствующий список
        _conditionToRule[rule.Out] = rule; // запоминаем, каким правилом доказано условие

        Log($"Все входы закрыты - правило R{_rules.IndexOf(rule)} доказано, вершина {rule.Out.Num} закрыта");

        if (rule.Out == _endCondition) // если вершина вывода искомая...
        {
            Log($"Доказана цель {_endCondition.Num} - РЕШЕНИЕ НАЙДЕНО");
            _hasAnswer = true; // .. то есть решение
        }
        else
        {
            Log($"Поднимаемся к правилу R{_rules.IndexOf(_openRules.Peek())}");
        }
    }

    private void Backtracking()
    {
        // подцель доказать невозможно, убираем ее из стека открытых вершин
        var subGoal = _openConditions.Pop();

        subGoal.Mark = ConditionMark.Denied; // помечаем вершину как запрещенную
        _deniedConditions.Add(subGoal); // добавляем в соответствующий список
        Log($"Backtracking: вершина {subGoal.Num} запрещена");

        // если стек открытых вершин опустел, то не доказана сама цель...
        if (_openConditions.Count == 0)
        {
            // ...то нет решений, запоминаем этот вариант
            Log($"Стек подцелей пуст, цель {_endCondition.Num} доказать невозможно - РЕШЕНИЯ НЕТ");
            _hasNoAnswer = true; // помечаем флаг, что РЕШЕНИЯ НЕТ
            return;
        }

        // правило, которое породило эту подцель, тоже доказать невозможно
        var rule = _openRules.Pop(); // убираем его из стека открытых правил

        rule.Mark = RuleMark.Denied; // помечаем правило как запрещенное
        _deniedRules.Add(rule); // записываем его в соответствующий список
        Log($"ОТКАТ: правило R{_rules.IndexOf(rule)}, породившее подцель {subGoal.Num}, запрещено");

        // на вершине стека снова оказывается подцель-родитель, для нее будет искаться другое правило
        Log($"Возвращаемся к подцели {_openConditions.Peek().Num}, для нее будет искаться другое правило");
    }

    // ---------- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ВЫВОДА ----------

    // заголовок шага: номер шага и глубина (число открытых правил)
    private void LogStep(string message)
    {
        Console.WriteLine();
        Console.WriteLine($"Шаг {_step} (глубина {_openRules.Count}). {message}");
    }

    // строка внутри шага
    private void Log(string message)
    {
        Console.WriteLine($"    {message}");
    }

    // текущее состояние стеков (от дна к вершине) и списка закрытых вершин (в порядке закрытия)
    private void LogStacks()
    {
        Log($"Стек подцелей:    [{string.Join(", ", _openConditions.Reverse().Select(c => c.Num))}]");
        Log($"Стек правил:      [{string.Join(", ", _openRules.Reverse().Select(r => $"R{_rules.IndexOf(r)}"))}]");
        Log($"Закрытые вершины: [{string.Join(", ", _closedConditions.Select(c => c.Num))}]");
    }

    // правило в виде "25 <- R1 <- 21 & 22"
    private string RuleToString(Rule rule)
    {
        return $"{rule.Out.Num} <- R{_rules.IndexOf(rule)} <- {string.Join(" & ", rule.In.Select(c => c.Num))}";
    }


    // выводим дерево доказательства от цели к начальным фактам - в том порядке, в котором его строил обратный поиск
    public void PrintSolutionTree()
    {
        if (!_hasAnswer) // если решения нет, то и печатать нечего
            return;

        PrintSolutionTree(_endCondition, 0, new HashSet<Condition>());
    }

    private void PrintSolutionTree(Condition condition, int depth, HashSet<Condition> printed)
    {
        var indent = new string(' ', depth * 4);

        // начальный факт - лист дерева
        if (!_conditionToRule.TryGetValue(condition, out var rule))
        {
            Console.WriteLine($"{indent}{condition.Num} - факт");
            return;
        }

        // вершина уже расписана выше - не повторяем ее поддерево
        if (!printed.Add(condition))
        {
            Console.WriteLine($"{indent}{condition.Num} - доказана выше правилом R{_rules.IndexOf(rule)}");
            return;
        }

        Console.WriteLine($"{indent}{RuleToString(rule)}");
        foreach (var ruleCondition in rule.In)
            PrintSolutionTree(ruleCondition, depth + 1, printed);
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
