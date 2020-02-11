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

	public static void PreCalculateCombat(MapUnit attackUnit, MapUnit defendUnit){
		if(attackUnit == null || defendUnit == null){
			Debug.Log("Combat failed! Not enough members!");
			return;
		}
		if(attackUnit == combatInfo.Attacker && defendUnit == combatInfo.Defender){
			/* if the attacker/defender are the same, and the attacker hasnt changed tiles, no need to recalculate combat
			but if the attacker is in a new position, recalculate combat in case AOE buffs have changed*/
			if(attackUnit.CurrentCell == combatInfo.attackerTile){
				return;
			}
		}

		combatInfo.Clear();
		combatInfo.SetAttackerDefender(attackUnit, defendUnit);
		attackUnit.Properties.CombatProperties.CalculateStartingStats();
		defendUnit.Properties.CombatProperties.CalculateStartingStats();
		attackUnit.StartCombat(defendUnit);
		defendUnit.StartCombat(attackUnit);

		int attackerCurrentHP = attackUnit.CurrentHP;
		int defenderCurrentHP = defendUnit.CurrentHP;



		//under normal condition, attacker attacks
		HitInfo hit = ResolveCombatRound(true, attackerCurrentHP, defenderCurrentHP);
		combatInfo.EnqueueHitInfo(hit);
		if(hit.attackerEndHP <= 0 || hit.defenderEndHP <= 0){
			return;
		}
		//defender counterattacks
		hit = ResolveCombatRound(false, hit.attackerEndHP, hit.defenderEndHP);
		combatInfo.EnqueueHitInfo(hit);
		if(hit.attackerEndHP <= 0 || hit.defenderEndHP <= 0){
			return;
		}
		
		
	}


	public static void CalculateAndPerformCombat(MapUnit attackUnit, MapUnit defendUnit){
		PreCalculateCombat(attackUnit, defendUnit);
		instance.StartCoroutine(instance.PlayCombat(combatInfo));
	}
	

	/* resolves one hit between two units */
	static HitInfo ResolveCombatRound(bool attackerIsAttackUnit, int atkStartHP, int defStartHP){
		HitInfo info = new HitInfo(attackerIsAttackUnit, atkStartHP, defStartHP);

		MapUnit currentAttacker = attackerIsAttackUnit ? combatInfo.Attacker : combatInfo.Defender;
		MapUnit currentDefender = attackerIsAttackUnit ? combatInfo.Defender : combatInfo.Attacker;

		int attackerAtk = currentAttacker.Properties.GetStat(CombatStat.Atk);
		int defenderDef = currentDefender.Properties.GetStat(CombatStat.Def);

		int damage = attackerAtk - defenderDef;
		Debug.Log(attackerAtk + " atk vs " + defenderDef + " def: " + damage + " damage dealt to " + currentDefender.Properties.weaponType);

		if(damage < 0){
			damage = 0;
		}
		if(attackerIsAttackUnit){
			info.defenderEndHP = info.defenderStartHP - damage;
			if(info.defenderEndHP < 0){
				info.defenderEndHP = 0;
			}
			info.attackerEndHP = atkStartHP;
		}
		else{
			info.attackerEndHP = info.attackerStartHP - damage;
			if(info.attackerEndHP < 0){
				info.attackerEndHP = 0;
			}
			info.defenderEndHP = defStartHP;
		}

		return info;
	}

	public IEnumerator PlayCombat(CombatInfo info){
		while(info.GetHitCount() > 0){
			HitInfo currentAttack = info.GetHitInfo();
			MapUnit attacker = currentAttack.attackerIsAttackUnit ? info.Attacker : info.Defender;
			MapUnit defender = currentAttack.attackerIsAttackUnit ? info.Defender : info.Attacker;
			yield return attacker.Animator.PerformAttackAnimation();
			info.Attacker.SetCurrentHP(currentAttack.attackerEndHP);
			info.Defender.SetCurrentHP(currentAttack.defenderEndHP);
			yield return new WaitForSeconds(0.15f);
		}
		combatInfo.Clear();
	}


}
