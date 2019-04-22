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
		if(attackUnit == combatInfo.Attacker && defendUnit == combatInfo.Defender){
			return;
		}

		combatInfo.Clear();
		combatInfo.SetAttackerDefender(attackUnit, defendUnit);
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

	public static void CalculateAndPerformCombat(HexUnit attackUnit, HexUnit defendUnit){
		PreCalculateCombat(attackUnit, defendUnit);
		instance.StartCoroutine(instance.PlayCombat(combatInfo));
	}
	

	/* resolves one hit between two units */
	static HitInfo ResolveCombatRound(bool attackerIsAttackUnit, int atkStartHP, int defStartHP){
		HitInfo info = new HitInfo(attackerIsAttackUnit, atkStartHP, defStartHP);

		HexUnit currentAttacker = attackerIsAttackUnit ? combatInfo.Attacker : combatInfo.Defender;
		HexUnit currentDefender = attackerIsAttackUnit ? combatInfo.Defender : combatInfo.Attacker;

		int attackerAtk = currentAttacker.Properties.GetStat(CombatStat.Atk);
		int defenderDef = currentDefender.Properties.GetStat(CombatStat.Def);

		int damage = attackerAtk - defenderDef;
		Debug.Log(attackerAtk + " atk vs " + defenderDef + " def: " + damage + " damage dealt to " + currentDefender.properties.weaponType);

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
			HexUnit attacker = currentAttack.attackerIsAttackUnit ? info.Attacker : info.Defender;
			HexUnit defender = currentAttack.attackerIsAttackUnit ? info.Defender : info.Attacker;
			yield return attacker.Animator.PerformAttackAnimation();
			info.Attacker.SetCurrentHP(currentAttack.attackerEndHP);
			info.Defender.SetCurrentHP(currentAttack.defenderEndHP);
			yield return new WaitForSeconds(0.15f);
		}
		combatInfo.Clear();
	}


}
