// using System.Text;
// using ExpertSystems.lab4;
// using ExpertSystems.lab4.Logic;
//
// Console.OutputEncoding = Encoding.UTF8; // чтобы русский текст не превращался в кракозябры
//
// var filePaths = args.Length > 0
//     ? args
//     : new[]
//     {
//         // @"C:\Work\Repos\ExpertSystems\lab4\Examples\example0.txt",
//         // @"C:\Work\Repos\ExpertSystems\lab4\Examples\zenit.txt",
//         // @"C:\Work\Repos\ExpertSystems\lab4\Examples\noAnswer.txt",
//         @"C:\Work\Repos\ExpertSystems\lab4\Examples\test.txt"
//     };
//
// foreach (var filePath in filePaths)
// {
//     Console.WriteLine($"########## Файл: {filePath} ##########");
//     Console.WriteLine();
//
//     var knowledgeBase = FormulaReader.ReadFromFile(filePath);
//
//     Console.WriteLine("=== Метод резолюции ===");
//     Console.WriteLine("Аксиомы:");
//     for (var i = 0; i < knowledgeBase.Axioms.Length; i++)
//         Console.WriteLine($"    {i + 1}. {knowledgeBase.Axioms[i]}");
//     Console.WriteLine($"Цель: {knowledgeBase.Goal}");
//
//     Console.WriteLine();
//     Console.WriteLine("--- Ход поиска ---");
//
//     var resolution = new Resolution();
//     var hasAnswer = resolution.Start(knowledgeBase.Axioms.ToList(), knowledgeBase.Goal);
//
//     Console.WriteLine();
//     Console.WriteLine("--- Итог ---");
//     Console.WriteLine($"Всего дизъюнктов в множестве: {resolution.Disjuncts.Count}");
//
//     if (hasAnswer)
//     {
//         Console.WriteLine($"Цель {knowledgeBase.Goal} ДОКАЗАНА: получен пустой дизъюнкт");
//         Console.WriteLine("Решение (только те дизъюнкты, которые привели к пустому):");
//         foreach (var disjunct in resolution.BuildSolution())
//             Console.WriteLine(disjunct.Parent1 == null
//                 ? $"    D{disjunct.Num}: {disjunct}   ({disjunct.Source})"
//                 : $"    D{disjunct.Num}: {disjunct}   (D{disjunct.Parent1.Num} и D{disjunct.Parent2!.Num}, " +
//                   $"контрарная пара {disjunct.ContraryName})");
//     }
//     else
//     {
//         Console.WriteLine($"Цель {knowledgeBase.Goal} НЕ ДОКАЗАНА: пустой дизъюнкт получить не удалось");
//     }
//
//     Console.WriteLine();
// }
