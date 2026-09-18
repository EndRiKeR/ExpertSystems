using ExpertSystems.graph;

namespace ExpertSystems;

public class SearchInWidth
{
    private List<Edge> _listOfEdges;
    private List<Node> _listOfClosedNodes; // посещённые (уже поставленные в очередь) вершины
    private Dictionary<Node, Node> _parents; // предок каждой вершины, для восстановления пути

    private bool _hasAnswer = false;
    private bool _hasNoAnswer = false;

    private Node _startNode;
    private Node _endNode;

    public Stack<Node> Start(List<Edge> listOfEdges, Node startNode, Node endNode)
    {
        _hasAnswer = false;
        _hasNoAnswer = false;

        // Инициализация списка посещённых вершин и словаря предков
        _listOfClosedNodes = new List<Node>();
        _parents = new Dictionary<Node, Node>();

        // Инициализация списка ребер, начальной и конечной вершин
        _startNode = startNode;
        _endNode = endNode;
        _listOfEdges = listOfEdges;
        for (var i = 0; i < _listOfEdges.Count; i++)
        {
            var edge = _listOfEdges[i];
            edge.Mark = false;
            _listOfEdges[i] = edge;
        }

        // Положил стартовую вершину в очередь открытых вершин
        var listOfOpenNodes = new Queue<Node>();
        listOfOpenNodes.Enqueue(_startNode);
        _listOfClosedNodes.Add(_startNode);

        // Запуск метода поиска решения
        SearchPath(listOfOpenNodes);

        // Восстанавливаем путь по словарю предков и возвращаем решение
        return BuildPath();
    }

    private void SearchPath(Queue<Node> listOfOpenNodes)
    {
        while (!_hasAnswer && !_hasNoAnswer)
        {
            var g = Descendants(listOfOpenNodes);
            if (_hasAnswer) // нашли решение
                return;

            if (g == 0 && listOfOpenNodes.Count == 0)
            {
                // очередь пуста и новых потомков не появилось - решений нет
                _hasNoAnswer = true;
                return;
            }
        }
    }

    private int Descendants(Queue<Node> listOfOpenNodes)
    {
        var g = 0; // число добавленных потомков

        if (listOfOpenNodes.Count == 0)
            return 0;

        // в отличие от поиска в глубину, здесь разбираем СРАЗУ ВСЕ рёбра
        // из текущей вершины, а не только одного потомка за вызов
        var subAim = listOfOpenNodes.Dequeue();

        for (var i = 0; i < _listOfEdges.Count; i++)
        {
            var currentEdge = _listOfEdges[i];

            if (currentEdge.From != subAim)
                continue;

            if (currentEdge.To == _endNode)
            {
                // нашли решение
                _parents[currentEdge.To] = subAim;
                _hasAnswer = true;
                g++;
                break;
            }

            if (!currentEdge.Mark && !_listOfClosedNodes.Contains(currentEdge.To))
            {
                // есть потомок, но он не решение - ставим в очередь
                currentEdge.Mark = true;
                _listOfEdges[i] = currentEdge; // Edge — структура, иначе пометка теряется

                _listOfClosedNodes.Add(currentEdge.To);
                _parents[currentEdge.To] = subAim;
                listOfOpenNodes.Enqueue(currentEdge.To);
                g++;
            }
        }

        // если ни одного нового потомка не нашлось - g остаётся 0
        return g;
    }

    private Stack<Node> BuildPath()
    {
        var result = new Stack<Node>();

        if (!_hasAnswer)
            return result;

        // идём от конечной вершины к начальной по словарю предков
        var path = new List<Node> { _endNode };
        var current = _endNode;
        while (current != _startNode)
        {
            current = _parents[current];
            path.Add(current);
        }

        // разворачиваем (start ... end) и кладём в стек так,
        // чтобы верх стека был конечной вершиной - как и в поиске в глубину
        path.Reverse();
        foreach (var node in path)
            result.Push(node);

        return result;
    }
}