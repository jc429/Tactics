using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public static class TurnManager {
	
	public static TurnDisplay turnDisplay;

	static int currentTurn;

	static int turnStartArmy;

	static List<MapUnit> currentArmy;

	public static void HideUI(){
		turnDisplay.Reset();
	}

	public static void StartGame(){
		currentTurn = 1;
		turnStartArmy = ArmyManager.GetFirstPopulatedArmyNumber();
		StartTurn();
	}

	static void StartTurn(){
		currentArmy = ArmyManager.GetArmy(turnStartArmy);
		StartPhase();
	}

	static void EndTurn(){
	}

	static void AdvanceTurn(){
		EndTurn();
		currentTurn++;
		currentArmy = ArmyManager.AdvanceToNextArmy();
		StartTurn();
	}

	static void StartPhase(){
		foreach(MapUnit unit in currentArmy){
			unit.StartTurn();
		}
		turnDisplay.StartTurnAnimation(currentTurn,ArmyManager.GetCurrentArmyNumber());
	}

	/* ends the current army's turn (public so it can be forcibly called without moving all units) */
	public static void EndPhase(){
		foreach(MapUnit unit in currentArmy){
			unit.EndTurn();
		}
		NextPhase();
	}

	/* after a unit finishes an action, this checks if all units in the army have finished their actions and ends the phase if so */
	public static void CheckPhase(){
		if(currentArmy == null){
			return;
		}
		foreach(MapUnit unit in currentArmy){
			if(unit.turnState != TurnState.Finished){
				return;
			}
		}
		EndPhase();
	}

	/* cycles to the next phase, advancing turns if needed*/
	static void NextPhase(){
		if(ArmyManager.GetNextArmyNumber() == turnStartArmy){
			AdvanceTurn();
		}
		else{
			currentArmy = ArmyManager.AdvanceToNextArmy();
			StartPhase();
		}
	}

	public static int GetCurrentTurn(){
		return currentTurn;
	}

	
}
