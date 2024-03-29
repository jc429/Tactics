﻿using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadMenu : MonoBehaviour {

	const int SaveFileVersionNumber = 1;

	//public HexGrid hexGrid;
	public MapGrid mapGrid;

	public TextMeshProUGUI menuLabel;
	public Text actionButtonLabel;

	public InputField nameInput;	//where file name is input

	public RectTransform listContent;	//list of maps
	
	public SaveLoadItem itemPrefab;		

	bool saveMode;


	public void Open (bool mode) {
		saveMode = mode;
		if (saveMode) {
			menuLabel.text = "Save Map";
			actionButtonLabel.text = "Save";
		}
		else {
			menuLabel.text = "Load Map";
			actionButtonLabel.text = "Load";
		}
		FillList();
		if(!saveMode && listContent.childCount > 0){
			SaveLoadItem sli = listContent.GetChild(listContent.childCount - 1).gameObject.GetComponent<SaveLoadItem>();
			if(sli != null){
				SelectItem(sli.MapName);
			}
		}
		gameObject.SetActive(true);
		//GameController.hexCamera.LockCamera();

	}

	public void Close () {
		gameObject.SetActive(false);
		//GameController.hexCamera.UnlockCamera();
	}

	string GetSelectedPath () {
		string mapName = nameInput.text;
		if (mapName.Length == 0) {
			return null;
		}
		return Path.Combine(Application.persistentDataPath, mapName + ".map");
	}


	
	public void SaveMap(string path) {
		Debug.Log("Saving to: " + path);
		using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create))) {
			writer.Write(SaveFileVersionNumber);
			mapGrid.SaveGrid(writer);
		}
	}

	public void LoadMap(string path) {
		if (!File.Exists(path)) {
			Debug.LogError("File does not exist: " + path);
			return;
		}
		GameController.instance.InitializeGame();
		using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
			int header = reader.ReadInt32();
			if(header <= 1){
				mapGrid.LoadGrid(reader, header);
				//GameController.hexCamera.ValidatePosition();
			}
			else {
				Debug.LogWarning("Unknown map format: " + header);
			}
		}
	}

	public void DeleteMap() {
		string path = GetSelectedPath();
		if (path == null) {
			return;
		}
		if (File.Exists(path)) {
			File.Delete(path);
		}
		nameInput.text = "";
		FillList();
	}

	/* Populates the save/load list with existing maps */
	void FillList () {
		for (int i = 0; i < listContent.childCount; i++) {
			Destroy(listContent.GetChild(i).gameObject);
		}
		string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.map");
		Array.Sort(paths);
		for (int i = 0; i < paths.Length; i++) {
			SaveLoadItem item = Instantiate(itemPrefab);
			item.menu = this;
			item.MapName = Path.GetFileNameWithoutExtension(paths[i]);
			item.transform.SetParent(listContent, false);
		}
	}

	/* select an item from the list */
	public void SelectItem (string name) {
		nameInput.text = name;
	}

	/* save or load depending on menu type */
	public void Action () {
		string path = GetSelectedPath();
		if (path == null) {
			Debug.Log("No Path");
			return;
		}
		if (saveMode) {
			SaveMap(path);
		}
		else {
			Debug.Log("Loading Map");
			LoadMap(path);
		}
		Close();
	}
}