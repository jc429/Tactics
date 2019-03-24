using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitProperties : System.Object{
	public MovementClass movementClass;
	public WeaponType weaponType;
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] stats = new int[(int)CombatStat.Total];

	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] statBuffs = new int[(int)CombatStat.Total];
	
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] statDebuffs = new int[(int)CombatStat.Total];

	public UnitProperties(){
		foreach(int i in stats){
			stats[i] = 1;
			statBuffs[i] = 0;
			statDebuffs[i] = 0;
		}
	}

	/* applies a buff to the desired stat */
	public void ApplyBuff(CombatStat stat, int amount){
		amount = Mathf.Clamp(amount,0,7);
		statBuffs[(int)stat] = Mathf.Max(statBuffs[(int)stat],amount);
	}

	/* applies a debuff to the desired stat */
	public void ApplyDebuff(CombatStat stat, int amount){
		amount = Mathf.Clamp(amount,-7,0);
		statDebuffs[(int)stat] = Mathf.Min(statDebuffs[(int)stat],amount);
	}

	/* resets all buffs to zero */
	public void ClearBuffs(){
		foreach(int i in statBuffs){
			statBuffs[i] = 0;
		}
	}

	/* resets all debuffs to zero */
	public void ClearDebuffs(){
		foreach(int i in statDebuffs){
			statDebuffs[i] = 0;
		}
	}
}
