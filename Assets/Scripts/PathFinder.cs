using System.Collections;
using System.Collections.Generic;

public class PathFinder
{
    public static List<GraphNode> buildPathFromPrevs(GraphNode node)
    {
        List<GraphNode> path = new List<GraphNode>();
        while (node.prev != null)
        {
            path.Add(node);
            node = node.prev;
        }
        return path;
    }


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
                return buildPathFromPrevs(node);
          
            }
            foreach (GraphNode n in node.adjacent)
            {
                if (!visited.Contains(n))
                {
                    n.prev = node; 
                    queue.Enqueue(n);
                }
            }            
        }
        return path;
    }

    //gets path the closest pacdot
    public static List<GraphNode> findPathToClosestPacdot(GraphNode src)
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
                return buildPathFromPrevs(node);
               
            }
            foreach (GraphNode n in node.adjacent)
            {
                if (!visited.Contains(n))
                {
                    n.prev = node;
                    queue.Enqueue(n);
                }
            }
        }
        return path;
    }


    public static List<List<GraphNode>> expand(GraphNode src, int k)
    {
        List<List<GraphNode>> pathList = new List<List<GraphNode>>();

        if (src == null)
        {
            return pathList;
        }

        HashSet<GraphNode> visited = new HashSet<GraphNode>();
        Queue<GraphNode> queue = new Queue<GraphNode>();
        queue.Enqueue(src);
        visited.Add(src);
        src.prev = null;
        src.level = 0;
        //
        while (queue.Count != 0 && queue.Peek().level < k)
        {
            GraphNode node = queue.Dequeue();
            visited.Add(node);

            foreach (GraphNode n in node.adjacent)
            {
                if (!visited.Contains(n))
                {
                    
                    n.level = node.level + 1;
                    n.prev = node;
                    queue.Enqueue(n);

                }
            }
        }

        while (queue.Count != 0)
        {
            GraphNode node = queue.Dequeue();
            pathList.Add(buildPathFromPrevs(node));
        }
        return pathList;
    }
}
