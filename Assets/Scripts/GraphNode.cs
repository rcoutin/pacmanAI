using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode
{
    public GraphNode prev;
    public int x;
    public int y;
    public int level = 0;
    public bool isPacDot;
    public bool isPowerUp;
    public HashSet<GraphNode> adjacent = new HashSet<GraphNode>();

    public GraphNode(int x, int y, bool isPacDot)
    {
        this.x = x;
        this.y = y;
        this.isPacDot = isPacDot;
        isPowerUp = false;
    }

    public override bool Equals(object obj)
    {
        return obj.GetType() == typeof(GraphNode) && this.x == ((GraphNode)obj).x && this.y == ((GraphNode)obj).y;
    }

    public override int GetHashCode()
    {
        return x + (547 * y);
    }

}
