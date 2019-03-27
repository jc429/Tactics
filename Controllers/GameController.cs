using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField]
	public static bool DEBUG_MODE = true;

	//the active instance of the game manager
	public static GameController instance;			

	public static MapCamera mapCamera;	

	public static HexGrid hexGrid;

	public static GameUI gameUI;
	public static UnitInfoPanel unitInfoPanel;

	public static PauseScreen pauseScreen;
	public static bool gamePaused;

	public Canvas hpBarCanvas;

    void Awake(){
        if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else if(instance != this) {
			Destroy(this.gameObject);
		}
    }

	public void Start(){
		SetGamePaused(false);
	}

	public void SetGamePaused(bool pause){
		gamePaused = pause;
		if(gamePaused){
			pauseScreen.ActivatePauseScreen();
		}
		else{
			pauseScreen.DeactivatePauseScreen();
		}
	}

}
