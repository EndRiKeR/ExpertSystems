using ExpertSystems.graph;

namespace ExpertSystems;

public class SearchInWidth
{
    private List<Edge> _listOfEdges;                    // список всех ребер
    private List<Node> _listOfClosedNodes;              // вершины, в которых мы уже побывали
    private Dictionary<Node, Node> _nodeToParentNode;   // предок каждой вершины, для восстановления пути

    private bool _hasAnswer = false;    // true = решение есть; false = пока неизвествно
    private bool _hasNoAnswer = false;  // true = решения нет; false = пока неизвествно

    private Node _startNode;    // начальная вершина
    private Node _endNode;      // конечная вершина

    public Stack<Node> Start(List<Edge> listOfEdges, Node startNode, Node endNode)
    {
        _hasAnswer = false;
        _hasNoAnswer = false;

        // Инициализация списка посещённых вершин и словаря предков
        _listOfClosedNodes = new List<Node>();
        _nodeToParentNode = new Dictionary<Node, Node>();

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
        while (!_hasAnswer && !_hasNoAnswer) // пока нет информации о том, если ли решение или нет...
        {
            var g = Descendants(listOfOpenNodes); // находим потомков текущей вершины
            if (_hasAnswer) // если нашли решение...
                return;     // ...выход
            
            if (g != 0)     // если есть потомки...
                continue;   // ...продолжаем работу

            // если дошли до сюда, то точно известно, что потомков нет
            // либо в тупике, либо нет виршин для исследования
            // тупик не страшен, ибо могут быть в очереди следующие вершины TODO
            
            // если вершин для исследования больше нет
            if (listOfOpenNodes.Count == 0)
            {
                // ...то нет решений, запоминаем этот вариант
                _hasNoAnswer = true; // помечаем флаг, что РЕШЕНИЯ НЕТ
                return;
            }
        }
    }

    private int Descendants(Queue<Node> listOfOpenNodes)
    {
        // если нет вершин для исследования - выход
        if (listOfOpenNodes.Count == 0)
            return 0;
        
        var g = 0; // число найденных потомков
        var subAim = listOfOpenNodes.Dequeue(); // берем вершину как подцель и удаляем ее из очереди

        for (var i = 0; i < _listOfEdges.Count; i++) // и ищем всех потомков для этой подцели
        {
            var currentEdge = _listOfEdges[i]; // берем ребро
            
            if (currentEdge.From != subAim) // если начало ребра не соответствует подцели...
                continue;                   // ... игнорируем ребро

            // до это точки доходят все ребра с началом равным подцели
            
            if (currentEdge.To == _endNode) // если конец ребра соответствует финальной цели
            {
                _nodeToParentNode[currentEdge.To] = subAim; // записываем, что перед финальной вершиной была вершина подцели
                _hasAnswer = true; // ставим флаг, что РЕШЕНИЕ ЕСТЬ
                g++; // увеличиваем количество найденных потомков, но это не повлияет на работу алгоритма
                break;
            }

            // если ребро не помечено и конец ребра не лежит в закрытых вершинах
            if (!currentEdge.Mark && !_listOfClosedNodes.Contains(currentEdge.To))
            {
                // есть потомок, но он не решение - ставим в очередь
                currentEdge.Mark = true;
                _listOfEdges[i] = currentEdge;

                _listOfClosedNodes.Add(currentEdge.To); // помечаем вершину как закрытую
                _nodeToParentNode[currentEdge.To] = subAim; // записываем, что родителем вершины была подцель
                listOfOpenNodes.Enqueue(currentEdge.To); // добавляем вершину из очереди
                g++; // увеличиваем количество найденных потомков
            }
        }

        // если ни одного нового потомка не нашлось - g остается 0
        return g;
    }

    // собираем решение из словаря родителей
    private Stack<Node> BuildPath()
    {
        var result = new Stack<Node>();

        if (!_hasAnswer) // если решениея нет, то и смысл строить пути тоже нет
            return result;

        // идём от конечной вершины к начальной по словарю предков
        result.Push(_endNode);
        
        var current = _endNode;
        while (current != _startNode)
        {
            current = _nodeToParentNode[current];
            result.Push(current);
        }

        return result;
    }
}