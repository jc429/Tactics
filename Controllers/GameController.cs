using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField]
	public static bool DEBUG_MODE = true;

	public static GameController instance;			//the active instance of the game manager

	public static MapCamera mapCamera;	

	public static HexGrid hexGrid;

	public static GameUI gameUI;
	public static UnitInfoPanel unitInfoPanel;

    void Awake(){
        if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else if(instance != this) {
			Destroy(this.gameObject);
		}
    }



}
