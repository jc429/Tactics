using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ArmyManager{
	
	public const int numArmies = 8;

	public static ArmyColorProfile[] colorProfiles = new ArmyColorProfile[numArmies];
	public static ACPInterface acpInterface;

	static List<MapUnit>[] armyLists = new List<MapUnit>[numArmies];

	/* which army is currently undergoing their turn */
	public static int currentArmy = 0;

	public static void Initialize(){
		ClearAllArmies();
		LoadArmyColorProfiles();
		currentArmy = 0;
	}

	public static void StartGame(){
		currentArmy = GetFirstPopulatedArmyNumber();
	}

	public static int GetCurrentArmyNumber(){
		return currentArmy;
	}

	public static int GetFirstPopulatedArmyNumber(){
		for(int firstArmy = 0; firstArmy < numArmies; firstArmy++){
			if(armyLists[firstArmy].Count > 0){
				return firstArmy;
			}
		}
		Debug.Log("no armies have any units");
		return 0;
	}

	public static int GetNextArmyNumber(){
		int nextArmy = (currentArmy + 1) % numArmies;
		while(armyLists[nextArmy].Count == 0){
			nextArmy = (nextArmy + 1) % numArmies;
			if(nextArmy == currentArmy){
				Debug.Log("no other armies have units??");
				break;
			}
		}
		return nextArmy;
	}

	public static List<MapUnit> GetCurrentArmy(){
		return armyLists[currentArmy];
	}

	public static List<MapUnit> GetArmy(int armyNo){
		if(armyNo < 0 || armyNo >= numArmies){
			Debug.Log("Invalid army request: " + armyNo);
			return null;
		}
		currentArmy = armyNo;
		return armyLists[armyNo];
	}

	public static List<MapUnit> AdvanceToNextArmy(){
		int nextArmy = (currentArmy + 1) % numArmies;
		while(armyLists[nextArmy].Count == 0){
			nextArmy = (nextArmy + 1) % numArmies;
			if(nextArmy == currentArmy){
				Debug.Log("no other armies have units??");
				break;
			}
		}
		currentArmy = nextArmy;
		return armyLists[nextArmy];
	}

	public static void ClearAllArmies(){
		//foreach (List<HexUnit> armyList in armyLists){
		for(int i = 0; i < numArmies; i++){
			if(armyLists[i] == null){
				armyLists[i] = new List<MapUnit>();
			}
			armyLists[i].Clear();
		}
	}
	
	public static void ClearArmy(int army){
		armyLists[army].Clear();
	}

	/* disables all units on the board */
	public static void SetAllUnitsToFinished(){
		for(int i = 0; i < numArmies; i++){
			foreach(MapUnit unit in armyLists[i]){
				unit.EndTurn();
			}
		}
	}

	public static void AssignUnitToArmy(MapUnit unit, int army){
		if(army <= 0 || army >= numArmies){
			Debug.Log("Invalid Army!");
			return;
		}

		if(!armyLists[army].Contains(unit)){
			armyLists[army].Add(unit);
			//Debug.Log("Unit added to army");
		}
	}
	
	public static void RemoveUnitFromArmy(MapUnit unit, int army){
		if(army <= 0 || army >= numArmies){
			Debug.Log("Invalid Army!");
			return;
		}

		if(!armyLists[army].Contains(unit)){
			armyLists[army].Remove(unit);
		}
		else{
			Debug.Log("Unit wasn't in army " + army + "!");
		}
	}

	/* returns true if all units in an army have expended their action */
	public static bool ArmyDone(int army){
		foreach(MapUnit unit in armyLists[army]){
			if(unit.IsFinished()){
				return false;
			}
		}
		return true;
	}

	public static ArmyColorProfile GetArmyColorProfile(int army){
		army = Mathf.Clamp(army, 0, numArmies - 1);
		return colorProfiles[army];
	}


	public static void SaveArmyColorProfiles(){
		if(acpInterface != null){
			colorProfiles = acpInterface.colorProfiles;
		}
		string path = Path.Combine(Application.persistentDataPath,"ArmyColorProfiles.cfg");
		Debug.Log("Saving Color Profiles to: " + path);
		using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create))) {
			writer.Write(1);
			foreach(ArmyColorProfile acp in colorProfiles){
				if(acp != null){
					acp.SaveProfile(writer);	
				}
			}
		}
	}
	
	public static void LoadArmyColorProfiles(){
		string path = Path.Combine(Application.persistentDataPath,"ArmyColorProfiles.cfg");
		Debug.Log("Loading Color Profiles from: " + path);
		if (!File.Exists(path)) {
			Debug.Log("Color Profiles not found!");
			return;
		}
		using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
			int header = reader.ReadInt32();
			for(int i = 0; i < numArmies; i++){
				if(colorProfiles[i] == null){
					colorProfiles[i] = new ArmyColorProfile();
				}
				colorProfiles[i].LoadProfile(reader);	
			}
		}
		if(acpInterface != null){
			Debug.Log("Color Profile added to interface");
			acpInterface.colorProfiles = colorProfiles;
		}
	}
}
