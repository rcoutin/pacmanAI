using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using MLAgents;
using UnityEngine.SceneManagement;

public class MazeGraph
{
    private Dictionary<String, GraphNode> graph;
    int MAX_DIST = 9999;

    public MazeGraph()
    {

        graph = new Dictionary<String, GraphNode>();

        GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
        System.Diagnostics.Debug.Print(currentPacdots.Length + "");

        foreach (GameObject pacdot in currentPacdots)
        {

            if (!ContainsNode(pacdot.transform))
            {
                AddNode(pacdot.transform);
            }
        }

        graph["4,8"].isPowerUp = true;
        graph["4,26"].isPowerUp = true;
        graph["25,26"].isPowerUp = true;
        graph["25,8"].isPowerUp = true;

        graph.Add(15 + "," + 11, new GraphNode(15, 11, false));
        graph.Add(14 + "," + 11, new GraphNode(14, 11, false));

        System.Diagnostics.Debug.Print(graph.Count + "");

        int[,] graph2 = new int[30, 33];
        for (int i = 0; i < graph2.GetLength(0); i++)
        {
            for (int j = 0; j < graph2.GetLength(1); j++)
            {
                graph2[i, j] = -1;
            }
        }
        graph2[15, 11] = 1;
        graph2[14, 11] = 1;

        foreach (GameObject pacdot in currentPacdots)
        {
            int i = (int)pacdot.transform.position.x;
            int j = (int)pacdot.transform.position.y;
            graph2[i, j] = 1;
        }

        for (int i = 1; i < graph2.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < graph2.GetLength(1) - 1; j++)
            {
                if (graph2[i, j] == 1)
                {
                    String key = i + "," + j;

                    GraphNode node = graph[key];
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

    public void AddNode(Transform transform)
    {
        graph.Add(localizedKeyString(transform), new GraphNode((int)transform.position.x, (int)transform.position.y, true));

    }

    public GraphNode GetNode(Transform transform)
    {
        if (!ContainsNode(transform)) return null;
        return graph[localizedKeyString(transform)];
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

}