using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CombatManager {
    

	public static void StartCombat(HexUnit attackUnit, HexUnit defendUnit, int combatRange){
		if(attackUnit == null || defendUnit == null){
			Debug.Log("Combat failed! Not enough members!");
			return;
		}
		Debug.Log(attackUnit.Properties.movementClass + " is attacking " + defendUnit.Properties.movementClass + "!");

		
		bool roundResult;

		//under normal condition, attacker attacks
		roundResult = ResolveCombatRound(attackUnit, defendUnit);
		if(roundResult){
			Debug.Log("Foe successfully defeated!");
			return;
		}
		//defender counterattacks
		roundResult = ResolveCombatRound(defendUnit, attackUnit);
		if(roundResult){
			Debug.Log("Oh no! Defeated by foe's counterattack!");
			return;
		}
		Debug.Log("Combat ended. No unit perished.");
	}
	
	/* returns true if the defender takes lethal damage */
	static bool ResolveCombatRound(HexUnit currentAttacker, HexUnit currentDefender){
		int attackerAtk = currentAttacker.Properties.GetStatUnmodified(CombatStat.Str);

		int defenderDef = currentDefender.Properties.GetStatUnmodified(CombatStat.Def);

		int damage = attackerAtk - defenderDef;

		Debug.Log(attackerAtk + " - " + defenderDef + " = " + damage);

		bool defenderDefeated = currentDefender.TakeDamage(damage);

		return defenderDefeated;
	}
}
