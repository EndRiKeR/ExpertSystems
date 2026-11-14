using System.Text;
using ExpertSystems.lab5;
using ExpertSystems.lab5.Logic;

Console.OutputEncoding = Encoding.UTF8; // чтобы русский текст не превращался в кракозябры

var filePaths = args.Length > 0
    ? args
    : new[]
    {
        // @"C:\Work\Repos\ExpertSystems\lab5\Examples\unify1.txt",
        // @"C:\Work\Repos\ExpertSystems\lab5\Examples\unify2.txt",
        // @"C:\Work\Repos\ExpertSystems\lab5\Examples\unifyLink.txt",
        // @"C:\Work\Repos\ExpertSystems\lab5\Examples\unifyFail.txt",
        @"C:\Work\Repos\ExpertSystems\lab5\Examples\club.txt"
    };

foreach (var filePath in filePaths)
{
    Console.WriteLine($"########## Файл: {filePath} ##########");
    Console.WriteLine();

    var knowledgeBase = DisjunctReader.ReadFromFile(filePath);

    Console.WriteLine("=== Метод резолюции в логике предикатов ===");
    Console.WriteLine();
    Console.WriteLine("--- Ход поиска ---");

    var resolution = new PredicateResolution();
    var hasAnswer = resolution.Start(knowledgeBase.Axioms.ToList(), knowledgeBase.NegativeGoal.ToList());

    Console.WriteLine();
    Console.WriteLine("--- Итог ---");

    // все резольвенты, которые удалось получить
    var resolvents = resolution.Disjuncts.Where(d => d.Parent1 != null).ToList();
    if (resolvents.Count == 0)
        Console.WriteLine("Резольвент нет: унификация невозможна");
    else
        Console.WriteLine($"Получено резольвент: {resolvents.Count}");

    if (hasAnswer)
    {
        Console.WriteLine("Получен пустой дизъюнкт - ЦЕЛЬ ДОКАЗАНА");
        Console.WriteLine("Решение (только те дизъюнкты, которые привели к пустому):");
        foreach (var disjunct in resolution.BuildSolution())
            Console.WriteLine(disjunct.Parent1 == null
                ? $"    D{disjunct.Num}: {disjunct}   ({disjunct.Source})"
                : $"    D{disjunct.Num}: {disjunct}   (D{disjunct.Parent1.Num} и D{disjunct.Parent2!.Num}, " +
                  $"подстановка {disjunct.Substitution})");
    }
    else
    {
        Console.WriteLine("Пустой дизъюнкт не получен");
        foreach (var disjunct in resolvents)
            Console.WriteLine($"    D{disjunct.Num}: {disjunct}   (D{disjunct.Parent1!.Num} и D{disjunct.Parent2!.Num}, " +
                              $"подстановка {disjunct.Substitution})");
    }

    Console.WriteLine();
}
