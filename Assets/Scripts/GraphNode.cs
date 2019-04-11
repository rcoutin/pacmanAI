using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public GraphNode prev;
    int x;
    int y;
    public HashSet<GraphNode> adjacent = new HashSet<GraphNode>();

    public GraphNode(int x, int y)
    {
        this.x = x;
        this.y = y;

    }

    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(GraphNode) && this.x == ((GraphNode)obj).x && this.y == ((GraphNode)obj).y;
    }

}
