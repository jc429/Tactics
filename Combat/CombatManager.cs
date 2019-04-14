using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CombatManager {
    
	public static CombatForecast combatForecast;

	public static CombatInfo combatInfo;

	public static void Initialize(){
		if(combatInfo != null){
			combatInfo.Clear();
		}
		else{
			combatInfo = new CombatInfo();
		}
	}

	public static void PreCalculateCombat(HexUnit attackUnit, HexUnit defendUnit){
		if(attackUnit == null || defendUnit == null){
			Debug.Log("Combat failed! Not enough members!");
			return;
		}

		int damage;
		int attackerCurrentHP = attackUnit.CurrentHP;
		int defenderCurrentHP = defendUnit.CurrentHP;

		//under normal condition, attacker attacks
		damage = ResolveCombatRound(attackUnit, defendUnit, ref attackerCurrentHP, ref defenderCurrentHP);
		if(attackerCurrentHP <= 0 || defenderCurrentHP <= 0){
			return;
		}
		//defender counterattacks
		damage = ResolveCombatRound(defendUnit, attackUnit, ref defenderCurrentHP, ref attackerCurrentHP);
		if(attackerCurrentHP <= 0 || defenderCurrentHP <= 0){
			return;
		}

	}

	public static void StartCombat(HexUnit attackUnit, HexUnit defendUnit, int combatRange){
		if(attackUnit == null || defendUnit == null){
			Debug.Log("Combat failed! Not enough members!");
			return;
		}
		Debug.Log(attackUnit.Properties.movementClass + " is attacking " + defendUnit.Properties.movementClass + "!");

		int damage;
		bool roundResult;
		int attackerCurrentHP;
		int defenderCurrentHP;

		//under normal condition, attacker attacks
		attackerCurrentHP = attackUnit.CurrentHP;
		defenderCurrentHP = defendUnit.CurrentHP;
		damage = ResolveCombatRound(attackUnit, defendUnit, ref attackerCurrentHP, ref defenderCurrentHP);
		roundResult = defendUnit.TakeDamage(damage);
		if(roundResult){
			Debug.Log("Foe successfully defeated!");
			return;
		}

		//defender counterattacks
		attackerCurrentHP = attackUnit.CurrentHP;
		defenderCurrentHP = defendUnit.CurrentHP;
		damage = ResolveCombatRound(defendUnit, attackUnit, ref defenderCurrentHP, ref attackerCurrentHP);
		roundResult = attackUnit.TakeDamage(damage);
		if(roundResult){
			Debug.Log("Oh no! Defeated by foe's counterattack!");
			return;
		}
		Debug.Log("Combat ended. No unit perished.");
	}
	
	/* returns damage to be dealt */
	static int ResolveCombatRound(HexUnit currentAttacker, HexUnit currentDefender, ref int atkHP, ref int defHP){
		int attackerAtk = currentAttacker.Properties.GetStat(CombatStat.Atk);
		int defenderDef = currentDefender.Properties.GetStat(CombatStat.Def);

		int damage = attackerAtk - defenderDef;
		//Debug.Log(attackerAtk + " - " + defenderDef + " = " + damage);

		return damage;
	}

}
