using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour{
	public static CombatManager instance;
    
	public static CombatForecast combatForecast;

	public static CombatInfo combatInfo;

	void Awake(){
		instance = this;
	}

	public static void Initialize(){
		if(combatInfo != null){
			combatInfo.Clear();
		}
		else{
			combatInfo = new CombatInfo();
		}
		combatForecast.Hide();
	}

	public static void PreCalculateCombat(HexUnit attackUnit, HexUnit defendUnit){
		if(attackUnit == null || defendUnit == null){
			Debug.Log("Combat failed! Not enough members!");
			return;
		}
		combatInfo.Clear();

		int attackerCurrentHP = attackUnit.CurrentHP;
		int defenderCurrentHP = defendUnit.CurrentHP;

		//under normal condition, attacker attacks
		HitInfo hit = ResolveCombatRound(attackUnit, defendUnit, attackerCurrentHP, defenderCurrentHP);
		combatInfo.EnqueueHitInfo(hit);
		if(hit.attackerEndHP <= 0 || hit.defenderEndHP <= 0){
			return;
		}
		//defender counterattacks
		hit = ResolveCombatRound(defendUnit, attackUnit, hit.defenderEndHP, hit.attackerEndHP);
		combatInfo.EnqueueHitInfo(hit);
		if(hit.attackerEndHP <= 0 || hit.defenderEndHP <= 0){
			return;
		}

		
	}

	public static void CalculateAndPerformCombat(HexUnit attackUnit, HexUnit defendUnit){
		PreCalculateCombat(attackUnit, defendUnit);
		instance.StartCoroutine(instance.PlayCombat(combatInfo));
	}
	/*
	public static void StartCombat(HexUnit attackUnit, HexUnit defendUnit, int combatRange){
		if(attackUnit == null || defendUnit == null){
			Debug.Log("Combat failed! Not enough members!");
			return;
		}
		Debug.Log(attackUnit.Properties.movementClass + " is attacking " + defendUnit.Properties.movementClass + "!");

		bool roundResult;
		int attackerCurrentHP;
		int defenderCurrentHP;
		Queue<HexUnit> attackOrder = new Queue<HexUnit>();

		//under normal condition, attacker attacks
		attackerCurrentHP = attackUnit.CurrentHP;
		defenderCurrentHP = defendUnit.CurrentHP;
		attackOrder.Enqueue(attackUnit);
		HitInfo hit = ResolveCombatRound(attackUnit, defendUnit, attackerCurrentHP, defenderCurrentHP);
		roundResult = defendUnit.SetCurrentHP(hit.defenderEndHP);
		if(roundResult){
			Debug.Log("Foe successfully defeated!");
			return;
		}

		//defender counterattacks
		attackerCurrentHP = hit.attackerEndHP;
		defenderCurrentHP = hit.defenderEndHP;
		attackOrder.Enqueue(defendUnit);
		HitInfo hit2 = ResolveCombatRound(defendUnit, attackUnit, defenderCurrentHP, attackerCurrentHP);
		roundResult = attackUnit.SetCurrentHP(hit.defenderEndHP);
		if(roundResult){
			Debug.Log("Oh no! Defeated by foe's counterattack!");
			return;
		}
		Debug.Log("Combat ended. No unit perished.");
		instance.StartCoroutine(instance.PlayCombatAnimations(attackOrder));
	}*/
	

	/* resolves one hit between two units */
	static HitInfo ResolveCombatRound(HexUnit currentAttacker, HexUnit currentDefender, int atkStartHP, int defStartHP){
		HitInfo info = new HitInfo(currentAttacker, currentDefender, atkStartHP, defStartHP);

		int attackerAtk = currentAttacker.Properties.GetStat(CombatStat.Atk);
		int defenderDef = currentDefender.Properties.GetStat(CombatStat.Def);

		int damage = attackerAtk - defenderDef;
		if(damage < 0){
			damage = 0;
		}
		info.defenderEndHP = info.defenderStartHP - damage;
		if(info.defenderEndHP < 0){
			info.defenderEndHP = 0;
		}
		info.attackerEndHP = atkStartHP;

		return info;
	}

	public IEnumerator PlayCombat(CombatInfo info){
		while(info.GetHitCount() > 0){
			HitInfo currentAttack = info.GetHitInfo();
			yield return currentAttack.attacker.Animator.PerformAttackAnimation();
			currentAttack.attacker.SetCurrentHP(currentAttack.attackerEndHP);
			currentAttack.defender.SetCurrentHP(currentAttack.defenderEndHP);
			yield return new WaitForSeconds(0.15f);
		}
	}


}
