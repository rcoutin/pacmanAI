using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using MLAgents;
using UnityEngine.SceneManagement;

public class PlayerController : Agent
{

    public float STAY_ALIVE_RW = 0.1f;
    public float SCORE_POS_RW = 0.25f;
    public float SCORE_NEG_RW = -0.001f;

    public float PACDOT_DIR_SCORE = 0.1f;
    public float DANGER_MULT = 0.1f;
    public float POWERUP_DIR_SCORE = 0.0f;

    private String[] dirNames = new String[] { "Left", "Right", "Up", "Down" };
    public bool debugEnabled;
    public bool drawExpandedPaths;
    public bool drawPathToNearestPacdot;
    public float speed = 0.2f;
    Vector2 _dest = Vector2.zero;
    Vector2 _dir = Vector2.zero;
    Vector2 _nextDir = Vector2.zero;
    public GameObject destroyThis;
    public GameObject mazeobject;
    private List<GraphNode> pathToClosestPacdot;
    public MazeGraph graph;

    public Boolean alive;
    private Dictionary<int, float[]> currentState;
    public void agent_done()
    {
        PrintLog("Level complete!");
        Done();
        AddReward(1.0f);
        destroyThis = GameObject.Find("mazeobject");
        if (!destroyThis) destroyThis = GameObject.Find("mazeobject(Clone)");
        Destroy(destroyThis);
        Instantiate(mazeobject);
        GM = GameObject.Find("Game Manager").GetComponent<GameManager>();
        GameManager.lives = 3;
        GameManager.Level++ ;
       // GameManager.score = 0;
        GM.OnLevelWasLoaded();
        GM.ResetScene();
    }

    public int prevScore;

    public Transform inky;
    public Transform blinky;
    public Transform pinky;
    public Transform clyde;

    public int pacdotCount = 0;

    private float inkyDistance = 9999;
    private float blinkyDistance = 9999;
    private float pinkyDistance = 9999;
    private float clydeDistance = 9999;
    private float nearestPacdotDistance = 9999;


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
            graph = new MazeGraph();
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.Print(e.Message);
            System.Diagnostics.Debug.Print(e.StackTrace);
        }
    }


    private void updateOnNodeChange()
    {
        
       // updateGhostDistances();
        // update positions of power up and pacdot
        if (graph.ContainsNode(transform))
        {
            GraphNode currentPacman = graph.GetNode(transform);
            currentPacman.isPacDot = false;
            currentPacman.isPowerUp = false;
        }
        updateDirectionStates();
        //int neighbourPos = -1;
        ////Make pacman prioritize direction to closest pacdot incase there are no pacdots k nodes away
        //if ( pathToClosestPacdot != null && pathToClosestPacdot.Count > 1)
        //{
        //    GraphNode current = graph.GetNode(transform);
        //    int x = current.x;
        //    int y = current.y;
        //    GraphNode next = pathToClosestPacdot[pathToClosestPacdot.Count - 2];
        //    if (next.x > x) neighbourPos = 1;
        //    else if (next.x < x) neighbourPos = 0;
        //    else if (next.y > y) neighbourPos = 2;
        //    else if (next.y < y) neighbourPos = 3;
            
        //    if(neighbourPos !=-1) currentState[neighbourPos][1] = 1.0f;
        //    Monitor.Log("DirToNearestPac", dirNames[neighbourPos]);
        //}

        findNearestPacdot(); // this assigns to float nearestPacdotLength and List<GraphNode> pathToNearestPacDot

        RequestDecision(); // On Demand Decisions = True

        followpath();
    }

    private void updateGhostDistances()
    {
        blinkyDistance = graph.distFrom(transform, blinky);
        inkyDistance = graph.distFrom(transform, inky);
        pinkyDistance = graph.distFrom(transform, pinky);
        clydeDistance = graph.distFrom(transform, clyde);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       

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

    //private float findNearestPacdotLength()
    //{
    //    float minDist = 9999;
    //    GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
    //    foreach (GameObject pacdot in currentPacdots)
    //    {
    //        if (pacdot == null) continue;

    //        int x = (int)pacdot.transform.position.x;
    //        int y = (int)pacdot.transform.position.y;

    //        if (new_graph.ContainsKey(x + "," + y) && new_graph.ContainsKey((int)transform.position.x + "," + (int)transform.position.y))
    //        {
    //            minDist = Math.Min(minDist, PathFinder.findPath(new_graph[x + "," + y], new_graph[(int)transform.position.x + "," + (int)transform.position.y]).Count);
    //        }
    //    }
    //    return minDist;
    //}

    

    private void updateDirectionStates()
    {
        //unsafe,safe,food
        String[] colorNames = new String[] { "red", "green", "yellow" };
        Color[] colors = new Color[] { Color.red, Color.green, Color.yellow};
        int colorIndex = 0;
        Dictionary<int,float[]> dirState = new Dictionary<int, float[]>();
        
        //initialize
        for (int i = 0; i < 4; i++)
        {
            dirState.Add(i,new float[] { 0, 0, 0 });
        }
        GraphNode current = graph.GetNode(transform);
      
        if (current == null) return;
        int x = current.x;
        int y = current.y;
        List<List<GraphNode>> pathList = PathFinder.expand(current, 10);

        int[] numPaths = new int[4];
        foreach (List<GraphNode> path in pathList)
        {
           
            GraphNode next = path[path.Count - 1];
            int neighbourPos = -1;
            //PrintLog(string.Join(", ",new int[] { x, y, next.x, next.y }));
            if (next.x > x) neighbourPos = 1;
            else if (next.x < x) neighbourPos = 0;
            else if (next.y > y) neighbourPos = 2;
            else if (next.y < y) neighbourPos = 3;
            numPaths[neighbourPos]++;
            //if (neighbourPos == -1) continue;
            //direction is valid, set default value for safety as 1
           
            for (int i=0;i<path.Count-1;i++)
            {
                float danger = 0;
                if (isGhost(path[i]))
                {
                    float cur = i;
                    danger = (cur*DANGER_MULT);
                    //float existingDanger = dirState[neighbourPos][0];
                    //if (danger > existingDanger)
                    //{
                    //    dirState[neighbourPos][0] = danger;
                    //}
                    dirState[neighbourPos][0] += danger;
                }
                //dirState[neighbourPos][0] = ((numPaths[neighbourPos] - 1) * dirState[neighbourPos][0] + danger) / numPaths[neighbourPos];
                if (path[i].isPacDot)
                {
                    //dirState[neighbourPos][1] =Math.Min(1,PACDOT_DIR_SCORE+ dirState[neighbourPos][1]);
                    dirState[neighbourPos][1] += PACDOT_DIR_SCORE;
                }
                if (path[i].isPowerUp)
                {
                    dirState[neighbourPos][2] += POWERUP_DIR_SCORE;
                }
            }


            bool currentDirHasPacdots = dirState[neighbourPos][1] > 0;
            if (!currentDirHasPacdots && pathToClosestPacdot != null && pathToClosestPacdot.Count > 1)
            {
                GraphNode nextNodeInPacdotPath = pathToClosestPacdot[pathToClosestPacdot.Count - 1];
                dirState[neighbourPos][1] += 0.2f;
                Monitor.Log("DirToNearestPac", dirNames[neighbourPos]);
            }
            if (dirState[neighbourPos][0] > 0)
            {
                colorIndex = 0; // high danger
            }
            else
            {
                if(dirState[neighbourPos][1] > 2)
                {
                    colorIndex = 2;
                }
                else
                {
                colorIndex = 1;

                }
            }
            //if(dirState[dir][1] > 2.4 && dirState[dir][0] > 0) // some pacdots and slightly safe
            //{
            //    colorIndex = 2;
            //}
            if(drawExpandedPaths) MazeGraph.drawPathLines(path, colors[colorIndex] , 0.3f);
            //PrintLog("PathColor " + colorNames[colorIndex] + string.Join("-", dirState[dir]));

        }
        // adding default values for unassigned directions

        for (int i=0;i<4;i++)
        {
            if (numPaths[i] != 0)
            {
                dirState[i][1] /= numPaths[i];
                dirState[i][0] /= numPaths[i];

            }
        }
        currentState = dirState;
    }


    private bool isGhost(GraphNode node)
    {
        int x = node.x;
        int y = node.y;

        if ((x == (int)blinky.transform.position.x) && (y == (int)blinky.transform.position.y))
        {
            return true;
        }
        if ((x == (int)inky.transform.position.x) && (y == (int)inky.transform.position.y))
        {
            return true;
        }
        if ((x == (int)pinky.transform.position.x) && (y == (int)pinky.transform.position.y))
        {
            return true;
        }
        if ((x == (int)clyde.transform.position.x) && (y == (int)clyde.transform.position.y))
        {
            return true;
        }

        return false;

    }

    

    private void findNearestPacdot()
    {
        GraphNode current = graph.GetNode(transform);
        if (current == null) return;
        current.isPacDot = false;
        List<GraphNode> path = PathFinder.findPathToClosestPacdot(current);
        if (path == null) return;
        //PrintLog(path.Count);

        if(drawPathToNearestPacdot) MazeGraph.drawPathLines(path,Color.magenta,0.3f);
        pathToClosestPacdot = path;
        nearestPacdotDistance = path.Count;
        return;
    }


    public void followpath()
    {
        //int x = (int)transform.position.x;
        //int y = (int)transform.position.y;
        ////needs to be reimplemented.
        //GraphNode next = null;

        ////if (x == next.x && y == next.y)
        ////{
        ////    next.isPacDot = false;
        ////}

        //if (next.x > x)
        //{
        //    _nextDir = Vector2.right;
        //}
        //else if (next.x < x)
        //{
        //    _nextDir = Vector2.left;
        //}
        //else if (next.y > y)
        //{
        //    _nextDir = Vector2.up;
        //}
        //else if (next.y < y)
        //{
        //    _nextDir = Vector2.down;
        //}
        //else
        //{
        //    _nextDir = Vector2.right;
        //}

        float max = float.MinValue;
        int direction = 0;
        if (pathToClosestPacdot != null && pathToClosestPacdot.Count > 0) {
            GraphNode next = pathToClosestPacdot[pathToClosestPacdot.Count - 1];
            if (next.x > transform.position.x) direction = 1;
            else if (next.x < transform.position.x) direction = 0;
            else if (next.y > transform.position.y) direction = 2;
            else if (next.y < transform.position.y) direction = 3;
        }
        foreach (int key in currentState.Keys)
        {
            float value = -5*currentState[key][0] + 2*currentState[key][1] + 2*currentState[key][2];
            if (value > max)
            {
                max = value;
                direction = key;

            }
        }

        switch (direction)
        {
            case 1:
                _nextDir = Vector2.right;
                break;

            case 0:
                _nextDir = Vector2.left;
                break;

            case 2:
                _nextDir = Vector2.up;
                break;

            case 3:
                _nextDir = Vector2.down;
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

    bool IsColliderNearby(Vector2 direction)
    {
        // cast line from 'next to pacman' to pacman
        // not from directly the center of next tile but just a little further from center of next tile
        Vector2 pos = transform.position;
        direction += new Vector2(direction.x * 0.45f, direction.y * 0.45f);
        RaycastHit2D hit = Physics2D.Linecast(pos + direction, pos);
        //UnityEngine.Debug.DrawRay(transform.position, direction * 5, color, 1000000, false);
        return hit.collider.name == "maze";
    }

    void PrintLog(System.Object s)
    {
        if(debugEnabled) System.Diagnostics.Debug.WriteLine(s);

    }

    private void validateAndChangeDir()
    {
        // if pacman is in the center of a tile
        if (Vector2.Distance(_dest, transform.position) == 0.0f)
        {
            updateOnNodeChange();
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

        //update state danger default values based on the mask

        if (currentState != null)
        {
            if (cantGoLeft) currentState[0][0] = 1;
            if (cantGoRight) currentState[1][0] = 1;
            if (cantGoUp) currentState[2][0] = 1;
            if (cantGoDown) currentState[3][0] = 1;
        }

    }

    public override void CollectObservations()
    {
        setActionMask();
        //Monitor.Log("danger_left", currentState[0][0]);
        //Monitor.Log("danger_right", currentState[1][0]);
        //Monitor.Log("danger_up", currentState[2][0]);
        //Monitor.Log("danger_down", currentState[3][0]);
        Monitor.Log("food_left", currentState[0][1]);
        Monitor.Log("food_right", currentState[1][1]);
        Monitor.Log("food_up", currentState[2][1]);
        Monitor.Log("food_down", currentState[3][1]);

        if (currentState != null)
        {
            AddVectorObs(currentState[0]);
            AddVectorObs(currentState[1]);
            AddVectorObs(currentState[2]);
            AddVectorObs(currentState[3]);
           
        }
        else
        {
            //default values
            AddVectorObs(new float[] { 0,0,0});
            AddVectorObs(new float[] { 0, 0, 0 });
            AddVectorObs(new float[] { 0, 0, 0 });
            AddVectorObs(new float[] { 0, 0, 0 });
        }
        //AddVectorObs(pathToClosestPacdot == null ? 0 : pathToClosestPacdot.Count);
        //AddVectorObs(_dir);
        //AddVectorObs(blinkyDistance);

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

        if (action[0] == 2) {
            Monitor.Log("dir", "right");
            _nextDir = Vector2.right;}
        if (action[0] == 1) {
            Monitor.Log("dir", "left");
            _nextDir = -Vector2.right;}
        if (action[1] == 1)
        {
            Monitor.Log("dir", "up");
            _nextDir = Vector2.up;}
        if (action[1] == 2) {
            Monitor.Log("dir", "down");
            _nextDir = -Vector2.up; }
    }

    public float distanceToPackDotReward()
    {
        float distanceToPacdot = pathToClosestPacdot==null?0:pathToClosestPacdot.Count;
        if (distanceToPacdot >= 5)
        {
            return -0.08f;
        }
        //if(distanceToPacdot > 10)
        //{
        //    return -0.05f;
        //}
        if (distanceToPacdot < 5)
        {
            return 0.01f;
        }
        return 0.0f;

    }

    private float scoreReward(float curScore,float prevScore)
    {
        if (curScore - prevScore > 0) return SCORE_POS_RW;
        return SCORE_NEG_RW;
    }
    private float distanceFromGhostReward()
    {
        //float[] ghostDistances = { inkyDistance, blinkyDistance, pinkyDistance,
        //clydeDistance};
        float[] ghostDistances = { blinkyDistance };
        float total = 0.0f;
        foreach (float distance in ghostDistances)
        {
            //if (distance < 1000) d
            //{
            //    total += (distance - 7) / 20;
            //}
            if(distance < 15)
            {
                total += -0.05f;
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
            float sr = scoreReward(currentScore, prevScore);
            Monitor.Log("Score_Reward", sr);
            Monitor.Log("Staying Alive", 0.1f);
            AddReward(sr);
            //reward to stay alive
            AddReward(STAY_ALIVE_RW);
           // AddReward(distanceFromGhostReward());
            //AddReward(distanceToPackDotReward());
        }
        prevScore = currentScore;
    }

}
