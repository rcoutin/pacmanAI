using System;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using MLAgents;
using UnityEngine.SceneManagement;

public class PlayerController : Agent
{

    private int currentSteps = 0;
    private int curDestOptimal = -1;
    public float STAY_ALIVE_RW = 0.1f;
    public float SCORE_POS_RW = 0.25f;
    public float SCORE_NEG_RW = -0.01f;
    private float chaseTimer = 0.0f;
    public float PACDOT_DIR_SCORE = 0.1f;
    public float DANGER_MULT = 0.1f;
    public float POWERUP_DIR_SCORE = 0.1f;
    public float DANGER_WT = -4;
    public float SCARED_WT = 2;
    public float PACDOT_WT = 2;
    public float POWERUP_WT = 4;
    public int PATH_SIZE = 10;
    public float GHOST_WT = -4;
    GameObject clyde_obj;
    GameObject pinky_obj;
    GameObject inky_obj;
    GameObject blinky_obj;

    private readonly String[] dirNames = new String[] { "Left", "Right", "Up", "Down" };
    private readonly Vector2[] dirs = new Vector2[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
    public bool debugEnabled;
    public bool drawExpandedPaths;
    public bool drawPathToNearestPacdot;
    public bool followCalculatedPath;
    public float speed = 0.15f;
    Vector2 _dest = Vector2.zero;
    Vector2 _dir = Vector2.zero;
    Vector2 _nextDir = Vector2.zero;
    public GameObject destroyThis;
    public GameObject mazeobject;
    private List<GraphNode> pathToClosestPacdot;
    public bool trainingEnabled;
    public MazeGraph graph;

    public Boolean alive;
    private Dictionary<int, float[]> currentState;

    public void agent_done()
    {
       
        currentSteps = 0;
        curDestOptimal = -1;
        destroyThis = GameObject.Find("mazeobject");
        if (!destroyThis) destroyThis = GameObject.Find("mazeobject(Clone)");
        Destroy(destroyThis);
        Instantiate(mazeobject);
        GM = GameObject.Find("Game Manager").GetComponent<GameManager>();
        GameManager.lives = 3;
        GameManager.Level = 1 ;
       // GameManager.score = 0;
        GM.OnLevelWasLoaded();
        GM.ResetScene();
        ResetDestination();
        transform.position = new Vector3(13f, 11f, 0f);

        try
        {
            graph.initGraph();

        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.Print(e.Message);
            System.Diagnostics.Debug.Print(e.StackTrace);
        }
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
        clyde_obj = GameObject.Find("clyde");
        pinky_obj = GameObject.Find("pinky");
        inky_obj = GameObject.Find("inky");
        blinky_obj = GameObject.Find("blinky");

        try
        {
            graph = trainingEnabled? new TrainingMazeGraph() : new MazeGraph();
            graph.initGraph();

            
            GameObject[] currentPacdots = GameObject.FindGameObjectsWithTag("pacdot");
           
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.Print(e.Message);
            System.Diagnostics.Debug.Print(e.StackTrace);
        }
    }

    private void showStateMonitors()
    {
        Monitor.Log("food_left", currentState[0][1]);
        Monitor.Log("food_right", currentState[1][1]);
        Monitor.Log("food_up", currentState[2][1]);
        Monitor.Log("food_down", currentState[3][1]);
        Monitor.Log("danger_left", currentState[0][0]);
        Monitor.Log("danger_right", currentState[1][0]);
        Monitor.Log("danger_up", currentState[2][0]);
        Monitor.Log("danger_down", currentState[3][0]);
    }

    private void updateOnNodeChange()
    {
        currentSteps++;
        GraphNode current = graph.GetNode(transform);
        if (current == null) return;
    
        if (graph.ContainsNode(transform))
        {
            GraphNode currentPacman = graph.GetNode(transform);
            currentPacman.isPacDot = false;
            currentPacman.isPowerUp = false;
        }
        findNearestPacdot();
        if (curDestOptimal == -1) curDestOptimal = pathToClosestPacdot.Count;
        updateDirectionStates();
        int neighbourPos = -1;
       // Make pacman prioritize direction to closest pacdot incase there are no pacdots k nodes away
        if (drawPathToNearestPacdot && pathToClosestPacdot != null && pathToClosestPacdot.Count > 0)
        {
            
            int x = current.x;
            int y = current.y;
            GraphNode next = pathToClosestPacdot[pathToClosestPacdot.Count - 1];
            if (next.x > x) neighbourPos = 1;
            else if (next.x < x) neighbourPos = 0;
            else if (next.y > y) neighbourPos = 2;
            else if (next.y < y) neighbourPos = 3;

            if (neighbourPos != -1) currentState[neighbourPos][1] = 0.7f;
            Monitor.Log("DirToNearestPac", dirNames[neighbourPos]);
        }

        // this assigns to float nearestPacdotLength and List<GraphNode> pathToNearestPacDot

        showStateMonitors();
        if (followCalculatedPath) followpath();

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
        List<List<GraphNode>> pathList = PathFinder.expand(current, PATH_SIZE);

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
            //direction is valid, set default value for safety as 1
           
            for (int i=0;i<path.Count-1;i++)
            {
                float danger = 0;
                if (isGhost(path[i]))
                {
                    float cur = i*(i/2);
                    danger = (cur*DANGER_MULT);
                    //float existingDanger = dirState[neighbourPos][0];
                    //if (danger > existingDanger)
                    //{
                    //    dirState[neighbourPos][0] = danger;
                    //}
                    dirState[neighbourPos][0] += GHOST_WT * danger;
                }
                //dirState[neighbourPos][0] = ((numPaths[neighbourPos] - 1) * dirState[neighbourPos][0] + danger) / numPaths[neighbourPos];
                if (path[i].isPacDot)
                {
                    //dirState[neighbourPos][1] =Math.Min(1,PACDOT_DIR_SCORE+ dirState[neighbourPos][1]);
                    dirState[neighbourPos][1] +=  (i * PACDOT_DIR_SCORE) / path.Count;
                }
                if (path[i].isPowerUp)
                {
                    dirState[neighbourPos][2] += i* POWERUP_DIR_SCORE;
                }
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
        
            if (drawExpandedPaths) graph.drawPathLines(path, colors[colorIndex] , 0.3f);

        }
        // adding default values for unassigned directions

        for (int i=0;i<4;i++)
        {
            if (numPaths[i] != 0)
            {
                //dirState[i][0] /= numPaths[i];
                dirState[i][1] /= numPaths[i];
                //dirState[i][2] /= numPaths[i];

            }
        }
        currentState = dirState;
       
    }




    private bool isGhost(GraphNode node)
    {
        int x = node.x;
        int y = node.y;
        if ((x == (int)blinky?.transform.position.x) && (y == (int)blinky?.transform.position.y))
        {
            GHOST_WT = blinky_obj?.GetComponent<GhostMove>().state != GhostMove.State.Run? DANGER_WT : SCARED_WT;
            return true;
        }
        if ((x == (int)inky?.transform.position.x) && (y == (int)inky?.transform.position.y))
        {
            GHOST_WT = inky_obj?.GetComponent<GhostMove>().state != GhostMove.State.Run ? DANGER_WT : SCARED_WT;
            return true;
        }
        if ((x == (int)pinky?.transform.position.x) && (y == (int)pinky?.transform.position.y))
        {
            GHOST_WT = pinky_obj?.GetComponent<GhostMove>().state != GhostMove.State.Run ? DANGER_WT : SCARED_WT;
            return true;
        }
        if ((x == (int)clyde?.transform.position.x) && (y == (int)clyde?.transform.position.y))
        {
            GHOST_WT = clyde_obj?.GetComponent<GhostMove>().state != GhostMove.State.Run ? DANGER_WT : SCARED_WT;
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
        if(drawPathToNearestPacdot) graph.drawPathLines(path,Color.magenta,0.3f);
        pathToClosestPacdot = path;
        nearestPacdotDistance = path.Count;
        return;
    }


    public void followpath()
    {

        float max = float.MinValue;
        int direction = -1;
        
        foreach (int key in currentState.Keys)
        {
            
            float value = currentState[key][0] + PACDOT_WT*currentState[key][1] + POWERUP_WT*currentState[key][2];
            if (value > max)
            {
                if (Valid(dirs[key])) {
                    max = value;
                    direction = key;
                }

            }
        }
        _nextDir = dirs[direction];

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
        _dest = new Vector2(13f, 11f);
        GetComponent<Animator>().SetFloat("DirX", 1);
        GetComponent<Animator>().SetFloat("DirY", 0);
    }

    void ReadInputAndMove()
    {
        // move closer to destination
        if (Vector2.Distance(_dest, transform.position) < speed)
        {

            updateOnNodeChange(); 
            RequestDecision();
        }
        validateAndChangeDir();
        Vector2 p = Vector2.MoveTowards(transform.position, _dest, speed);
        GetComponent<Rigidbody2D>().MovePosition(p);

    }


    void PrintLog(System.Object s)
    {
        if(debugEnabled) System.Diagnostics.Debug.WriteLine(s);

    }

    private void validateAndChangeDir()
    {
        // if pacman is in the center of a tile
        if (Vector2.Distance(_dest, transform.position) == 0f)
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

    protected void setActionMask()
    {
        List<int> mask = new List<int>();
        mask.Add(0);
        if (!Valid(Vector2.left)) mask.Add(1);
        if (!Valid(Vector2.right)) mask.Add(2);
        if (!Valid(Vector2.up)) mask.Add(3);
        if (!Valid(Vector2.down)) mask.Add(4);

        if(mask.Count > 0) SetActionMask(0, mask.ToArray());

        //update state danger default values based on the mask

        //if (currentState != null)
        //{
        //    if (cantGoLeft) currentState[0][0] = 1;
        //    if (cantGoRight) currentState[1][0] = 1;
        //    if (cantGoUp) currentState[2][0] = 1;
        //    if (cantGoDown) currentState[3][0] = 1;
        //}

    }

    public override void CollectObservations()
    {
        setActionMask();
       
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
            AddVectorObs(new float[] { 0, 0, 0 });
            AddVectorObs(new float[] { 0, 0, 0 });
            AddVectorObs(new float[] { 0, 0, 0 }); 
            AddVectorObs(new float[] { 0, 0, 0 });

        
        }
        AddVectorObs(pathToClosestPacdot == null ? 0 : pathToClosestPacdot.Count);
        AddVectorObs(GameManager.scared);
       

    }

    public float euclideanDistance(float x1, float y1, float x2, float y2)
    {
        return (float)Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }

    public override void AgentReset()
    {

        agent_done();


        print("Agent reset is called");
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
            //Monitor.Log("dir", "right");
            _nextDir = Vector2.right;
            //validateAndChangeDir();
            //print("Going Right");
        }
        else if (action[0] == 1) {
            //Monitor.Log("dir", "left");
            _nextDir = Vector2.left;
            // validateAndChangeDir();
            //print("Going Left");
        }
        else if (action[0] == 3)
        {
            //Monitor.Log("dir", "up");
            _nextDir = Vector2.up;
            //validateAndChangeDir();
            //print("Going Up");
        }
        else if (action[0] == 4) {
            //Monitor.Log("dir", "down");
            _nextDir = Vector2.down;
            //validateAndChangeDir();
            //print("Going Down");
        }
       
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

    private float scoreReward(float curScore, float prevScore)
    {

        float pointsGained = curScore - prevScore;

        if (pointsGained == 10) // taken pacdot
        {

            return SCORE_POS_RW;

        }
        else if (pointsGained % 100 == 0) // ate the ghosts
        {
            float mult = pointsGained / 100;

            return mult * 0.1f;
        }
        else if (pointsGained == 0) return SCORE_NEG_RW;
        return 0.0f;

    }
    private float distanceFromGhostReward()
    {
     
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
       // print(textAction);
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
            ///Monitor.Log("Staying Alive", 0.1f);

            //if (currentSteps > curDestOptimal + 10)
            //{
            //    AddReward(-1.0f);
            //    Done();
            //}
            // AddReward(-0.05f);
            // AddReward(sr);
            // AddReward(sr);
            //reward to stay alive
            //AddReward(STAY_ALIVE_RW);
            // AddReward(distanceFromGhostReward());
            //AddReward(distanceToPackDotReward());
        }
        prevScore = currentScore;
    }

}
