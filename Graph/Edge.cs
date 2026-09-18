namespace ExpertSystems.graph;

public struct Edge
{
    public int EdgeNum;
    public bool Mark;
    
    public Node From;
    public Node To;

    public Edge(Node from, Node to)
    {
        From = from;
        To = to;
    }
}
