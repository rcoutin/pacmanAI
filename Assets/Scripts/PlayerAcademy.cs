using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using UnityEngine.UI;

public class PlayerAcademy : Academy
{
    //private int test = 0;
    GameManager GM;
    GameObject[] pacdots;
    GameObject[] pacdotsRef;
    public GameObject mazeobject;
    private bool saved = false;
    public GameObject destroyThis;
    public Image[] livesImage;
    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Debug.Log("Sup6");
    }

    // Update is called once per frame
    void Update()
    {

    }
    

    public override void AcademyReset()
    {

        //GM = GameObject.Find("Game Manager").GetComponent<GameManager>();
        //GameManager.lives = 3;
        //GameManager.Level = 0;
        //GameManager.score = 0;
        //GM.OnLevelWasLoaded();
        //GM.ResetScene();

        //if (!saved)
        //{
        //    pacdotsRef = (GameObject[])GameObject.FindGameObjectsWithTag("pacdot");
        //    pacdots = new GameObject[pacdotsRef.Length];
        //    for (int i = 0; i < pacdotsRef.Length; i++)
        //    {
        //        pacdots[i] = pacdotsRef[i];
        //    }
        //    saved = true;
        //}
        //else
        //{
        //    for (int i = 0; i < pacdotsRef.Length; i++)
        //    {
        //        pacdotsRef[i] = pacdots[i];
        //    }
        //    saved = false;
        //}



        //System.Diagnostics.Debug.WriteLine("AcademyReset");
    }

    public override void InitializeAcademy()
    {
        UIScript ui = GameObject.FindObjectOfType<UIScript>();
        livesImage =(Image[]) ui.lives.ToArray().Clone();
        
        Monitor.SetActive(true);
        System.Diagnostics.Debug.WriteLine("Academy Initialized()");
    }

    public override void AcademyStep()
    {
        GameObject[] pacdots = GameObject.FindGameObjectsWithTag("pacdot");
        //System.Diagnostics.Debug.WriteLine("AcademyStep");
        if (GameManager.lives <= 0 || pacdots.Length == 0)
        {
            //System.Diagnostics.Debug.WriteLine("AcademyStep Done");
            Done();
            destroyThis = GameObject.Find("mazeobject");
            if(!destroyThis) destroyThis = GameObject.Find("mazeobject(Clone)");
            Destroy(destroyThis);
            Instantiate(mazeobject);
            GM = GameObject.Find("Game Manager").GetComponent<GameManager>();
            GameManager.lives = 3;
            GameManager.Level = pacdots.Length == 0 ? GameManager.Level + 1 : 0;
            GameManager.score = 0;
            GM.OnLevelWasLoaded();
            GM.ResetScene();
            UIScript ui = GameObject.FindObjectOfType<UIScript>();
            for(int i = 0;i < 3; i++)
            {
                ui.lives.Add(livesImage[i]);
            }
            
           
            GameObject.FindObjectOfType<PlayerController>().graph.initGraph();
            GameObject.FindObjectOfType<PlayerController>().prevScore = 0;

            //Application.LoadLevel("game");
        }

    }
}
