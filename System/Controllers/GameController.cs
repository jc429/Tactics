﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameMode{
	TestingGrounds,
	MapEditor,
	PlayGame
}

public class GameController : MonoBehaviour
{
	static GameMode gMode = GameMode.TestingGrounds;
	public static GameMode gameMode{
		get { return gMode; }
	}

	//the active instance of the game manager
	public static GameController instance;			
	public static UIManager uiManager;

	/* the current map */ 
	public static MapGrid mapGrid;
	public static MapCamera mapCamera;	

	public static MapEditor mapEditor;

	public static GameUI gameUI;
	public static UnitInfoPanel unitInfoPanel;

	public static PauseScreen pauseScreen;
	public static bool gamePaused;

	public RectTransform hpBarCanvasParent;


	public TextAsset keywordCSV;

	public Texture2D skillSpriteSheet;


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
		switch(gameMode){
			case GameMode.TestingGrounds:
			{
				mapGrid.CreateMapRect(8,8,true);
				uiManager.CloseAllMenus();
				mapEditor.SetMapEditorActive(false);
				uiManager.HideCombatUI();
				
				//StartGame();

			}
			break;
			case GameMode.MapEditor:
			{
				//randomly generate a map 
				mapGrid.CreateMapRect(8,8);
				uiManager.OpenStartingMenus();
				mapEditor.SetMapEditorActive(true);
				uiManager.HideCombatUI();

			}
			break;
			case GameMode.PlayGame:
			{
			//	mapGrid.StartMap();
			//	TurnManager.StartTurn();
				SetGamePaused(false);
				uiManager.keywordInfoPanel.Close();
				StartGame();
			}
			break;
		}
	}

	void Update(){
		InputController.CaptureInputs();
		if(!gameStarted && Input.GetKeyDown(KeyCode.Space)){
			StartGame();
		}

		if(GameSettings.DEBUG_MODE){
			DebugInput();
		}

	}

	void DebugInput(){

		if(Input.GetKeyDown(KeyCode.B)){
			//Debug.Log(Input.mousePosition);
			ConditionIDExtensions.PrintAllEnumIDs();
			EffectIDExtensions.PrintAllEnumIDs();
			SkillTriggerIDExtensions.PrintAllEnumIDs();
			SkillTargetExtensions.PrintAllEnumIDs();
		}

		
		if(Input.GetKeyDown(KeyCode.Alpha0)){
			ArmyManager.SaveArmyColorProfiles();
		}
		if(Input.GetKeyDown(KeyCode.Alpha9)){
			ArmyManager.LoadArmyColorProfiles();
		}
	}


	public void InitializeGame(){
		Debug.Log("Initializing Game...");
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
		gameUI.StartGame();
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


	public void RestartGame(){
		
	}

}
