using System.Collections;
using System.Collections.Generic;

public class PathFinder
{
    public static int findWeight(GraphNode src, GraphNode dest)
    {

        if (src == null || dest == null)
        {
            return 9999;
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
                int size = 0;
                while (node.prev != null)
                {
                    node = node.prev;
                    size++;
                }
                return size;
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
        return 9999;
    }
}
