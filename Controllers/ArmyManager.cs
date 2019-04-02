using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArmyManager{
	
	public const int numArmies = 8;

	static List<HexUnit>[] armyLists = new List<HexUnit>[numArmies];

	/* which army is currently undergoing their turn */
	public static int currentArmy = 0;

	public static void Initialize(){
		ClearAllArmies();
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

	public static List<HexUnit> GetCurrentArmy(){
		return armyLists[currentArmy];
	}

	public static List<HexUnit> GetArmy(int armyNo){
		if(armyNo < 0 || armyNo >= numArmies){
			Debug.Log("Invalid army request: " + armyNo);
			return null;
		}
		currentArmy = armyNo;
		return armyLists[armyNo];
	}

	public static List<HexUnit> AdvanceToNextArmy(){
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
				armyLists[i] = new List<HexUnit>();
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
			foreach(HexUnit unit in armyLists[i]){
				unit.EndTurn();
			}
		}
	}

	public static void AssignUnitToArmy(HexUnit unit, int army){
		if(army <= 0 || army > numArmies){
			Debug.Log("Invalid Army!");
			return;
		}

		if(!armyLists[army].Contains(unit)){
			armyLists[army].Add(unit);
			//Debug.Log("Unit added to army");
		}
	}
	
	public static void RemoveUnitFromArmy(HexUnit unit, int army){
		if(army <= 0 || army > numArmies){
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
		foreach(HexUnit unit in armyLists[army]){
			if(unit.IsFinished()){
				return false;
			}
		}
		return true;
	}
}
