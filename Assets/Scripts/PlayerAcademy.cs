﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PlayerAcademy : Academy
{
    //private int test = 0;
    GameManager GM;
    GameObject[] pacdots;
    GameObject[] pacdotsRef;
    public GameObject mazeobject;
    private bool saved = false;
    public GameObject destroyThis;
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
        System.Diagnostics.Debug.WriteLine("Acamdemy Initialized()");
    }

    public override void AcademyStep()
    {
        //System.Diagnostics.Debug.WriteLine("AcademyStep");
        if (GameManager.lives <= 0)
        {
            //System.Diagnostics.Debug.WriteLine("AcademyStep Done");
            Done();
            destroyThis = GameObject.Find("mazeobject");
            if(!destroyThis) destroyThis = GameObject.Find("mazeobject(Clone)");
            Destroy(destroyThis);
            Instantiate(mazeobject);
            GM = GameObject.Find("Game Manager").GetComponent<GameManager>();
            GameManager.lives = 3;
            GameManager.Level = 0;
            GameManager.score = 0;
            GM.OnLevelWasLoaded();
            GM.ResetScene();
            //Application.LoadLevel("game");
        }

    }
}
