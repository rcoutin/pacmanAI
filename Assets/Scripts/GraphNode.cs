using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public GraphNode prev;
    int x;
    int y;
    public HashSet<GraphNode> adjacent;

    public GraphNode(int x, int y)
    {
        this.x = x;
        this.y = y;
        adjacent = new HashSet<GraphNode>();
    }

    public void addEdge(GraphNode otherNode)
    {
        if(otherNode != null)
        this.adjacent.Add(otherNode);
    }

    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(GraphNode) && this.x == ((GraphNode)obj).x && this.y == ((GraphNode)obj).y;
    }

    public override int GetHashCode()
    {
        return (int)(this.x + 31 * this.y);
    }

}
