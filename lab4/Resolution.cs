using ExpertSystems.lab4.Logic;

namespace ExpertSystems.lab4;

public class Resolution
{
    private List<Disjunct> _disjuncts;  // множество дизъюнктов: аксиомы, отрицание цели и все полученные резольвенты

    private int _checkedCount;  // сколько дизъюнктов было на прошлой итерации: все пары среди них уже перебраны
    private int _maxIterations; // ограничение числа итераций, чтобы не было комбинаторного взрыва

    private bool _hasAnswer = false;    // true = получен пустой дизъюнкт, следствие доказано; false = пока неизвестно
    private bool _hasNoAnswer = false;  // true = доказать не удалось; false = пока неизвестно

    private Disjunct? _emptyDisjunct;   // пустой дизъюнкт (пустая резольвента), для восстановления доказательства

    private int _step; // номер итерации алгоритма, для лога

    public bool HasAnswer => _hasAnswer;                    // доказано ли следствие
    public IReadOnlyList<Disjunct> Disjuncts => _disjuncts; // все дизъюнкты, включая резольвенты

    public bool Start(List<Formula> axioms, Formula goal, int maxIterations = 10)
    {
        _hasAnswer = false;
        _hasNoAnswer = false;

        // Инициализация списков начальными значениями
        _disjuncts     = new List<Disjunct>();
        _checkedCount  = 0;
        _maxIterations = maxIterations;
        _emptyDisjunct = null;
        _step          = 0;

        // 1. Принимаем отрицание заключения
        var negativeGoal = new Formula(FormulaKind.Not, goal);
        Console.WriteLine($"Отрицание цели: {negativeGoal}");

        // 2. Приводим все аксиомы и отрицание цели к КНФ (конъюнкции дизъюнктов)
        Console.WriteLine();
        Console.WriteLine("Приведение к КНФ:");

        for (var i = 0; i < axioms.Count; i++)
        {
            Log($"Аксиома {i + 1}: {axioms[i]}");
            foreach (var disjunct in CnfConverter.ToCnf(axioms[i])) // одна аксиома может дать несколько дизъюнктов
            {
                disjunct.Num = _disjuncts.Count + 1; // нумеруем дизъюнкты по порядку
                disjunct.Source = $"аксиома {i + 1}";
                _disjuncts.Add(disjunct); // записываем в множество дизъюнктов
            }
        }

        Log($"Отрицание цели: {negativeGoal}");
        foreach (var disjunct in CnfConverter.ToCnf(negativeGoal))
        {
            disjunct.Num = _disjuncts.Count + 1;
            disjunct.Source = "отрицание цели";
            _disjuncts.Add(disjunct);
        }

        // 3. Получили исходное множество дизъюнктов
        Console.WriteLine();
        Console.WriteLine("Исходное множество дизъюнктов:");
        foreach (var disjunct in _disjuncts)
            Log($"D{disjunct.Num}: {disjunct}   ({disjunct.Source})");

        // Запуск метода поиска пустой резольвенты
        Search();

        // Возвращаем ответ: доказано следствие или нет
        return _hasAnswer;
    }

    private void Search()
    {
        while (!_hasAnswer && !_hasNoAnswer) // пока нет информации о том, доказано следствие или нет...
        {
            _step++; // для лога шагов
            var g = Descendants(); // строим резольвенты для текущего множества

            if (_hasAnswer) // если получили пустую резольвенту...
                return;     // ...выход, следствие доказано

            // если ни одной новой резольвенты не получили,...
            if (g == 0)
            {
                // ...то перебирать больше нечего: противоречия в множестве нет
                Log("Новых резольвент нет, пустой дизъюнкт получить невозможно => СЛЕДСТВИЕ НЕ ДОКАЗАНО");
                _hasNoAnswer = true; // помечаем флаг, что РЕШЕНИЯ НЕТ
                return;
            }

            Log($"Получено новых резольвент: {g}, продолжаем перебор на новом множестве");

            // если исчерпали число итераций - останавливаемся, чтобы не было комбинаторного взрыва
            if (_step >= _maxIterations)
            {
                Log($"Достигнуто ограничение в {_maxIterations} итераций => СЛЕДСТВИЕ НЕ ДОКАЗАНО");
                _hasNoAnswer = true;
            }
        }
    }

    private int Descendants()
    {
        int g = 0; // считает количество новых резольвент

        // резольвенты добавляются в то же множество, поэтому запоминаем, сколько дизъюнктов было в начале итерации:
        // новые резольвенты будут участвовать в переборе только со следующей итерации
        var count = _disjuncts.Count;
        LogStep($"Descendants: полный перебор пар для множества из {count} дизъюнктов");

        for (var i = 0; i < count; i++) // первый дизъюнкт пары
        {
            for (var j = i + 1; j < count; j++) // второй дизъюнкт пары
            {
                if (j < _checkedCount) // оба дизъюнкта старые - эту пару уже перебирали на прошлых итерациях
                    continue;

                var first = _disjuncts[i];
                var second = _disjuncts[j];

                foreach (var atom in first.Atoms) // ищем контрарную пару: атом из первого...
                {
                    if (!second.Atoms.Any(a => a.IsContrary(atom))) // ...и он же с другим знаком во втором
                        continue;

                    var resolvent = Resolvent(first, second, atom); // нашли контрарную пару - строим резольвенту
                    var pair = $"D{first.Num} и D{second.Num}, контрарная пара {atom.Name}:";

                    if (resolvent.IsEmpty) // если резольвента пустая...
                    {
                        resolvent.Num = _disjuncts.Count + 1;
                        _disjuncts.Add(resolvent);
                        _emptyDisjunct = resolvent; // запоминаем ее, чтобы восстановить доказательство
                        Log($"{pair} D{resolvent.Num} = {resolvent}");
                        Log("Получена пустая резольвента (ложь) => СЛЕДСТВИЕ ДОКАЗАНО");
                        _hasAnswer = true; // ...то получили противоречие, следствие доказано
                        return g + 1;
                    }

                    if (resolvent.IsTautology()) // резольвента вида A v ~A всегда истинна - пропуск
                    {
                        Log($"{pair} {resolvent} - тавтология, пропускаем");
                        continue;
                    }

                    var same = _disjuncts.FirstOrDefault(d => d.IsSame(resolvent));
                    if (same != null) // такой дизъюнкт уже есть в множестве - пропуск
                    {
                        Log($"{pair} {resolvent} - уже есть (D{same.Num}), пропускаем");
                        continue;
                    }

                    g++; // иначе мы получили новую резольвенту

                    resolvent.Num = _disjuncts.Count + 1; // даем ей следующий номер
                    _disjuncts.Add(resolvent); // добавляем к множеству дизъюнктов
                    Log($"{pair} D{resolvent.Num} = {resolvent}");
                }
            }
        }

        // все пары среди первых count дизъюнктов перебраны
        _checkedCount = count;

        // возвращаем g
        return g;
    }

    // резольвента двух дизъюнктов: объединение их атомов за исключением контрарной пары
    private Disjunct Resolvent(Disjunct first, Disjunct second, Atom atom)
    {
        var resolvent = new Disjunct();

        resolvent.Source = $"резольвента D{first.Num} и D{second.Num}";
        resolvent.Parent1 = first;          // запоминаем родителей, чтобы потом восстановить доказательство
        resolvent.Parent2 = second;
        resolvent.ContraryName = atom.Name; // и атом, по которому резольвировали

        foreach (var a in first.Atoms) // берем атомы первого дизъюнкта...
        {
            if (a.IsSame(atom)) // ...кроме атома из контрарной пары
                continue;

            resolvent.Atoms.Add(a);
        }

        foreach (var a in second.Atoms) // берем атомы второго дизъюнкта...
        {
            if (a.IsContrary(atom)) // ...кроме атома из контрарной пары
                continue;

            if (resolvent.Atoms.Any(x => x.IsSame(a))) // и кроме тех, что уже пришли из первого (A v A = A)
                continue;

            resolvent.Atoms.Add(a);
        }

        return resolvent;
    }

    // ---------- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ВЫВОДА ----------

    // заголовок шага: номер итерации
    private void LogStep(string message)
    {
        Console.WriteLine();
        Console.WriteLine($"Шаг {_step}. {message}");
    }

    // строка внутри шага
    private void Log(string message)
    {
        Console.WriteLine($"    {message}");
    }

    // собираем решение: только те дизъюнкты, которые действительно понадобились для получения пустой резольвенты
    public List<Disjunct> BuildSolution()
    {
        var result = new List<Disjunct>();

        if (!_hasAnswer) // если следствие не доказано, то и строить нечего
            return result;

        // идём от пустого дизъюнкта к исходным по ссылкам на родителей
        var stack = new Stack<Disjunct>();
        stack.Push(_emptyDisjunct!);

        while (stack.Count > 0)
        {
            var disjunct = stack.Pop();
            if (result.Contains(disjunct))
                continue;

            result.Add(disjunct);

            // исходный дизъюнкт (аксиома или отрицание цели) - родителей нет
            if (disjunct.Parent1 == null || disjunct.Parent2 == null)
                continue;

            stack.Push(disjunct.Parent1);
            stack.Push(disjunct.Parent2);
        }

        // порядок - по номерам, то есть в порядке получения
        return result.OrderBy(d => d.Num).ToList();
    }
}
