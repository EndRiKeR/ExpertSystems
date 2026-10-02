using System.Text;

namespace ExpertSystems.graph;

public static class DotWriter
{
    /// <summary>
    /// Формирует описание графа в формате Graphviz DOT.
    /// Начальная и конечная вершины выделяются цветом,
    /// найденный путь (если он есть) подсвечивается красным.
    /// </summary>
    public static string ToDot(Graph graph, IReadOnlyList<Node>? path = null)
    {
        var pathEdges = new HashSet<(int From, int To)>();
        var pathNodes = new HashSet<int>();

        if (path != null)
        {
            for (var i = 0; i < path.Count; i++)
            {
                pathNodes.Add(path[i].NodeNum);
                if (i + 1 < path.Count)
                    pathEdges.Add((path[i].NodeNum, path[i + 1].NodeNum));
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("    rankdir=LR;");
        sb.AppendLine("    node [shape=circle];");
        sb.AppendLine();

        // Вершины
        foreach (var node in graph.Nodes)
        {
            var attrs = new List<string>();

            if (node == graph.StartNode)
                attrs.Add($"style=filled, fillcolor=lightgreen, label=\"{node.NodeNum}\\n(S)\"");
            else if (node == graph.EndNode)
                attrs.Add($"style=filled, fillcolor=lightblue, label=\"{node.NodeNum}\\n(F)\"");
            else if (pathNodes.Contains(node.NodeNum))
                attrs.Add("style=filled, fillcolor=mistyrose");

            sb.Append("    ").Append(node.NodeNum);
            if (attrs.Count > 0)
                sb.Append(" [").Append(string.Join(", ", attrs)).Append(']');
            sb.AppendLine(";");
        }

        sb.AppendLine();

        // Рёбра
        foreach (var edge in graph.Edges)
        {
            sb.Append("    ").Append(edge.From.NodeNum).Append(" -> ").Append(edge.To.NodeNum);
            if (pathEdges.Contains((edge.From.NodeNum, edge.To.NodeNum)))
                sb.Append(" [color=red, penwidth=2.5]");
            sb.AppendLine(";");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Записывает DOT-описание графа в файл.
    /// </summary>
    public static void WriteToFile(string filePath, Graph graph, IReadOnlyList<Node>? path = null)
    {
        File.WriteAllText(filePath, ToDot(graph, path), new UTF8Encoding(false));
    }
}
