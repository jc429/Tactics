using System.Collections;
using System.Collections.Generic;
using System.IO;
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

	//which army unit is a member of
	public int affiliation;

	/* returns with an attack range based on unit's weapon type */
	public int AttackRange{
		get{
			if(weaponType == WeaponType.Sword || weaponType == WeaponType.Lance || weaponType == WeaponType.Axe){
				return 1;
			}
			else{
				return 2;
			}
		}
	}

	public UnitProperties(){
		for(int i = 0; i < (int)CombatStat.Total; i++){
			stats[i] = 1;
			statBuffs[i] = 0;
			statDebuffs[i] = 0;
		}
	}


	/* sets a stat to the desired stat */
	public void SetStat(CombatStat stat, int amount){
		amount = Mathf.Clamp(amount,0,99);
		stats[(int)stat] = amount;
	}

	/* returns a given stat with no modifiers */
	public int GetStatUnmodified(CombatStat stat){
		return stats[(int)stat];
	}

	/* returns a given stat with mods applied */
	public int GetStat(CombatStat stat, bool ignoreBuffs = false, bool ignoreDebuffs = false){
		return stats[(int)stat] 
			+ (ignoreBuffs ? statBuffs[(int)stat] : 0)
			+ (ignoreDebuffs ? statDebuffs[(int)stat] : 0);
	}

	public int GetStat(int stat, bool ignoreBuffs = false, bool ignoreDebuffs = false){
		return stats[stat] 
			+ (ignoreBuffs ? statBuffs[(int)stat] : 0)
			+ (ignoreDebuffs ? statDebuffs[(int)stat] : 0);
	}


	/* randomizes stats */
	public void RandomizeStats(){
		stats[0] = 40 + Random.Range(0,20);
		stats[1] = 30 + Random.Range(0,20);
		stats[2] = 30 + Random.Range(0,20);
		stats[3] = 30 + Random.Range(0,20);
		stats[4] = 20 + Random.Range(0,20);
		stats[5] = 20 + Random.Range(0,20);
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

	/* resets all stats to one (not zero bc hp of 0 would break the game) */
	void ClearStats(){
		foreach(int i in stats){
			stats[i] = 1;
		}
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

	public void Clear(){
		ClearStats();
		ClearBuffs();
		ClearDebuffs();
	}


	
	/* save unit properties to file */
	public void Save (BinaryWriter writer) {
		writer.Write((int)movementClass);
		writer.Write((int)weaponType);
		for(int i = 0; i < (int)CombatStat.Total; i++){
			writer.Write(stats[i]);
		}
	}

	public void Load (BinaryReader reader) {
		movementClass = (MovementClass)reader.ReadInt32();
		weaponType = (WeaponType)reader.ReadInt32();
		for(int i = 0; i < (int)CombatStat.Total; i++){
			stats[i] = reader.ReadInt32();
		}
	}
}
