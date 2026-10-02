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
        // 1. Условия
        var numConditions = ReadInts(reader, "Ожидалось число условий.")[0];
        if (numConditions <= 0)
            throw new FormatException("Число условий должно быть положительным.");

        var conditions = new Condition[numConditions];
        for (var i = 0; i < numConditions; i++)
            conditions[i] = new Condition { Num = i };

        // 2. Правила
        var numRules = ReadInts(reader, "Ожидалось число правил.")[0];
        if (numRules < 0)
            throw new FormatException("Число правил не может быть отрицательным.");

        var rules = new Rule[numRules];
        for (var i = 0; i < numRules; i++)
        {
            var parts = ReadInts(reader, $"Не хватает строк с правилами (ожидалось {numRules}).");
            if (parts.Length < 1)
                throw new FormatException($"Строка правила #{i + 1} пуста.");

            var outNum = parts[0];
            ValidateCondition(outNum, numConditions, $"правило #{i + 1}");

            var ins = new Condition[parts.Length - 1];
            for (var j = 1; j < parts.Length; j++)
            {
                ValidateCondition(parts[j], numConditions, $"правило #{i + 1}");
                ins[j - 1] = conditions[parts[j]];
            }

            rules[i] = new Rule(RuleMark.None, ins, conditions[outNum]);
        }

        // 3. Начальные факты
        var numFacts = ReadInts(reader, "Ожидалось число начальных фактов.")[0];
        if (numFacts < 0 || numFacts > numConditions)
            throw new FormatException(
                $"Число начальных фактов {numFacts} вне диапазона [0, {numConditions}].");

        var facts = new List<Condition>(numFacts);
        if (numFacts > 0)
        {
            var factsLine = ReadInts(reader, "Не хватает строки с начальными фактами.");
            if (factsLine.Length < numFacts)
                throw new FormatException(
                    $"Ожидалось {numFacts} начальных фактов, найдено {factsLine.Length}.");

            for (var i = 0; i < numFacts; i++)
            {
                ValidateCondition(factsLine[i], numConditions, "начальные факты");
                var c = conditions[factsLine[i]];
                c.Mark = ConditionMark.Closed;
                facts.Add(c);
            }
        }

        // 4. Целевое условие
        var goalLine = ReadInts(reader, "Не хватает строки с целевым условием.");
        if (goalLine.Length < 1)
            throw new FormatException("Строка с целью должна содержать одно число.");
        ValidateCondition(goalLine[0], numConditions, "цель");

        return new RuleGraph(
            conditions,
            rules,
            facts.ToArray(),
            conditions[goalLine[0]]);
    }

    private static void ValidateCondition(int num, int total, string context)
    {
        if (num < 0 || num >= total)
            throw new FormatException(
                $"Номер условия {num} в \"{context}\" вне диапазона [0, {total - 1}].");
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