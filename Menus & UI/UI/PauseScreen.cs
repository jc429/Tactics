using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{
	public GameObject screen;

    void Awake(){
		GameController.pauseScreen = this;
	}


	public void ActivatePauseScreen(){
		screen.SetActive(true);
	}

	public void DeactivatePauseScreen(){
		screen.SetActive(false);
	}

}
