namespace ExpertSystems.lab4.Logic;

// Формат файла (пустые строки и строки с # пропускаются):
//   число аксиом
//   аксиомы, по одной формуле в строке
//   цель - одна формула
// Связки: ~ отрицание, & конъюнкция, v (или |) дизъюнкция, -> импликация, <-> эквивалентность, скобки ( )
// Атомы - любые имена из букв и цифр, кроме одиночной буквы v (она занята под дизъюнкцию)
public static class FormulaReader
{
    public static KnowledgeBase ReadFromFile(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return ReadFromReader(reader);
    }

    public static KnowledgeBase ReadFromReader(TextReader reader)
    {
        // 1. Аксиомы: сначала их число, затем сами формулы
        var countLine = ReadLine(reader, "Ожидалось число аксиом.");
        if (!int.TryParse(countLine, out var numAxioms) || numAxioms < 0)
            throw new FormatException($"Число аксиом задано неверно: \"{countLine}\".");

        var axioms = new Formula[numAxioms];
        for (var i = 0; i < numAxioms; i++)
            axioms[i] = Parse(ReadLine(reader, $"Не хватает строк с аксиомами (ожидалось {numAxioms})."));

        // 2. Цель
        var goal = Parse(ReadLine(reader, "Не хватает строки с целью."));

        return new KnowledgeBase(axioms, goal);
    }

    // разбор одной формулы из строки
    public static Formula Parse(string text)
    {
        // делим строку на лексемы: скобки, связки и имена атомов
        var tokens = new List<string>();
        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
            }
            else if (text.Substring(i).StartsWith("<->"))
            {
                tokens.Add("<->");
                i += 3;
            }
            else if (text.Substring(i).StartsWith("->"))
            {
                tokens.Add("->");
                i += 2;
            }
            else if ("()~&|".Contains(c))
            {
                tokens.Add(c == '|' ? "v" : c.ToString());
                i++;
            }
            else if (char.IsLetterOrDigit(c) || c == '_')
            {
                var start = i;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
                    i++;
                tokens.Add(text.Substring(start, i - start));
            }
            else
            {
                throw new FormatException($"Непонятный символ '{c}' в формуле \"{text}\".");
            }
        }

        var pos = 0; // номер текущей лексемы
        string Current() => pos < tokens.Count ? tokens[pos] : "";

        // разбор идет от самой слабой связки к самой сильной: <->, ->, v, &, ~
        Formula ParseEquiv()
        {
            var left = ParseImplies();
            while (Current() == "<->")
            {
                pos++;
                left = new Formula(FormulaKind.Equiv, left, ParseImplies());
            }
            return left;
        }

        Formula ParseImplies()
        {
            var left = ParseOr();
            if (Current() != "->")
                return left;

            pos++;
            return new Formula(FormulaKind.Implies, left, ParseImplies()); // A -> B -> C читаем как A -> (B -> C)
        }

        Formula ParseOr()
        {
            var left = ParseAnd();
            while (Current() == "v")
            {
                pos++;
                left = new Formula(FormulaKind.Or, left, ParseAnd());
            }
            return left;
        }

        Formula ParseAnd()
        {
            var left = ParseNot();
            while (Current() == "&")
            {
                pos++;
                left = new Formula(FormulaKind.And, left, ParseNot());
            }
            return left;
        }

        Formula ParseNot()
        {
            var token = Current();
            pos++;

            if (token == "~")
                return new Formula(FormulaKind.Not, ParseNot());

            if (token == "(")
            {
                var inner = ParseEquiv();
                if (Current() != ")")
                    throw new FormatException($"Не хватает закрывающей скобки в формуле \"{text}\".");
                pos++;
                return inner;
            }

            if (token.Length > 0 && (char.IsLetterOrDigit(token[0]) || token[0] == '_') && token != "v")
                return new Formula(token);

            throw new FormatException($"Ожидался атом, а встретилось \"{token}\" в формуле \"{text}\".");
        }

        var formula = ParseEquiv();
        if (pos < tokens.Count)
            throw new FormatException($"Лишняя лексема \"{tokens[pos]}\" в формуле \"{text}\".");

        return formula;
    }

    // читаем следующую непустую строку, комментарии (#) пропускаем
    private static string ReadLine(TextReader reader, string errorMessage)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("#"))
                continue;

            return trimmed;
        }

        throw new FormatException(errorMessage);
    }
}
