using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public static readonly bool OpenLoadMenuOnGameStart = true;


	public SaveLoadMenu saveLoadMenu;
	public NewMapMenu newMapMenu;

	public InfoPanel keywordInfoPanel;

	void Awake(){
		GameController.uiManager = this;
	}

	public void OpenStartingMenus(){
	
		if(OpenLoadMenuOnGameStart){
			saveLoadMenu.Open(false);
		}
		else{
			saveLoadMenu.Close();
		}
	}

	public void CloseAllMenus(){
		saveLoadMenu.Close();
		newMapMenu.Close();
	}

	public void HideCombatUI(){
		TurnManager.HideUI();
		CombatManager.instance.SetCombatPreviewVisible(false);
	}
}
