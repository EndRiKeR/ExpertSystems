using ExpertSystems.lab2;
using ExpertSystems.lab2.Graph;

var graphPaths = args.Length > 0
    ? args
    : new[]
    {
        // @"C:\Work\Repos\ExpertSystems\lab2\Examples\graph.txt",
        // @"C:\Work\Repos\ExpertSystems\lab2\Examples\graphComplex.txt",
        // @"C:\Work\Repos\ExpertSystems\lab2\Examples\graphNoAnswer.txt",
        @"C:\Work\Repos\ExpertSystems\lab2\Examples\graphWide.txt"
    };

foreach (var graphPath in graphPaths)
{
    Console.WriteLine($"########## Граф: {graphPath} ##########");
    Console.WriteLine();

    var graph = RuleGraphReader.ReadFromFile(graphPath);

    // ---------- Поиск в ширину от данных ----------

    Console.WriteLine("=== Поиск в ширину от данных ===");
    Console.WriteLine($"Начальные вершины: {string.Join(", ", graph.ClosedConditions.Select(c => c.Num))}");
    Console.WriteLine($"Целевая вершина: {graph.EndCondition.Num}");

    var search = new SearchInWidthFromData();
    var closedConditions = search.Start(graph.Rules.ToList(), graph.ClosedConditions.ToList(), graph.EndCondition);

    Console.WriteLine($"Закрытые вершины: {string.Join(", ", closedConditions.Select(c => c.Num))}");

    List<Rule>? solution = null;

    if (search.HasAnswer)
    {
        Console.WriteLine("Решение:");
        solution = search.BuildSolution();
        foreach (var rule in solution)
            Console.WriteLine($"R{Array.IndexOf(graph.Rules, rule)}: " +
                              $"{string.Join(" & ", rule.In.Select(c => c.Num))} -> {rule.Out.Num}");
    }
    else
    {
        Console.WriteLine("Для данного графа нет решения");
    }

    var pngPath = Path.ChangeExtension(graphPath, ".width.png");
    RuleDotWriter.RenderToPng(pngPath, graph, search.ConfirmedRules, solution);

    Console.WriteLine($"Картинка: {pngPath}");
    Console.WriteLine();
}
