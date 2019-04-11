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
    public int score;

    public Transform inky;
    public Transform blinky;
    public Transform pinky;
    public Transform clyde;

    Dictionary<String, GraphNode> new_graph;

    public float inkyDistance = 9999;
    public float blinkyDistance = 9999;
    public float pinkyDistance = 9999;
    public float clydeDistance = 9999;

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
        catch (Exception e) {
            System.Diagnostics.Debug.Print(e.Message);
            System.Diagnostics.Debug.Print(e.StackTrace);
        }
    }

    private void initializeGraph()
    {
        new_graph = new Dictionary<String, GraphNode>();

        GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
        System.Diagnostics.Debug.Print(currentPacdots.Length+"");

        foreach (GameObject pacdot in currentPacdots)
        {
            int x = (int)pacdot.transform.position.x;
            int y = (int)pacdot.transform.position.y;

            if (!new_graph.ContainsKey(x + "," + y))
            {
                new_graph.Add(x+","+y, new GraphNode(x, y));
            }
            //System.Diagnostics.Debug.Print(x+","+y);
        }

        new_graph.Add(15+","+11, new GraphNode(15, 11));
        new_graph.Add(14+","+11, new GraphNode(14, 11));

        System.Diagnostics.Debug.Print(new_graph.Count+"");

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

        for (int i = 1; i < graph2.GetLength(0)-1; i++)
        {
            for (int j = 1; j < graph2.GetLength(1)-1; j++)
            {
                if (graph2[i,j] == 1)
                {
                    String key = i+","+j;

                    GraphNode node = new_graph[key];
                    if (graph2[i + 1, j] == 1)
                    {
                        node.adjacent.Add(new_graph[(i + 1) + "," + j]);
                    }
                    if (graph2[i, j+1] == 1)
                    {
                        node.adjacent.Add(new_graph[(i ) + "," + (j+1)]);
                    }
                    if (graph2[i - 1, j] == 1)
                    {
                        node.adjacent.Add(new_graph[(i - 1) + "," + j]);
                    }
                    if (graph2[i , j-1] == 1)
                    {
                        node.adjacent.Add(new_graph[(i ) + "," + (j-1)]);
                    }
                
                }
            }
        }
    }

    //private void initializeGraph()
    //{
    //    int[,] graph = new int[30, 33];
    //    for (int i = 0; i < graph.GetLength(0); i++)
    //    {
    //        for (int j = 0; j < graph.GetLength(1); j++)
    //        {
    //            graph[i, j] = -1;
    //        }
    //    }

    //    GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
    //    foreach (GameObject pacdot in currentPacdots)
    //    {
    //        int i = (int)pacdot.transform.position.x;
    //        int j = (int)pacdot.transform.position.y;
    //        graph[i, j] = 1;
    //    }
    //    //for (int i = 0; i < graph.GetLength(0); i++)
    //    //{
    //    //    for (int j = 0; j < graph.GetLength(1); j++)
    //    //    {
    //    //        System.Diagnostics.Debug.Write(graph[i, j] + "");
    //    //    }
    //    //    System.Diagnostics.Debug.WriteLine("");
    //    //}
    //}

    // Update is called once per frame
    void FixedUpdate()
    {
        blinkyDistance = new_graph.ContainsKey((int)blinky.transform.position.x + "," + (int)blinky.transform.position.y)
            ? PathFinder.findWeight(new_graph[(int)transform.position.x + "," + (int)transform.position.y], new_graph[(int)blinky.transform.position.x + "," + (int)blinky.transform.position.y])
            : 9999;

        inkyDistance = new_graph.ContainsKey((int)inky.transform.position.x + "," + (int)inky.transform.position.y)
            ? PathFinder.findWeight(new_graph[(int)transform.position.x + "," + (int)transform.position.y], new_graph[(int)inky.transform.position.x + "," + (int)inky.transform.position.y])
            : 9999;

        pinkyDistance = new_graph.ContainsKey((int)pinky.transform.position.x + "," + (int)pinky.transform.position.y)
            ? PathFinder.findWeight(new_graph[(int)transform.position.x + "," + (int)transform.position.y], new_graph[(int)pinky.transform.position.x + "," + (int)pinky.transform.position.y])
            : 9999;

        clydeDistance = new_graph.ContainsKey((int)clyde.transform.position.x + "," + (int)clyde.transform.position.y)
            ? PathFinder.findWeight(new_graph[(int)transform.position.x + "," + (int)transform.position.y], new_graph[(int)clyde.transform.position.x + "," + (int)clyde.transform.position.y])
            : 9999;


        //inkyDistance = (euclideanDistance(gameObject.transform.position.x, gameObject.transform.position.y, inky.position.x, inky.position.y));
        //blinkyDistance =   (euclideanDistance(gameObject.transform.position.x, gameObject.transform.position.y, blinky.position.x, blinky.position.y));
        //pinkyDistance =  (euclideanDistance(gameObject.transform.position.x, gameObject.transform.position.y, pinky.position.x, pinky.position.y));
        //clydeDistance = (euclideanDistance(gameObject.transform.position.x, gameObject.transform.position.y, clyde.position.x, clyde.position.y));

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
            //UnityEngine.Debug.Log("Sup1");
            UnityEngine.Debug.Log("Treshold for High Score: " + SM.LowestHigh());
            //int scene = SceneManager.GetActiveScene().buildIndex;
            //SceneManager.LoadScene(scene, LoadSceneMode.Single);
            if (GameManager.score >= SM.LowestHigh())
                GUINav.getScoresMenu();
            else
                GUINav.H_ShowGameOverScreen();


            //Application.LoadLevel("game");
            //GM.ResetScene();

            //UnityEngine.Debug.Log("Sup2");
            //FindObjectOfType<Academy>().Done();

            //Start();
            //GameGUINavigation gui = GameObject.FindObjectOfType<GameGUINavigation>();
            //gui.H_ShowReadyScreen();
            //GameManager.gameState = GameManager.GameState.Game;
            //UnityEngine.Debug.Log("Sup3");
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
        //if (Input.GetAxis("Horizontal") > 0) _nextDir = Vector2.right;
        //if (Input.GetAxis("Horizontal") < 0) _nextDir = -Vector2.right;
        //if (Input.GetAxis("Vertical") > 0) _nextDir = Vector2.up;
        //if (Input.GetAxis("Vertical") < 0) _nextDir = -Vector2.up;

        // if pacman is in the center of a tile
        if (Vector2.Distance(_dest, transform.position) < 0.00001f)
        {
            if (Valid(_nextDir))
            {
                _dest = (Vector2)transform.position + _nextDir;
                _dir = _nextDir;
            }
            else   // if next direction is not valid
            {
                if (Valid(_dir))  // and the prev. direction is valid
                    _dest = (Vector2)transform.position + _dir;   // continue on that direction

                // otherwise, do nothing
            }
        }
    }

    //void ReadInputAndMove()
    //{
    //    // move closer to destination
    //    Vector2 p = Vector2.MoveTowards(transform.position, _dest, speed);
    //    GetComponent<Rigidbody2D>().MovePosition(p);

    //    // _nextDir = Some direction based on Algorithm

    //    //System.Diagnostics.Debug.WriteLine("Inky " + inky.GetComponent<AI>().ghost._direction);
    //    //System.Diagnostics.Debug.WriteLine("Inky " + inky.position);

    //    // Food finding
    //    findFoodToConsume();

    //    // Ghost fleeing
    //    _nextDir = Vector2.right;
    //    fleeFromClosestGhost();

    //    // 4 components

    //    // Prioritizing between food finding and ghost fleeing


    //    validateAndChangeDir();
    //}

    private void findFoodToConsume()
    {
        GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
        //System.Diagnostics.Debug.WriteLine(currentPacdots[0].transform.position);
    }

    private void validateAndChangeDir()
    {
        // if pacman is in the center of a tile
        if (Vector2.Distance(_dest, transform.position) < 0.00001f)
        {
            if (Valid(_nextDir))
            {
                _dest = (Vector2)transform.position + _nextDir;
                _dir = _nextDir;
            }
            else   // if next direction is not valid
            {
                if (Valid(_dir))  // and the prev. direction is valid
                    _dest = (Vector2)transform.position + _dir;   // continue on that direction

                // otherwise, do nothing
            }
        }
    }

    private void fleeFromClosestGhost()
    {
        Transform ghost = closestGhost();
        if (ghost == null)
        {
            return;
        }
        _nextDir.x = ghost.GetComponent<AI>().ghost._direction.x;
        _nextDir.y = ghost.GetComponent<AI>().ghost._direction.y;
    }

    private Transform closestGhost()
    {
        Transform temp = null;
        float distance = 100000000;
        if (Vector2.Distance(inky.position, transform.position) < distance)
        {
            distance = Vector2.Distance(inky.position, transform.position);
            temp = inky;
        }
        if (Vector2.Distance(blinky.position, transform.position) < distance)
        {
            distance = Vector2.Distance(blinky.position, transform.position);
            temp = blinky;
        }
        if (Vector2.Distance(pinky.position, transform.position) < distance)
        {
            distance = Vector2.Distance(pinky.position, transform.position);
            temp = pinky;
        }
        if (Vector2.Distance(clyde.position, transform.position) < distance)
        {
            distance = Vector2.Distance(clyde.position, transform.position);
            temp = clyde;
        }
        if (distance < 10)
        {
            return temp;
        }
        else
        {
            return null;
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

    public override void CollectObservations()
    {
        //AddVectorObs(float);
        //return null
        AddVectorObs(gameObject.transform.position.x);
        AddVectorObs(gameObject.transform.position.y);
        AddVectorObs(_dir.x);
        AddVectorObs(_dir.y);
        AddVectorObs(inkyDistance);
        AddVectorObs(blinkyDistance);
        AddVectorObs(pinkyDistance);
        AddVectorObs(clydeDistance);
    }

    public float euclideanDistance(float x1, float y1, float x2, float y2)
    {
        return (float)Math.Sqrt((x1-x2) * (x1-x2) + (y1-y2) * (y1-y2));
    }

    public override void AgentReset()
    {
        //UnityEngine.Debug.Log("Sup4");
        //GM.ResetScene();
        //Start();
        //GameManager.gameState = GameManager.GameState.Game;
        //GameGUINavigation gui = GameObject.FindObjectOfType<GameGUINavigation>();
        //gui.H_ShowReadyScreen();

        System.Diagnostics.Debug.WriteLine("Agent Reset");
    }

    public override void AgentOnDone()
    {
        System.Diagnostics.Debug.WriteLine("Agent Done");
        base.AgentOnDone();
        
    }

    public override void AgentAction(float[] action, String textAction)
    {
        int currentScore = GameManager.score;
        Transform temp = transform;
        if (action[0] == 2) _nextDir = Vector2.right;
        if (action[0] == 1) _nextDir = -Vector2.right;
        if (action[1] == 1) _nextDir = Vector2.up;
        if (action[1] == 2) _nextDir = -Vector2.up;

        //AddReward(float);
        if (GameManager.gameState == GameManager.GameState.Dead)
        {
            AddReward(-1.0f);
        }
        else
        {
            float currentReward = 0.07f;
            int difference = currentScore - score;
            float posDifference = euclideanDistance(transform.position.x, transform.position.y, temp.position.x, temp.position.y);
            switch (difference)
            {
                case 10:
                    currentReward += 0.3f;
                    break;
                case 200:
                    currentReward += 0.6f;
                    break;
                case 400:
                    currentReward += 0.7f;
                    break;
                case 800:
                    currentReward += 0.8f;
                    break;
                case 1600:
                    currentReward += 0.9f;
                    break;
            }

            float[] ghostDistances = { inkyDistance, blinkyDistance, pinkyDistance,
        clydeDistance};

            foreach (float distance in ghostDistances)
            {
                currentReward += (distance - 7) / 20;
            }

            if (posDifference < 1) {
                AddReward(-0.05f);
            }

            AddReward(currentReward);
        }

        score = currentScore;
    }

}
