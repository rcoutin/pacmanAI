using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using MLAgents;
using UnityEngine.SceneManagement;

public class PlayerController : Agent
{

    public float speed = 0.4f;
    Vector2 _dest = Vector2.zero;
    Vector2 _dir = Vector2.zero;
    Vector2 _nextDir = Vector2.zero;

    public Boolean alive;
    public int prevScore;

    public Transform inky;
    public Transform blinky;
    public Transform pinky;
    public Transform clyde;

    Dictionary<String, GraphNode> new_graph;

    private float inkyDistance = 9999;
    private float blinkyDistance = 9999;
    private float pinkyDistance = 9999;
    private float clydeDistance = 9999;
    private float nearestPacdot = 9999;


    [Serializable]
    public class PointSprites
    {
        public GameObject[] pointSprites;
    }

    public PointSprites points;

    public static int killstreak = 0;

    private List<Transform> nodes;

    // script handles
    private GameGUINavigation GUINav;
    private GameManager GM;
    private ScoreManager SM;

    private bool _deadPlaying = false;

    // Use this for initialization
    void Start()
    {
        GM = GameObject.Find("Game Manager").GetComponent<GameManager>();
        SM = GameObject.Find("Game Manager").GetComponent<ScoreManager>();
        GUINav = GameObject.Find("UI Manager").GetComponent<GameGUINavigation>();
        _dest = transform.position;
        try
        {
            initializeGraph();
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.Print(e.Message);
            System.Diagnostics.Debug.Print(e.StackTrace);
        }
    }

    private void initializeGraph()
    {
        new_graph = new Dictionary<String, GraphNode>();

        GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
        System.Diagnostics.Debug.Print(currentPacdots.Length + "");

        foreach (GameObject pacdot in currentPacdots)
        {
            int x = (int)pacdot.transform.position.x;
            int y = (int)pacdot.transform.position.y;

            if (!new_graph.ContainsKey(x + "," + y))
            {
                new_graph.Add(x + "," + y, new GraphNode(x, y));
            }
            //System.Diagnostics.Debug.Print(x+","+y);
        }

        new_graph.Add(15 + "," + 11, new GraphNode(15, 11));
        new_graph.Add(14 + "," + 11, new GraphNode(14, 11));

        System.Diagnostics.Debug.Print(new_graph.Count + "");

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

                    GraphNode node = new_graph[key];
                    if (graph2[i + 1, j] == 1)
                    {
                        node.adjacent.Add(new_graph[(i + 1) + "," + j]);
                    }
                    if (graph2[i, j + 1] == 1)
                    {
                        node.adjacent.Add(new_graph[(i) + "," + (j + 1)]);
                    }
                    if (graph2[i - 1, j] == 1)
                    {
                        node.adjacent.Add(new_graph[(i - 1) + "," + j]);
                    }
                    if (graph2[i, j - 1] == 1)
                    {
                        node.adjacent.Add(new_graph[(i) + "," + (j - 1)]);
                    }

                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        blinkyDistance = new_graph.ContainsKey((int)blinky.transform.position.x + "," + (int)blinky.transform.position.y) && new_graph.ContainsKey((int)transform.position.x + "," + (int)transform.position.y)
            ? PathFinder.findWeight(new_graph[(int)transform.position.x + "," + (int)transform.position.y], new_graph[(int)blinky.transform.position.x + "," + (int)blinky.transform.position.y])
            : 9999;

        inkyDistance = new_graph.ContainsKey((int)inky.transform.position.x + "," + (int)inky.transform.position.y) && new_graph.ContainsKey((int)transform.position.x + "," + (int)transform.position.y)
            ? PathFinder.findWeight(new_graph[(int)transform.position.x + "," + (int)transform.position.y], new_graph[(int)inky.transform.position.x + "," + (int)inky.transform.position.y])
            : 9999;

        pinkyDistance = new_graph.ContainsKey((int)pinky.transform.position.x + "," + (int)pinky.transform.position.y) && new_graph.ContainsKey((int)transform.position.x + "," + (int)transform.position.y)
            ? PathFinder.findWeight(new_graph[(int)transform.position.x + "," + (int)transform.position.y], new_graph[(int)pinky.transform.position.x + "," + (int)pinky.transform.position.y])
            : 9999;

        clydeDistance = new_graph.ContainsKey((int)clyde.transform.position.x + "," + (int)clyde.transform.position.y) && new_graph.ContainsKey((int)transform.position.x + "," + (int)transform.position.y)
            ? PathFinder.findWeight(new_graph[(int)transform.position.x + "," + (int)transform.position.y], new_graph[(int)clyde.transform.position.x + "," + (int)clyde.transform.position.y])
            : 9999;


        switch (GameManager.gameState)
        {
            case GameManager.GameState.Game:
                ReadInputAndMove();
                Animate();
                break;

            case GameManager.GameState.Dead:
                if (!_deadPlaying)
                    StartCoroutine("PlayDeadAnimation");
                break;
        }

    }

    private float findNearestPacdot()
    {
        float minDist = 9999;
        GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
        foreach (GameObject pacdot in currentPacdots)
        {
            if (pacdot == null) continue;

            int x = (int)pacdot.transform.position.x;
            int y = (int)pacdot.transform.position.y;

            if (new_graph.ContainsKey(x + "," + y) && new_graph.ContainsKey((int)transform.position.x + "," + (int)transform.position.y))
            {
                minDist = Math.Min(minDist, PathFinder.findWeight(new_graph[x + "," + y], new_graph[(int)transform.position.x + "," + (int)transform.position.y]));
            }

            //if(new_graph.ContainsKey(x + "," + y))
            //{
            //    new_graph.Add(x + "," + y, new GraphNode(x, y));
            //}
            //System.Diagnostics.Debug.Print(x+","+y);
        }
        //System.Diagnostics.Debug.WriteLine("MinDist "+minDist);
        return minDist;
    }

    IEnumerator PlayDeadAnimation()
    {
        _deadPlaying = true;
        GetComponent<Animator>().SetBool("Die", true);
        Done();
        yield return new WaitForSeconds(1);
        GetComponent<Animator>().SetBool("Die", false);
        _deadPlaying = false;

        if (GameManager.lives <= 0)
        {
            UnityEngine.Debug.Log("Treshold for High Score: " + SM.LowestHigh());
            if (GameManager.score >= SM.LowestHigh())
                GUINav.getScoresMenu();
            else
                GUINav.H_ShowGameOverScreen();
        }

        else
            GM.ResetScene();
    }

    void Animate()
    {
        Vector2 dir = _dest - (Vector2)transform.position;
        GetComponent<Animator>().SetFloat("DirX", dir.x);
        GetComponent<Animator>().SetFloat("DirY", dir.y);
    }

    bool Valid(Vector2 direction)
    {
        // cast line from 'next to pacman' to pacman
        // not from directly the center of next tile but just a little further from center of next tile
        Vector2 pos = transform.position;
        direction += new Vector2(direction.x * 0.45f, direction.y * 0.45f);
        RaycastHit2D hit = Physics2D.Linecast(pos + direction, pos);
        return hit.collider.name == "pacdot" || (hit.collider == GetComponent<Collider2D>());
    }

    public void ResetDestination()
    {
        _dest = new Vector2(15f, 11f);
        GetComponent<Animator>().SetFloat("DirX", 1);
        GetComponent<Animator>().SetFloat("DirY", 0);
    }

    void ReadInputAndMove()
    {
        // move closer to destination
        Vector2 p = Vector2.MoveTowards(transform.position, _dest, speed);
        GetComponent<Rigidbody2D>().MovePosition(p);

        // get the next direction from keyboard
        //agentMove()
        // if pacman is in the center of a tile
        validateAndChangeDir();
    }

    //bool IsColliderNearby(Vector2 direction, Color color)
    //{
    //    // cast line from 'next to pacman' to pacman
    //    // not from directly the center of next tile but just a little further from center of next tile
    //    Vector2 pos = transform.position;
    //    direction += new Vector2(direction.x * 0.45f, direction.y * 0.45f);
    //    RaycastHit2D hit = Physics2D.Linecast(pos + direction, pos);
    //    PrintLog(hit.collider.name + " " + hit.point + " " + hit.distance);
    //    //UnityEngine.Debug.DrawRay(transform.position, direction * 5, color, 1000000, false);
    //    return hit.collider.name == "maze";
    //}

    void PrintLog(System.Object s)
    {
        System.Diagnostics.Debug.WriteLine(s);

    }

    //void CheckSurroundingWalls()
    //{
    //    //PrintLog("Pacman direction" + _dir);
    //    //PrintLog("Start - Checking walls");
    //    //if (IsColliderNearby(Vector2.right, Color.green)) PrintLog("can't move right");
    //    //if (IsColliderNearby(-Vector2.right, Color.red)) PrintLog("can't move left");
    //    //if (IsColliderNearby(Vector2.up, Color.blue)) PrintLog("Can't move up");
    //    //if (IsColliderNearby(-Vector2.up, Color.yellow)) PrintLog("Can't move down");
        
    //    //PrintLog("Done - checking walls");
    //}

    private void validateAndChangeDir()
    {
        // if pacman is in the center of a tile
        if (Vector2.Distance(_dest, transform.position) < 0.00001f)
        {
            if (Valid(_nextDir))
            {
                _dest = (Vector2)transform.position + _nextDir;
                _dir = _nextDir;
                nearestPacdot = findNearestPacdot();
            }

            else   // if next direction is not valid
            {
                if (Valid(_dir))  // and the prev. direction is valid
                    _dest = (Vector2)transform.position + _dir;   // continue on that direction

                // otherwise, do nothing
            }
        }
    }

    public Vector2 getDir()
    {
        return _dir;
    }

    public void UpdateScore()
    {
        killstreak++;

        // limit killstreak at 4
        if (killstreak > 4) killstreak = 4;

        Instantiate(points.pointSprites[killstreak - 1], transform.position, Quaternion.identity);
        GameManager.score += (int)Mathf.Pow(2, killstreak) * 100;

    }

    private void setActionMask()
    {
        int size = 0;
        Boolean cantGoLeft = !Valid(Vector2.left), cantGoRight = !Valid(Vector2.right);
        size += cantGoLeft ? 1 : 0; size += cantGoRight ? 1 : 0;
        if (size > 0) SetActionMask(0, size == 2 ? new int[] { 1, 2 } : new int[] {cantGoLeft ? 1 : 2 });

        size = 0;
        Boolean cantGoUp = !Valid(Vector2.up), cantGoDown = !Valid(Vector2.down);
        size += cantGoUp ? 1 : 0; size += cantGoDown ? 1 : 0;
        if (size > 0) SetActionMask(1, size == 2 ? new int[] { 1, 2 } : new int[] { cantGoUp ? 1 : 2 });
    }

    public override void CollectObservations()
    {
        setActionMask();
       // AddVectorObs(gameObject.transform.position.x);
        //AddVectorObs(gameObject.transform.position.y);
        AddVectorObs(_dir.x);
        AddVectorObs(_dir.y);
        AddVectorObs(nearestPacdot);
        //AddVectorObs(inkyDistance);
        //AddVectorObs(blinkyDistance);
        //AddVectorObs(pinkyDistance);
        //AddVectorObs(clydeDistance);
    }

    public float euclideanDistance(float x1, float y1, float x2, float y2)
    {
        return (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }

    public override void AgentReset()
    {
        //UnityEngine.Debug.Log("Sup4");
        //GM.ResetScene();
        //Start();
        //GameManager.gameState = GameManager.GameState.Game;
        //GameGUINavigation gui = GameObject.FindObjectOfType<GameGUINavigation>();
        //gui.H_ShowReadyScreen();

        //System.Diagnostics.Debug.WriteLine("Agent Reset");
    }

    public override void AgentOnDone()
    {
        //System.Diagnostics.Debug.WriteLine("Agent Done");
        base.AgentOnDone();

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Walls")
        {
            _nextDir = -_nextDir;
        }

    }

    public void agentMove(float[] action)
    {
        if (action[0] == 2) _nextDir = Vector2.right;
        if (action[0] == 1) _nextDir = -Vector2.right;
        if (action[1] == 1) _nextDir = Vector2.up;
        if (action[1] == 2) _nextDir = -Vector2.up;
    }

    public float distanceToPackDotReward()
    {
        float distanceToPacdot = nearestPacdot;

        if(distanceToPacdot > 20)
        {
            return -0.2f;
        }
        if(distanceToPacdot > 10)
        {
            return -0.05f;
        }
        if(distanceToPacdot < 10)
        {
            return 0.2f;
        }
        return 0.0f;

    }

    private float scoreReward(float curScore,float prevScore)
    {
        if (curScore - prevScore > 0) return 0.3f;

        if (nearestPacdot < 10) return -0.25f;
        return -0.05f;
    }
    private float distanceFromGhostReward()
    {
        float[] ghostDistances = { inkyDistance, blinkyDistance, pinkyDistance,
        clydeDistance};
        float total = 0.0f;
        foreach (float distance in ghostDistances)
        {
            if (distance < 1000)
            {
                total += (distance - 7) / 20;
            }
        }
        return total;
    }
    public override void AgentAction(float[] action, String textAction)
    {
        int currentScore = GameManager.score;
        agentMove(action);
        if (GameManager.gameState == GameManager.GameState.Dead)
        {
            AddReward(-1.0f);
            Done();
        }
        else
        {
            AddReward(scoreReward(currentScore, prevScore));
            //reward to stay alive
            //AddReward(0.002f);
            AddReward(distanceFromGhostReward());
            AddReward(distanceToPackDotReward());
        }
        if (prevScore % 100 == 0) Done();
        prevScore = currentScore;
    }

}
