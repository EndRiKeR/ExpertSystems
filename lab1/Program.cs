// using System.Diagnostics;
// using ExpertSystems;
// using ExpertSystems.graph;
//
// var graphPaths = new[]
// {
//     @"C:\Work\Repos\ExpertSystems\Examples\graph.txt",
//     // @"C:\Work\Repos\ExpertSystems\Examples\graph_deadend.txt",
//     // @"C:\Work\Repos\ExpertSystems\Examples\graph_cycle.txt",
//     // @"C:\Work\Repos\ExpertSystems\Examples\graph_nosolution.txt",
//     // @"C:\Work\Repos\ExpertSystems\Examples\graph_shortest_vs_long.txt",
// };
//
// foreach (var graphPath in graphPaths)
// {
//     Console.WriteLine($"########## Граф: {graphPath} ##########");
//     Console.WriteLine();
//
//     var graph = GraphReader.ReadFromFile(graphPath);
//
//     // ---------- Поиск в глубину ----------
//
//     Console.WriteLine("=== Поиск в глубину ===");
//
//     var searchDepth = new SearchInDepth();
//     var answerDepth = searchDepth.Start(graph.Edges.ToList(), graph.StartNode, graph.EndNode);
//
//     List<Node>? pathDepth = null;
//
//     if (answerDepth.Count != 0)
//     {
//         Console.WriteLine("Решение:");
//         pathDepth = answerDepth.Reverse().ToList();
//         foreach (var node in pathDepth)
//             Console.WriteLine(node);
//     }
//     else
//     {
//         Console.WriteLine("Для данного графа нет решения");
//     }
//
//     var dotPathDepth = Path.ChangeExtension(graphPath, ".depth.dot");
//     DotWriter.WriteToFile(dotPathDepth, graph, pathDepth);
//
//     var pngPathDepth = Path.ChangeExtension(graphPath, ".depth.png");
//     Process.Start("dot", $"-Tpng \"{dotPathDepth}\" -o \"{pngPathDepth}\"").WaitForExit();
//
//     Console.WriteLine($"Картинка: {pngPathDepth}");
//     Console.WriteLine();
//
//     // ---------- Поиск в ширину ----------
//
//     Console.WriteLine("=== Поиск в ширину ===");
//
//     var searchWidth = new SearchInWidth();
//     var answerWidth = searchWidth.Start(graph.Edges.ToList(), graph.StartNode, graph.EndNode);
//
//     List<Node>? pathWidth = null;
//
//     if (answerWidth.Count != 0)
//     {
//         Console.WriteLine("Решение:");
//         pathWidth = answerWidth.ToList();
//         foreach (var node in pathWidth)
//             Console.WriteLine(node);
//     }
//     else
//     {
//         Console.WriteLine("Для данного графа нет решения");
//     }
//
//     var dotPathWidth = Path.ChangeExtension(graphPath, ".width.dot");
//     DotWriter.WriteToFile(dotPathWidth, graph, pathWidth);
//
//     var pngPathWidth = Path.ChangeExtension(graphPath, ".width.png");
//     Process.Start("dot", $"-Tpng \"{dotPathWidth}\" -o \"{pngPathWidth}\"").WaitForExit();
//
//     Console.WriteLine($"Картинка: {pngPathWidth}");
//     Console.WriteLine();
// }