namespace ExpertSystems.graph;

public class Graph
{
    public Node[] Nodes { get; private set; }
    public Edge[] Edges { get; private set; }
    public Node StartNode { get; private set; }
    public Node EndNode { get; private set; }

    public Graph() {}

    public Graph(Node[] nodes, Edge[] edges, Node startNode, Node endNode)
    {
        Nodes = nodes;
        Edges = edges;
        StartNode = startNode;
        EndNode = endNode;
    }

    public Node[] CreateNodes(int numOfNodes)
    {
        var nodes = new Node[numOfNodes];
        for (var i = 0; i < nodes.Length; i++)
            nodes[i] = new Node(i);
        
        return nodes;
    }
}