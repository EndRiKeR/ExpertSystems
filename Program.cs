using System.Diagnostics;
using ExpertSystems;
using ExpertSystems.graph;

var graphPath = args.Length > 0 ? args[0] : @"C:\Work\Repos\ExpertSystems\Examples\graph.txt";
var graph = GraphReader.ReadFromFile(graphPath);

var search = new SearchInDepth();
var answer = search.Start(graph.Edges.ToList(), graph.StartNode, graph.EndNode);

List<Node>? path = null;

if (answer.Count != 0)
{
    Console.WriteLine("Вот решение");
    path = answer.Reverse().ToList();
    foreach (var node in path)
    {
        Console.WriteLine(node);
    }
}
else
{
    Console.WriteLine("Для данного графа нет решения");
}

var dotPath = Path.ChangeExtension(graphPath, ".dot");
DotWriter.WriteToFile(dotPath, graph, path);

Console.WriteLine();
Console.WriteLine($"Граф в формате DOT (сохранён в {dotPath}):");
// dot -Tpng Examples/graph.dot -o Examples/graph.png
