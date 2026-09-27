using ExpertSystems.graph;

namespace ExpertSystems;

public class SearchInDepth
{
    private List<Edge> _listOfEdges;        // список всех ребер
    private List<Node> _listOfClosedNodes;  // вершины, в которых мы уже побывали
    private List<Node> _listOfBlockedNodes; // тупиковые вершины

    private bool _hasAnswer = false;    // true = решение есть; false = пока неизвествно
    private bool _hasNoAnswer = false;  // true = решения нет; false = пока неизвествно

    private Node _startNode;    // начальная вершина
    private Node _endNode;      // конечная вершина

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

        // Кладем стартовую вершину в стек открытых вершин
        var listOfOpenNodes = new Stack<Node>();
        listOfOpenNodes.Push(_startNode);
        _listOfClosedNodes.Add(_startNode);

        // Запуск метода поиска решения
        SearchPath(listOfOpenNodes);
        
        // Возвращаем решение
        return listOfOpenNodes;
    }

    private void SearchPath(Stack<Node> listOfOpenNodes)
    {
        while (!_hasAnswer && !_hasNoAnswer) // пока нет информации о том, если ли решение или нет...
        {
            var g = Descendants(listOfOpenNodes); // находим потомков текущей вершины
            if (_hasAnswer) // если нашли решение...
                return;     // ...выход

            if (g != 0)     // если есть потомок...
                continue;   // ...продолжаем работу
            
            // если дошли до сюда, то точно известно, что потомков нет
            // либо в тупике, либо нет виршин для исследования
            
            // запрещаем тупиковую вершину, если такая есть
            if (listOfOpenNodes.Count != 0) 
            {
                // бэктрекинг
                var top = listOfOpenNodes.Pop();
                _listOfBlockedNodes.Add(top);
            }
            
            // если после удаления кончились вершины для исследования...
            if (listOfOpenNodes.Count == 0)
            {
                // ...то нет решений, запоминаем этот вариант
                _hasNoAnswer = true; // помечаем флаг, что РЕШЕНИЯ НЕТ
                return;
            }
        }
    }

    private int Descendants(Stack<Node> listOfOpenNodes)
    {
        // если нет вершин для исследования - выход
        if (listOfOpenNodes.Count == 0)
            return 0;
        
        
        var g = 0; // число потомков
        var subAim = listOfOpenNodes.Peek(); // берем вершину как подцель (но не исключаем ее из стека)

        for (int i = 0; i < _listOfEdges.Count; i++) // пробегаемя по ребрам
        {
            var currentEdge = _listOfEdges[i]; // берем ребро

            // если начало ребра соответствует подцели и конец ребра соответствует главной цели...
            if (currentEdge.From == subAim && currentEdge.To == _endNode)
            {
                g = 1; // есть потомок, но это не так важно для проверки
                listOfOpenNodes.Push(currentEdge.To); // добавляем финальную цель в стек для красивого вывода
                _hasAnswer = true; // ставим флаг, что РЕШЕНИЕ ЕСТЬ
                break;
            }
            
            if (currentEdge.From == subAim                          // если начало ребра соответствует подцели...
                && !currentEdge.Mark                                // ...и ребро не помечено...
                && !_listOfBlockedNodes.Contains(currentEdge.To)    // ...и конец ребра не входит в запрещенные вершины...
                && !_listOfClosedNodes.Contains(currentEdge.To))    // ...и не входит в закрытые вершины
            {
                g = 1; // есть потомок, но он не решение
                listOfOpenNodes.Push(currentEdge.To); // добавляем потомка в списко вершин для осмотра
                _listOfClosedNodes.Add(currentEdge.To); // помечаем вершину как закрытую, ибо мы в нее зашли
                currentEdge.Mark = true; // помечаем ребро как пройденное
                _listOfEdges[i] = currentEdge; // Edge — структура, потому сохраняем так для сохранения метки
                break;
            }
        }

        // если все пройдет мимо - количество потомков = 0
        return g;
    }
}