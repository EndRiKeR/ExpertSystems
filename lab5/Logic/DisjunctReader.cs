namespace ExpertSystems.lab5.Logic;

// Формат файла (пустые строки и строки с # пропускаются):
//   число дизъюнктов аксиом
//   дизъюнкты аксиом, по одному в строке
//   число дизъюнктов отрицания цели (может быть 0)
//   дизъюнкты отрицания цели, по одному в строке
// Дизъюнкт: атомы через v (или |), отрицание - знак ~ перед атомом, например: ~S(x3) v L(x3, Снег)
// Константы пишутся с большой буквы или с цифры, переменные - с маленькой буквы
// В каждом дизъюнкте должны быть свои имена переменных
public static class DisjunctReader
{
    public static KnowledgeBase ReadFromFile(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return ReadFromReader(reader);
    }

    public static KnowledgeBase ReadFromReader(TextReader reader)
    {
        // 1. Дизъюнкты аксиом
        var axioms = ReadDisjuncts(reader, "аксиом");

        // 2. Дизъюнкты отрицания цели
        var negativeGoal = ReadDisjuncts(reader, "отрицания цели");

        return new KnowledgeBase(axioms, negativeGoal);
    }

    // читаем число дизъюнктов и затем сами дизъюнкты
    private static Disjunct[] ReadDisjuncts(TextReader reader, string what)
    {
        var countLine = ReadLine(reader, $"Ожидалось число дизъюнктов {what}.");
        if (!int.TryParse(countLine, out var count) || count < 0)
            throw new FormatException($"Число дизъюнктов {what} задано неверно: \"{countLine}\".");

        var disjuncts = new Disjunct[count];
        for (var i = 0; i < count; i++)
            disjuncts[i] = Parse(ReadLine(reader, $"Не хватает строк с дизъюнктами {what} (ожидалось {count})."));

        return disjuncts;
    }

    // разбор одного дизъюнкта из строки
    public static Disjunct Parse(string text)
    {
        var disjunct = new Disjunct();
        var variables = new Dictionary<string, Term>(); // переменная с одним именем - один и тот же терм во всем дизъюнкте

        var pos = 0; // номер текущего символа

        void SkipSpaces()
        {
            while (pos < text.Length && char.IsWhiteSpace(text[pos]))
                pos++;
        }

        // читаем имя (предиката или терма): буквы, цифры и _
        string ReadName()
        {
            SkipSpaces();
            var start = pos;
            while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_'))
                pos++;

            if (pos == start)
                throw new FormatException($"Ожидалось имя на месте {start + 1} в дизъюнкте \"{text}\".");

            return text.Substring(start, pos - start);
        }

        // проверяем, что дальше идет нужный символ, и проходим его
        void Expect(char c)
        {
            SkipSpaces();
            if (pos >= text.Length || text[pos] != c)
                throw new FormatException($"Ожидался символ '{c}' на месте {pos + 1} в дизъюнкте \"{text}\".");
            pos++;
        }

        while (true)
        {
            // знак атома
            SkipSpaces();
            var sign = true;
            if (pos < text.Length && text[pos] == '~')
            {
                sign = false;
                pos++;
            }

            // имя предиката и его аргументы в скобках
            var atom = new Atom(ReadName(), sign);
            Expect('(');
            while (true)
            {
                var name = ReadName();
                if (char.IsUpper(name[0]) || char.IsDigit(name[0])) // с большой буквы или с цифры - константа
                {
                    atom.Terms.Add(new Term(name, true));
                }
                else // иначе переменная: если такая уже встречалась в дизъюнкте, берем ее же
                {
                    if (!variables.ContainsKey(name))
                        variables[name] = new Term(name, false);
                    atom.Terms.Add(variables[name]);
                }

                SkipSpaces();
                if (pos < text.Length && text[pos] == ',') // после запятой - следующий аргумент
                {
                    pos++;
                    continue;
                }

                break;
            }
            Expect(')');

            disjunct.Atoms.Add(atom);

            // дальше либо конец строки, либо связка v и следующий атом
            SkipSpaces();
            if (pos >= text.Length)
                break;

            if (text[pos] != 'v' && text[pos] != '|')
                throw new FormatException($"Ожидалась связка v на месте {pos + 1} в дизъюнкте \"{text}\".");
            pos++;
        }

        return disjunct;
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
