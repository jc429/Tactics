﻿using UnityEngine;
using UnityEngine.UI;

public class SaveLoadItem : MonoBehaviour {

	public SaveLoadMenu menu;
	
	string mapName;
	
	public string MapName {
		get {
			return mapName;
		}
		set {
			mapName = value;
			transform.GetChild(0).GetComponent<Text>().text = value;
		}
	}
	
	public void Select () {
		menu.SelectItem(mapName);
	}
}