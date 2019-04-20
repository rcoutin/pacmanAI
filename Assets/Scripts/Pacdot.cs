using UnityEngine;
using System.Collections;

public class Pacdot : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "pacman")
		{
			GameManager.score += 10;
		    GameObject[] pacdots = GameObject.FindGameObjectsWithTag("pacdot");
            Destroy(gameObject);
            
            if (pacdots.Length == 1)
		    {
                GameObject.FindObjectOfType<PlayerController>().AddReward(1.0f);
                GameObject.FindObjectOfType<PlayerController>().Done();
                //GameObject.FindObjectOfType<GameGUINavigation>().LoadLevel();
            }
        }
	}
}
