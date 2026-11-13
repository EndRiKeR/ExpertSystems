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
            var g = Descendants(); // находим потомка

            // пока нет правил, которыми можно доказать подцель (и это не сама цель),...
            // откат может каскадом подниматься на несколько уровней
            while (g == 0 && !IsOnlyGoalLeft())
            {
                Backtracking(); // ...откатываемся назад...
                LogStacks();

                _step++;
                g = Descendants(); // ...и ищем альтернативный путь для предыдущей вершины
            }

            // если потомков нет, а в стеке открытых вершин осталась только цель - решения нет
            if (g == 0 && IsOnlyGoalLeft())
            {
                if (_endCondition.Mark != ConditionMark.Denied)
                {
                    _endCondition.Mark = ConditionMark.Denied; // цель тоже запрещена
                    _deniedConditions.Add(_endCondition);
                }
                Log($"В стеке подцелей осталась только цель {_endCondition.Num}, потомков нет => РЕШЕНИЯ НЕТ");
                _hasNoAnswer = true; // помечаем флаг, что РЕШЕНИЯ НЕТ
            }

            LogStacks();
        }
    }

    private int Descendants()
    {
        var subGoal = _openConditions.Peek(); // текущая подцель - голова стека открытых вершин
        LogStep($"Descendants: Ищем правило для подцели {subGoal.Num}");

        // запрещенную подцель раскрыть нельзя (ее уже не удалось доказать в другой ветке)
        if (subGoal.Mark == ConditionMark.Denied)
        {
            Log($"Подцель {subGoal.Num} уже запрещена");
            return 0;
        }
        
        foreach (var rule in _rules) // проходимся по всем правилам
        {
            if (rule.Out != subGoal) // если правило выводит не подцель - пропуск
                continue;

            if (rule.Mark != RuleMark.None) // если метка правила не 0 (открыто, доказано или запрещено) - пропуск
                continue;

            _openRules.Push(rule); // мы нашли потомка, кладем правило в голову стека открытых правил
            rule.Mark = RuleMark.Opened; // метку выбранного правила устанавливаем в 1
            Log($"{RuleToString(rule)} выбрано, кладем в стек открытых правил");

            // новые подцели - входы правила, которые не входят в список закрытых вершин;
            // кладем их по порядку, поэтому первым раскрывается последний вход
            var newSubGoals = rule.In.Where(c => c.Mark != ConditionMark.Closed).ToList();
            foreach (var newSubGoal in newSubGoals)
                _openConditions.Push(newSubGoal);

            foreach (var ruleCondition in rule.In.Where(c => c.Mark == ConditionMark.Closed))
                Log($"Вход {ruleCondition.Num} закрыт " +
                    (_conditionToRule.TryGetValue(ruleCondition, out var proofRule)
                        ? $"(доказан правилом R{proofRule.Num})"
                        : "(начальный факт)"));

            if (newSubGoals.Count > 0)
                Log($"Новые подцели: {string.Join(", ", newSubGoals.Select(c => c.Num))} - уходим в глубину");
            else
                Labeling(); // все входы закрыты - новых подцелей нет, правило доказано

            return 1;
        }

        // потомков нет
        Log($"Подходящих правил для подцели {subGoal.Num} нет");
        return 0;
    }

    private void Labeling()
    {
        var isModuleProved = true; // f_m: правило (модуль) из головы стека доказано

        while (!_hasAnswer && isModuleProved)
        {
            var rule = _openRules.Pop(); // доказанное правило убираем из стека открытых правил...
            rule.Mark = RuleMark.Confirmed; // ...помечаем как доказанное...
            _confirmedRules.Add(rule); // ...и переносим в список закрытых правил

            _openConditions.Pop(); // подцель доказана, убираем ее из стека открытых вершин
            rule.Out.Mark = ConditionMark.Closed; // закрываем ее
            _closedConditions.Add(rule.Out); // добавляем в список закрытых вершин
            _conditionToRule[rule.Out] = rule; // запоминаем, каким правилом доказано условие

            Log($"Labeling: правило R{rule.Num} доказано, вершина {rule.Out.Num} закрыта");

            if (rule.Out == _endCondition) // если вершина вывода искомая...
            {
                Log($"Доказана цель {_endCondition.Num} - РЕШЕНИЕ НАЙДЕНО");
                _hasAnswer = true; // .. то есть решение
                return;
            }

            // подцели, которые оказались доказаны в другой ветке, тоже убираем из стека
            while (_openConditions.Peek().Mark == ConditionMark.Closed)
            {
                var closed = _openConditions.Pop();
                Log($"Подцель {closed.Num} уже доказана правилом R{_conditionToRule[closed].Num}, убираем из стека");
            }

            // в правиле из головы стека проверяем, все ли входные вершины доказаны
            var parentRule = _openRules.Peek();
            isModuleProved = parentRule.In.All(c => c.Mark == ConditionMark.Closed);

            if (isModuleProved)
                Log($"Все входы правила R{parentRule.Num} закрыты - поднимаемся выше");
            else
                Log($"У правила R{parentRule.Num} остались недоказанные входы, следующая подцель {_openConditions.Peek().Num}");
        }
    }

    private void Backtracking()
    {
        // подцель доказать невозможно, убираем ее из стека открытых вершин
        var subGoal = _openConditions.Pop();

        if (subGoal.Mark != ConditionMark.Denied)
        {
            subGoal.Mark = ConditionMark.Denied; // помечаем вершину как запрещенную
            _deniedConditions.Add(subGoal); // добавляем в соответствующий список
        }
        Log($"Backtracking: вершина {subGoal.Num} запрещена");

        // правило, которое породило эту подцель, тоже доказать невозможно
        var rule = _openRules.Pop(); // убираем его из стека открытых правил

        rule.Mark = RuleMark.Denied; // помечаем правило как запрещенное
        _deniedRules.Add(rule); // записываем его в соответствующий список
        Log($"Backtracking: правило R{rule.Num}, породившее подцель {subGoal.Num}, запрещено");

        // убираем из стека остальные подцели, которые положило запрещенное правило: они лежат подряд
        // на вершине стека; ниже могут лежать такие же вершины, но они принадлежат родительскому правилу
        var removed = new List<Condition>();
        while (_openConditions.Count > 0
               && rule.In.Contains(_openConditions.Peek())
               && _openConditions.Peek() != subGoal
               && !removed.Contains(_openConditions.Peek()))
            removed.Add(_openConditions.Pop());

        if (removed.Count > 0)
            Log($"Убираем из стека остальные входы правила R{rule.Num}: {string.Join(", ", removed.Select(c => c.Num))}");
    }

    // в стеке открытых вершин осталась только целевая вершина
    private bool IsOnlyGoalLeft()
    {
        return _openConditions.Count == 1 && _openConditions.Peek() == _endCondition;
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
        Log($"Стек правил:      [{string.Join(", ", _openRules.Reverse().Select(r => $"R{r.Num}"))}]");
        Log($"Закрытые вершины: [{string.Join(", ", _closedConditions.Select(c => c.Num))}]");
    }

    // правило в виде "25 <- R1 <- 21 & 22"
    private string RuleToString(Rule rule)
    {
        return $"{rule.Out.Num} <- R{rule.Num} <- {string.Join(" & ", rule.In.Select(c => c.Num))}";
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
            Console.WriteLine($"{indent}{condition.Num} - доказана выше правилом R{rule.Num}");
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
