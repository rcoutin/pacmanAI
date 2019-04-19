using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using MLAgents;
using UnityEngine.SceneManagement;

public class TrainingMazeGraph : MazeGraph
{
    private Dictionary<String, GraphNode> graph;
    int MAX_DIST = 9999;
    int WIDTH = 14;
    int HEIGHT = 12;

    public TrainingMazeGraph()
    {

        graph = new Dictionary<String, GraphNode>();

        GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
        System.Diagnostics.Debug.Print(currentPacdots.Length + "");

        //foreach (GameObject pacdot in currentPacdots)
        //{

        //    if (pacdot.transform.position.x < WIDTH && pacdot.transform.position.y < HEIGHT)
        //    {
        //        if (!ContainsNode(pacdot.transform))
        //        {
        //            AddNode(pacdot.transform);
        //            GetNode(pacdot.transform).isPacDot = false;
        //        }
        //    }
            
        //}

//        graph["4,8"].isPowerUp = true;

        int[,] graph2 = new int[WIDTH+2, HEIGHT+2];
        for (int i = 0; i < graph2.GetLength(0); i++)
        {
            for (int j = 0; j < graph2.GetLength(1); j++)
            {
                graph2[i, j] = -1;
            }
        }

        foreach (GameObject pacdot in currentPacdots)
        {
            int i = (int)pacdot.transform.position.x;
            int j = (int)pacdot.transform.position.y;

            if (i < WIDTH && j < HEIGHT)
                graph2[i, j] = 1;
        }
        //for (int i = 0; i < graph2.GetLength(0); i++)
        //{
        //    for (int j = 0; j < graph2.GetLength(1); j++)
        //    {
        //        System.Diagnostics.Debug.Write(graph2[i,j]);
        //    }
        //    System.Diagnostics.Debug.WriteLine("");
        //}

        for (int i = 1; i < graph2.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < graph2.GetLength(1) - 1; j++)
            {
                if (!ContainsNode(i, j) && graph2[i, j] == 1)
                {
                    AddNode(i, j);
                }
            }
        }

        for (int i = 1; i < graph2.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < graph2.GetLength(1) - 1; j++)
            {
                if (graph2[i, j] == 1)
                {
                    
                    GraphNode node = GetNode(i,j);
                    if (graph2[i + 1, j] == 1)
                    {
                        node.adjacent.Add(graph[(i + 1) + "," + j]);
                    }
                    if (graph2[i, j + 1] == 1)
                    {
                        node.adjacent.Add(graph[(i) + "," + (j + 1)]);
                    }
                    if (graph2[i - 1, j] == 1)
                    {
                        node.adjacent.Add(graph[(i - 1) + "," + j]);
                    }
                    if (graph2[i, j - 1] == 1)
                    {
                        node.adjacent.Add(graph[(i) + "," + (j - 1)]);
                    }

                }
            }
        }

    }
    private String localizedKeyString(Transform transform)
    {
        return (int)transform.position.x + "," + (int)transform.position.y;
    }

    public bool ContainsNode(Transform transform)
    {
        return graph.ContainsKey(localizedKeyString(transform));
    }

    public bool ContainsNode(int i,int j)
    {
        String key = i + "," + j;
        return graph.ContainsKey(key);
    }


    public void AddNode(Transform transform)
    {
        graph.Add(localizedKeyString(transform), new GraphNode((int)transform.position.x, (int)transform.position.y, true));

    }

    public void AddNode(int i,int j)
    {
        String key = i + "," + j;
        graph.Add(key, new GraphNode(i, j, false));

    }

    public GraphNode GetNode(Transform transform)
    {
        if (!ContainsNode(transform)) return null;
        return graph[localizedKeyString(transform)];
    }

    public GraphNode GetNode(int i, int j)
    {
        if (!ContainsNode(i,j)) return null;
        return graph[i+","+j];
    }

    public List<GraphNode> pathFrom(Transform src, Transform dest)
    {
        String srcKey = (int)src.position.x + "," + (int)src.position.y;

        String destKey = (int)dest.position.x + "," + (int)dest.position.y;

        if (!graph.ContainsKey(srcKey) || !graph.ContainsKey(destKey))
        {
            return null;
        }
        GraphNode srcNode = graph[srcKey];
        GraphNode destNode = graph[destKey];
        return PathFinder.findPath(srcNode, destNode);
    }

    public float distFrom(Transform src, Transform dest)
    {
        List<GraphNode> path = pathFrom(src, dest);
        return path == null ? MAX_DIST : path.Count;
    }

    public static void drawPathLines(List<GraphNode> path, Color color, float duration)
    {
        //draw ray from prev path to current path
        for (int i = 1; i < path.Count; i++)
        {
            GraphNode curNode = path[i];
            GraphNode prevNode = path[i - 1];
            Vector2 curVector = new Vector2(curNode.x, curNode.y);
            Vector2 prevVector = new Vector2(prevNode.x, prevNode.y);
            UnityEngine.Debug.DrawLine(curVector, prevVector, color, duration, true);
        }
    }

    //private int totalPacdotNodes()
    //{

    //}

}