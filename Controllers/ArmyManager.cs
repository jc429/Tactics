using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArmyManager{
	
	public const int numArmies = 8;

	static List<HexUnit>[] armyLists = new List<HexUnit>[numArmies];

	/* which army is currently undergoing their turn */
	public static int currentArmy = 0;

	public static List<HexUnit> GetCurrentArmy(){
		return armyLists[currentArmy];
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

	public static void AssignUnitToArmy(HexUnit unit, int army){
		if(army <= 0 || army > numArmies){
			Debug.Log("Invalid Army!");
			return;
		}

		if(!armyLists[army].Contains(unit)){
			armyLists[army].Add(unit);
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
