using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    void Awake(){
		GameController.pauseScreen = this;
	}


	public void ActivatePauseScreen(){
		gameObject.SetActive(true);
	}

	public void DeactivatePauseScreen(){
		gameObject.SetActive(false);
	}

}
