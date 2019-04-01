using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnUI : MonoBehaviour
{
	public Text turnNumber;
	public Text armyNumber;

	void Update(){
		turnNumber.text = "turn " + TurnManager.GetCurrentTurn();

		armyNumber.text = "army " + ArmyManager.GetCurrentArmyNumber();
	}
}
