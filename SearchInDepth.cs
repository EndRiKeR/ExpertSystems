using System.Diagnostics;
using ExpertSystems.graph;

namespace ExpertSystems;

public class SearchInDepth
{
    private List<Edge> _listOfEdges;
    private List<Node> _listOfClosedNodes;
    private List<Node> _listOfBlockedNodes;

    private bool _hasAnswer = false;
    private bool _hasNoAnswer = false;

    private Node _startNode;
    private Node _endNode;

    public Stack<Node> Start(List<Edge> listOfEdges, Node startNode, Node endNode)
    {
        _hasAnswer = false;
        _hasNoAnswer = false;
        
        // Инициализация списков закрытых и запрещенных вершин
        _listOfClosedNodes = new List<Node>();
        _listOfBlockedNodes = new List<Node>();
        
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
        
        // Положил стартыую вершину в стек открытых вершин
        var listOfOpenNodes = new Stack<Node>();
        listOfOpenNodes.Push(_startNode);

        // Запуск метода поиска решения
        SearchPath(listOfOpenNodes);
        
        // Возвращаем решение
        return listOfOpenNodes;
    }

    private void SearchPath(Stack<Node> listOfOpenNodes)
    {
        while (!_hasAnswer && !_hasNoAnswer)
        {
            var g = Descendants(listOfOpenNodes);
            if (_hasAnswer) // нашли решение
                return;
            
            if (g == 1)
                continue;
            
            if (listOfOpenNodes.Count != 0) // убрал g == 0, заменил на проверку выше
            {
                // бэктрекинг
                var top = listOfOpenNodes.Pop();
                _listOfBlockedNodes.Add(top);
            }
            
            if (listOfOpenNodes.Count == 0)// нет смысла проверять на пустоту, если верхнее условие не выполнено
            {
                // нет решений, запоминаем этот вариант
                _hasNoAnswer = true;
                return;
            }
        }
    }

    private int Descendants(Stack<Node> listOfOpenNodes)
    {
        var g = 0; // число потомков

        for (int i = 0; i < _listOfEdges.Count; i++)
        {
            var currentEdge = _listOfEdges[i];
            if (listOfOpenNodes.Count == 0)
            {
                return 0;
            }
            
            var subAim = listOfOpenNodes.Peek();

            if (currentEdge.From == subAim && currentEdge.To == _endNode)
            {
                g = 1; // есть потомок
                listOfOpenNodes.Push(currentEdge.To); // для вывода решения
                _hasAnswer = true;
                break;
            }
            
            if (currentEdge.From == subAim
                && !currentEdge.Mark
                && !_listOfBlockedNodes.Contains(currentEdge.To)
                && !listOfOpenNodes.Contains(currentEdge.To)) // защита от циклов: вершина уже на пути
            {
                g = 1; // есть потомк, но он не решение
                listOfOpenNodes.Push(currentEdge.To);
                currentEdge.Mark = true;
                _listOfEdges[i] = currentEdge; // Edge — структура, иначе пометка теряется
                break;
            }
        }

        // если все пройдет мимо - количество потомков = 0
        return g;
    }
}