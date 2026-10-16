using System.Text;

namespace ExpertSystems.lab2.Graph;

public static class RuleDotWriter
{
    /// <summary>
    /// Формирует описание графа правил в формате Graphviz DOT.
    /// Условия — круги, правила — прямоугольники (входы -> правило -> выход).
    /// Начальные факты и цель выделяются цветом, доказанные условия и правила
    /// закрашиваются, правила из найденного решения подсвечиваются красным.
    /// </summary>
    public static string ToDot(
        RuleGraph graph,
        IEnumerable<Rule>? confirmedRules = null,
        IEnumerable<Rule>? solution = null)
    {
        var confirmed = new HashSet<Rule>(confirmedRules ?? Enumerable.Empty<Rule>());
        var solutionRules = new HashSet<Rule>(solution ?? Enumerable.Empty<Rule>());
        var facts = new HashSet<Condition>(graph.ClosedConditions);
        var proved = new HashSet<Condition>(confirmed.Select(r => r.Out));

        var sb = new StringBuilder();
        sb.AppendLine("digraph G {");
        sb.AppendLine("    rankdir=LR;");
        sb.AppendLine("    node [shape=circle];");
        sb.AppendLine();

        // Условия
        foreach (var condition in graph.Conditions)
        {
            sb.Append("    c").Append(condition.Num);

            if (facts.Contains(condition))
                sb.Append($" [style=filled, fillcolor=lightgreen, label=\"{condition.Num}\\n(S)\"]");
            else if (condition == graph.EndCondition)
                sb.Append($" [style=filled, fillcolor=lightblue, label=\"{condition.Num}\\n(F)\"" +
                          (proved.Contains(condition) ? ", penwidth=2.5, color=red" : "") + "]");
            else if (proved.Contains(condition))
                sb.Append($" [style=filled, fillcolor=mistyrose, label=\"{condition.Num}\"]");
            else
                sb.Append($" [label=\"{condition.Num}\"]");

            sb.AppendLine(";");
        }

        sb.AppendLine();

        // Правила и рёбра
        for (var i = 0; i < graph.Rules.Length; i++)
        {
            var rule = graph.Rules[i];
            var inSolution = solutionRules.Contains(rule);
            var edgeAttrs = inSolution ? " [color=red, penwidth=2.5]" : "";

            sb.Append("    r").Append(i).Append($" [shape=box, label=\"R{i}\"");
            if (inSolution)
                sb.Append(", style=filled, fillcolor=mistyrose, color=red, penwidth=2.5");
            else if (confirmed.Contains(rule))
                sb.Append(", style=filled, fillcolor=lightyellow");
            sb.AppendLine("];");

            foreach (var condition in rule.In)
                sb.Append("    c").Append(condition.Num).Append(" -> r").Append(i).Append(edgeAttrs).AppendLine(";");

            sb.Append("    r").Append(i).Append(" -> c").Append(rule.Out.Num).Append(edgeAttrs).AppendLine(";");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Рендерит граф в PNG через Graphviz, передавая DOT-описание в stdin
    /// (промежуточный .dot-файл не сохраняется).
    /// </summary>
    public static void RenderToPng(
        string pngPath,
        RuleGraph graph,
        IEnumerable<Rule>? confirmedRules = null,
        IEnumerable<Rule>? solution = null)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo("dot", $"-Tpng -o \"{pngPath}\"")
        {
            RedirectStandardInput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            StandardInputEncoding = new UTF8Encoding(false),
        };

        using var process = System.Diagnostics.Process.Start(startInfo)
                            ?? throw new InvalidOperationException("Не удалось запустить Graphviz (dot).");

        process.StandardInput.Write(ToDot(graph, confirmedRules, solution));
        process.StandardInput.Close();

        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Graphviz (dot) завершился с ошибкой: {error}");
    }
}
