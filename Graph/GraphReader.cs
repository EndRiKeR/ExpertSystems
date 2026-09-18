namespace ExpertSystems.graph;

public class GraphReader
{
    /// <summary>
    /// Формат файла:
    /// N                 (число вершин, отдельной строкой)
    /// M                 (число рёбер, отдельной строкой)
    /// from1 to1         (M строк с рёбрами)
    /// ...
    /// fromM toM
    ///                   (пустые строки допустимы и пропускаются)
    /// S F               (произвольная строка-заголовок — пропускается)
    /// start end         (сама начальная и конечная вершина)
    /// </summary>
    public static Graph ReadFromFile(string filePath)
    {
        using var reader = new StreamReader(filePath);
        return ReadFromReader(reader);
    }

    public static Graph ReadFromReader(TextReader reader)
    {
        var numOfNodes = ReadIntLine(reader, "Ожидалось число вершин.")[0];
        var numOfEdges = ReadIntLine(reader, "Ожидалось число рёбер.")[0];

        var graph = new Graph();
        var nodes = graph.CreateNodes(numOfNodes);
        var edges = new Edge[numOfEdges];

        for (var i = 0; i < numOfEdges; i++)
        {
            var parts = ReadIntLine(reader, $"Не хватает строк с описанием рёбер (ожидалось {numOfEdges}).");
            if (parts.Length < 2)
                throw new FormatException($"Строка ребра #{i + 1} должна содержать две вершины.");

            var from = parts[0];
            var to = parts[1];
            ValidateNode(from, numOfNodes);
            ValidateNode(to, numOfNodes);

            edges[i] = new Edge(nodes[from], nodes[to]);
        }

        var lastParts = ReadIntLine(reader, "Не хватает строки с начальной и конечной вершиной.");
        if (lastParts.Length < 2)
            throw new FormatException("Строка со стартом и финишем должна содержать два числа.");

        var start = lastParts[0];
        var end = lastParts[1];
        ValidateNode(start, numOfNodes);
        ValidateNode(end, numOfNodes);

        return new Graph(nodes, edges, nodes[start], nodes[end]);
    }

    private static void ValidateNode(int nodeNum, int numOfNodes)
    {
        if (nodeNum < 0 || nodeNum >= numOfNodes)
            throw new FormatException($"Номер вершины {nodeNum} вне диапазона [0, {numOfNodes - 1}].");
    }

    /// <summary>
    /// Читает следующую строку, состоящую из целых чисел, пропуская по пути
    /// пустые строки и строки-заголовки вроде "S F", которые числами не являются.
    /// </summary>
    private static int[] ReadIntLine(TextReader reader, string errorMessage)
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length > 0 && tokens.All(t => int.TryParse(t, out _)))
                return tokens.Select(int.Parse).ToArray();

            // строка не состоит из чисел (например, заголовок "S F") — пропускаем её
        }

        throw new FormatException(errorMessage);
    }
}