﻿using System.Collections;
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

	public RectTransform hpBarCanvasParent;

	public InfoPanel keywordInfoPanel;

	public TextAsset keywordCSV;

	public Texture2D skillSpriteSheet;

	public static class UIElements{
		public static TurnDisplay turnDisplay;
	}

	static bool gameStarted = false;

    void Awake(){
        if (instance == null) {
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else if(instance != this) {
			Destroy(this.gameObject);
		}
		InitializeGame();
    }

	void Start(){
		SetGamePaused(false);
		keywordInfoPanel.Close();
		StartGame();
	}

	void Update(){
		if(!gameStarted && Input.GetKeyDown(KeyCode.Space)){
			StartGame();
		}

		if(Input.GetKeyDown(KeyCode.B)){
			//Debug.Log(Input.mousePosition);
			ConditionIDExtensions.PrintAllEnumIDs();
			EffectIDExtensions.PrintAllEnumIDs();
			SkillTriggerIDExtensions.PrintAllEnumIDs();
		}
	}

	public void InitializeGame(){
		gameStarted = false;
		ArmyManager.Initialize();
		CombatManager.Initialize();
		KeywordTable.InitializeKeywordTable(keywordCSV);
		SkillDBReader.Initialize();
		SkillSpriteLibrary.InitializeSpriteLibrary(skillSpriteSheet);
	}

	public void StartGame(){
		gameStarted = true;
		ArmyManager.SetAllUnitsToFinished();
		TurnManager.StartGame();
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
