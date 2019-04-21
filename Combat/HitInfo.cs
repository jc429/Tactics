using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitInfo{
	public HexUnit attacker;
	public HexUnit defender;

	public int attackerStartHP;
	public int attackerEndHP;
	public int defenderStartHP;
	public int defenderEndHP;


	public HitInfo(){

	}

	public HitInfo(HexUnit atkUnit, HexUnit defUnit, int atkHP, int defHP){
		attacker = atkUnit;
		defender = defUnit;
		attackerStartHP = atkHP;
		defenderStartHP = defHP;
	}
}
