using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatInfo {
	HexUnit attacker;
	HexUnit defender;

	public HexUnit Attacker{
		get{ return attacker; }
	}
	public HexUnit Defender{
		get{ return defender; }
	}

	public void Clear(){
		attacker = defender = null;
	}

	public void SetAttackerDefender(HexUnit attackUnit, HexUnit defendUnit){
		attacker = attackUnit;
		defender = defendUnit;
	}
}