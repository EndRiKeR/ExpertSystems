using ExpertSystems.lab5.Logic;

namespace ExpertSystems.lab5;

public class PredicateResolution
{
    private List<Disjunct> _disjuncts;  // множество дизъюнктов: аксиомы, отрицание цели и все полученные резольвенты

    private int _checkedCount;  // сколько дизъюнктов было на прошлой итерации: все пары среди них уже перебраны
    private int _maxIterations; // ограничение числа итераций, чтобы не было комбинаторного взрыва

    private bool _hasAnswer = false;    // true = получен пустой дизъюнкт, цель доказана; false = пока неизвестно
    private bool _hasNoAnswer = false;  // true = доказать не удалось; false = пока неизвестно

    private Disjunct? _emptyDisjunct;   // пустой дизъюнкт (пустая резольвента), для восстановления доказательства

    private HashSet<string> _variableNames; // имена переменных, которые уже заняты
    private int _variableCount;             // счетчик для новых имен переменных в резольвентах

    private int _step; // номер итерации алгоритма, для лога

    public bool HasAnswer => _hasAnswer;                    // получен ли пустой дизъюнкт
    public IReadOnlyList<Disjunct> Disjuncts => _disjuncts; // все дизъюнкты, включая резольвенты

    public bool Start(List<Disjunct> axioms, List<Disjunct> negativeGoal, int maxIterations = 10)
    {
        _hasAnswer = false;
        _hasNoAnswer = false;

        // Инициализация списков начальными значениями
        _disjuncts     = new List<Disjunct>();
        _checkedCount  = 0;
        _maxIterations = maxIterations;
        _emptyDisjunct = null;
        _variableNames = new HashSet<string>();
        _variableCount = 0;
        _step          = 0;

        // Создаем базу "список дизъюнктов": сначала аксиомы, потом отрицание цели
        foreach (var disjunct in axioms)
        {
            disjunct.Num = _disjuncts.Count + 1; // нумеруем дизъюнкты по порядку
            disjunct.Source = "аксиома";
            _disjuncts.Add(disjunct);
        }

        foreach (var disjunct in negativeGoal)
        {
            disjunct.Num = _disjuncts.Count + 1;
            disjunct.Source = "отрицание цели";
            _disjuncts.Add(disjunct);
        }

        // запоминаем имена переменных исходных дизъюнктов, чтобы новые имена с ними не совпали
        foreach (var disjunct in _disjuncts)
            foreach (var atom in disjunct.Atoms)
                foreach (var term in atom.Terms)
                    if (!term.IsConstant)
                        _variableNames.Add(term.Name);

        Console.WriteLine("Исходное множество дизъюнктов:");
        foreach (var disjunct in _disjuncts)
            Log($"D{disjunct.Num}: {disjunct}   ({disjunct.Source})");

        // Запуск метода поиска пустой резольвенты
        Search();

        // Возвращаем ответ: получен пустой дизъюнкт или нет
        return _hasAnswer;
    }

    private void Search()
    {
        while (!_hasAnswer && !_hasNoAnswer) // пока нет информации о том, доказана цель или нет...
        {
            _step++; // для лога шагов
            var g = Descendants(); // строим резольвенты для текущего множества

            if (_hasAnswer) // если получили пустую резольвенту...
                return;     // ...выход, цель доказана

            // если ни одной новой резольвенты не получили,...
            if (g == 0)
            {
                // ...то перебирать больше нечего
                Log("Новых резольвент нет => ПУСТОЙ ДИЗЪЮНКТ НЕ ПОЛУЧЕН");
                _hasNoAnswer = true; // помечаем флаг, что РЕШЕНИЯ НЕТ
                return;
            }

            Log($"Получено новых резольвент: {g}, продолжаем перебор на новом множестве");

            // если исчерпали число итераций - останавливаемся, чтобы не было комбинаторного взрыва
            if (_step >= _maxIterations)
            {
                Log($"Достигнуто ограничение в {_maxIterations} итераций => ПУСТОЙ ДИЗЪЮНКТ НЕ ПОЛУЧЕН");
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

                for (var a = 0; a < first.Atoms.Count; a++) // атом из первого дизъюнкта...
                {
                    for (var b = 0; b < second.Atoms.Count; b++) // ...и атом из второго
                    {
                        // ищем атомы с одинаковым именем и разным знаком
                        if (!first.Atoms[a].IsContrary(second.Atoms[b]))
                            continue;

                        var pair = $"D{first.Num} и D{second.Num}, атомы {first.Atoms[a]} и {second.Atoms[b]}:";

                        // пробуем унифицировать эти атомы и построить резольвенту
                        var resolvent = Resolvent(first, second, a, b, out var reason);

                        if (resolvent == null) // унификация невозможна - резольвенты нет
                        {
                            Log($"{pair} унификация невозможна ({reason})");
                            continue;
                        }

                        var substitution = $"подстановка {resolvent.Substitution}";

                        if (resolvent.IsEmpty) // если резольвента пустая...
                        {
                            resolvent.Num = _disjuncts.Count + 1;
                            _disjuncts.Add(resolvent);
                            _emptyDisjunct = resolvent; // запоминаем ее, чтобы восстановить доказательство
                            Log($"{pair} {substitution} => D{resolvent.Num} = {resolvent}");
                            Log("Получена пустая резольвента (ложь) => ЦЕЛЬ ДОКАЗАНА");
                            _hasAnswer = true; // ...то получили противоречие, цель доказана
                            return g + 1;
                        }

                        if (resolvent.IsTautology()) // резольвента вида P(x) v ~P(x) всегда истинна - пропуск
                        {
                            Log($"{pair} {substitution} => {resolvent} - тавтология, пропускаем");
                            continue;
                        }

                        var same = _disjuncts.FirstOrDefault(d => d.IsSame(resolvent));
                        if (same != null) // такой дизъюнкт уже есть в множестве - пропуск
                        {
                            Log($"{pair} {substitution} => {resolvent} - уже есть (D{same.Num}), пропускаем");
                            continue;
                        }

                        g++; // иначе мы получили новую резольвенту

                        resolvent.Num = _disjuncts.Count + 1; // даем ей следующий номер
                        _disjuncts.Add(resolvent); // добавляем к множеству дизъюнктов
                        Log($"{pair} {substitution} => D{resolvent.Num} = {resolvent}");
                    }
                }
            }
        }

        // все пары среди первых count дизъюнктов перебраны
        _checkedCount = count;

        // возвращаем g
        return g;
    }

    // метод, который формирует резольвенту двух дизъюнктов по атомам с номерами a и b;
    // возвращает null, если атомы унифицировать невозможно (причина - в reason)
    private Disjunct? Resolvent(Disjunct first, Disjunct second, int a, int b, out string reason)
    {
        // унификация меняет флаги термов, поэтому работаем с копиями дизъюнктов
        var firstCopy = first.Copy();
        var secondCopy = second.Copy();

        // формируем подстановку для контрарных атомов
        var substitution = new List<string>();
        if (!Unify(firstCopy.Atoms[a], secondCopy.Atoms[b], substitution, out reason))
            return null;

        var resolvent = new Disjunct();

        resolvent.Source = $"резольвента D{first.Num} и D{second.Num}";
        resolvent.Parent1 = first;  // запоминаем родителей, чтобы потом восстановить доказательство
        resolvent.Parent2 = second;
        resolvent.Substitution = "{" + string.Join(", ", substitution) + "}"; // и подстановку

        // резольвента - объединение атомов двух дизъюнктов за исключением контрарной пары
        var atoms = new List<Atom>();
        atoms.AddRange(firstCopy.Atoms.Where((_, k) => k != a));
        atoms.AddRange(secondCopy.Atoms.Where((_, k) => k != b));

        // у резольвенты, как у любого дизъюнкта, должны быть свои имена переменных
        var newVariables = new Dictionary<Term, Term>(); // переменная родителя -> новая переменная резольвенты

        foreach (var atom in atoms)
        {
            var newAtom = new Atom(atom.Name, atom.Sign);

            foreach (var term in atom.Terms) // распространяем подстановку в остальные атомы
            {
                var end = term.End(); // идем по связям до конца

                if (end.Flag == TermFlag.Valued) // терм имеет значение - в резольвенту пишем константу
                {
                    newAtom.Terms.Add(new Term(end.Value!, true));
                    continue;
                }

                // иначе это свободная переменная; все связанные с ней переменные заменяются одной новой
                if (!newVariables.ContainsKey(end))
                {
                    string name;
                    do
                        name = $"z{++_variableCount}"; // новое имя, которого еще не было
                    while (!_variableNames.Add(name));

                    newVariables[end] = new Term(name, false);
                }

                newAtom.Terms.Add(newVariables[end]);
            }

            if (resolvent.Atoms.Any(x => x.IsSame(newAtom))) // после подстановки атомы могли совпасть (P v P = P)
                continue;

            resolvent.Atoms.Add(newAtom);
        }

        return resolvent;
    }

    // метод, который формирует подстановку: унифицирует термы, стоящие на одинаковых местах в контрарных атомах;
    // возвращает false, если унификация невозможна
    private bool Unify(Atom first, Atom second, List<string> substitution, out string reason)
    {
        reason = "";

        // первый шаг: у атомов должно быть равное число аргументов
        if (first.Terms.Count != second.Terms.Count)
        {
            reason = "разное число аргументов";
            return false;
        }

        for (var i = 0; i < first.Terms.Count; i++) // просматриваем термы по одному
        {
            // если переменная уже связана на прошлых шагах - берем то, с чем она связана
            var term1 = first.Terms[i].End();
            var term2 = second.Terms[i].End();

            if (term1 == term2) // это уже одна и та же переменная - унифицировать нечего
                continue;

            // 1. оба терма имеют значение (константы): унифицируемы, только если значения совпадают
            if (term1.Flag == TermFlag.Valued && term2.Flag == TermFlag.Valued)
            {
                if (term1.Value == term2.Value)
                    continue;

                reason = $"константы {term1.Value} и {term2.Value} не совпали";
                return false;
            }

            // 2. один терм - переменная, другой - константа: переменная получает значение константы
            if (term2.Flag == TermFlag.Valued)
            {
                term1.Flag = TermFlag.Valued;
                term1.Value = term2.Value;
                substitution.Add($"{term1.Name}/{term2.Value}");
                continue;
            }

            if (term1.Flag == TermFlag.Valued)
            {
                term2.Flag = TermFlag.Valued;
                term2.Value = term1.Value;
                substitution.Add($"{term2.Name}/{term1.Value}");
                continue;
            }

            // 3. оба терма - переменные: они становятся связанными (вторая ссылается на первую)
            term2.Flag = TermFlag.Linked;
            term2.Link = term1;
            substitution.Add($"{term2.Name}/{term1.Name}");
        }

        return true;
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

        if (!_hasAnswer) // если пустой дизъюнкт не получен, то и строить нечего
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
