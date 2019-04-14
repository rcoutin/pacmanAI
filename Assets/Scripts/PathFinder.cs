using System.Collections;
using System.Collections.Generic;

public class PathFinder
{
    public static List<GraphNode> findPath(GraphNode src, GraphNode dest)
    {
        List<GraphNode> path = new List<GraphNode>();
        if (src == null || dest == null)
        {
            return path;
        }
        HashSet<GraphNode> visited  = new HashSet<GraphNode>();
        Queue<GraphNode> queue = new Queue<GraphNode>();
        queue.Enqueue(src);
        visited.Add(src);
        src.prev = null;
        
        while (queue.Count != 0)
        {
            GraphNode node = queue.Dequeue();
            visited.Add(node);
            if (node == dest)
            {
                while (node.prev != null)
                {
                    node = node.prev;
                    path.Add(node);
                }
                return path;
            }
            foreach (GraphNode n in node.adjacent)
            {
                if (!visited.Contains(n))
                {
                    queue.Enqueue(n);
                    n.prev = node;
                }
            }            
        }
        return path;
    }


    public static List<GraphNode> findClosestPacdot(GraphNode src)
    {
        List<GraphNode> path = new List<GraphNode>();
        if (src == null)
        {
            return path;
        }
        HashSet<GraphNode> visited = new HashSet<GraphNode>();
        Queue<GraphNode> queue = new Queue<GraphNode>();
        queue.Enqueue(src);
        visited.Add(src);
        src.prev = null;

        while (queue.Count != 0)
        {
            GraphNode node = queue.Dequeue();
            visited.Add(node);
            if (node.isPacDot)
            {
                while (node.prev != null)
                {
                    node = node.prev;
                    path.Add(node);
                }
                return path;
            }
            foreach (GraphNode n in node.adjacent)
            {
                if (!visited.Contains(n))
                {
                    queue.Enqueue(n);
                    n.prev = node;
                }
            }
        }
        return path;
    }
}
