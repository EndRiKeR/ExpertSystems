using System;

namespace ExpertSystems.graph;

public struct Node : IEquatable<Node>
{
    public int NodeNum;
    public bool IsClosed;

    public Node(int nodeNum)
    {
        NodeNum = nodeNum;
        IsClosed = false;
    }
    
    public bool Equals(Node other)
    {
        return NodeNum == other.NodeNum;
    }

    public override bool Equals(object? obj)
    {
        return obj is Node other && Equals(other);
    }

    public override int GetHashCode()
    {
        return NodeNum.GetHashCode();
    }
    
    public static bool operator ==(Node left, Node right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Node left, Node right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
        => $"Node({NodeNum})";
}