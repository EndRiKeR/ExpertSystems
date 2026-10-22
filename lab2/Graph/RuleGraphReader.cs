using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExpertSystems.lab2.Graph;

public static class RuleGraphReader
{
    public static RuleGraph ReadFromFile(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return ReadFromReader(reader);
    }

    public static RuleGraph ReadFromReader(TextReader reader)
    {
        // Вершины собираются из входов и выходов правил по уникальным номерам.
        var conditions = new Dictionary<int, Condition>();
        Condition GetOrCreateCondition(int num)
        {
            if (!conditions.TryGetValue(num, out var condition))
            {
                condition = new Condition { Num = num };
                conditions.Add(num, condition);
            }
            return condition;
        }

        Condition GetCondition(int num, string context)
        {
            if (!conditions.TryGetValue(num, out var condition))
                throw new FormatException($"Вершина {num} в \"{context}\" не встречается в правилах.");
            return condition;
        }

        // 1. Правила: номер правила, выходная вершина, входные вершины.
        var numRules = ReadInts(reader, "Ожидалось число правил.")[0];
        if (numRules < 0)
            throw new FormatException("Число правил не может быть отрицательным.");

        var rules = new Rule[numRules];
        var ruleNumbers = new HashSet<int>();
        for (var i = 0; i < numRules; i++)
        {
            var parts = ReadInts(reader, $"Не хватает строк с правилами (ожидалось {numRules}).");
            if (parts.Length < 2)
                throw new FormatException($"Строка правила #{i + 1} должна содержать номер правила и выходную вершину.");

            var ruleNum = parts[0];
            if (!ruleNumbers.Add(ruleNum))
                throw new FormatException($"Номер правила R{ruleNum} повторяется.");

            var outNum = parts[1];
            var output = GetOrCreateCondition(outNum);

            var ins = new Condition[parts.Length - 2];
            for (var j = 2; j < parts.Length; j++)
            {
                ins[j - 2] = GetOrCreateCondition(parts[j]);
            }

            rules[i] = new Rule(ruleNum, RuleMark.None, ins, output);
        }

        // 2. Начальные факты
        var numFacts = ReadInts(reader, "Ожидалось число начальных фактов.")[0];
        if (numFacts < 0 || numFacts > conditions.Count)
            throw new FormatException(
                $"Число начальных фактов {numFacts} вне диапазона [0, {conditions.Count}].");

        var facts = new List<Condition>(numFacts);
        if (numFacts > 0)
        {
            var factsLine = ReadInts(reader, "Не хватает строки с начальными фактами.");
            if (factsLine.Length < numFacts)
                throw new FormatException(
                    $"Ожидалось {numFacts} начальных фактов, найдено {factsLine.Length}.");

            for (var i = 0; i < numFacts; i++)
            {
                var c = GetCondition(factsLine[i], "начальные факты");
                c.Mark = ConditionMark.Closed;
                facts.Add(c);
            }
        }

        // 3. Целевое условие
        var goalLine = ReadInts(reader, "Не хватает строки с целевым условием.");
        if (goalLine.Length < 1)
            throw new FormatException("Строка с целью должна содержать одно число.");
        var goal = GetCondition(goalLine[0], "цель");

        return new RuleGraph(
            conditions.Values.OrderBy(c => c.Num).ToArray(),
            rules,
            facts.ToArray(),
            goal);
    }

    private static int[] ReadInts(TextReader reader, string errorMessage)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("#"))
                continue;

            var tokens = trimmed.Split(
                new[] { ' ', '\t' },
                StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length > 0 && tokens.All(t => int.TryParse(t, out _)))
                return tokens.Select(int.Parse).ToArray();
        }

        throw new FormatException(errorMessage);
    }
}
