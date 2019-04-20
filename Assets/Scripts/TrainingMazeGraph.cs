using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using MLAgents;
using UnityEngine.SceneManagement;
using System.Linq;
using Random = System.Random;

public class TrainingMazeGraph : MazeGraph
{
    //public Dictionary<String, GraphNode> graph;
    int MAX_DIST = 9999;
    int WIDTH = 14;
    int HEIGHT = 12;

    public TrainingMazeGraph() {


        //pick a random pacdot 
    }

    public void destroyPacdotsExceptRandom()
    {
        Random rand = new Random();
       
        List<String> keyList = Enumerable.ToList<String>(graph.Keys);

        String randomKey = keyList[rand.Next(graph.Count)];

        //iterate over all pacdots until this is found and destroy that object
        GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
     
        String[] xy = randomKey.Split(',');
        int dx = Int32.Parse(xy[0]);
        int dy = Int32.Parse(xy[1]);
        if(dx == 13 && dy == 11)
        {
            dx = 10;
        }
        //PrintLog(dx + "," + dy);
        foreach (GameObject pacdot in currentPacdots)
        {

            int px = (int)pacdot.transform.position.x;
            int py = (int)pacdot.transform.position.y;


            if (px != dx || py != dy)
            {
                UnityEngine.Object.Destroy(pacdot);

            }
            else
            {
                //PrintLog("Not destroying random node" + dx + "," + dy);
            }
        }

                GetNode(dx, dy).isPacDot = true;
    }

    public override void initGraph()
    {
        System.Diagnostics.Debug.Print("initializing training graph");

        graph = new Dictionary<String, GraphNode>();

        GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");

        //foreach (GameObject pacdot in currentPacdots)
        //{

        //    int px = (int)pacdot.transform.position.x;
        //    int py = (int)pacdot.transform.position.y;


        //    if (px >= WIDTH || py >=HEIGHT)
        //    {
        //        UnityEngine.Object.Destroy(pacdot);

        //    }
        //}
        System.Diagnostics.Debug.Print(currentPacdots.Length + "");

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
      destroyPacdotsExceptRandom();

    }


    public bool ContainsNode(int i,int j)
    {
        String key = i + "," + j;
        return graph.ContainsKey(key);
    }


   

    public void AddNode(int i,int j)
    {
        String key = i + "," + j;
        graph.Add(key, new GraphNode(i, j, false));

    }

   

    public GraphNode GetNode(int i, int j)
    {
        if (!ContainsNode(i,j)) return null;
        return graph[i+","+j];
    }


}