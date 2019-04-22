using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitInfo{
	// if true, this means the attacker of this hit is the same as the unit who initiated the combat
	public bool attackerIsAttackUnit;

	public int attackerStartHP;
	public int attackerEndHP;
	public int defenderStartHP;
	public int defenderEndHP;


	public HitInfo(){

	}

	public HitInfo(bool isAttacker, int atkHP, int defHP){
		attackerIsAttackUnit = isAttacker;
		attackerStartHP = atkHP;
		defenderStartHP = defHP;
	}
}
