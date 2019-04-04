using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class UnitProperties : System.Object{
	public MovementClass movementClass;
	public WeaponType weaponType;
	
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] baseStats = new int[(int)CombatStat.Total];
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] statBuffs = new int[(int)CombatStat.Total];
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] statDebuffs = new int[(int)CombatStat.Total];

	public int[] skillIDs = new int[7];
	public Skill[] skills = new Skill[7];

	//which army unit is a member of
	public int affiliation;

	/* returns with an attack range based on unit's weapon type */
	public int AttackRange{
		get{
			if(weaponType == WeaponType.Sword || weaponType == WeaponType.Lance || weaponType == WeaponType.Axe){
				return 1;
			}
			else if(weaponType == WeaponType.None){
				return 0;
			}
			else{
				return 2;
			}
		}
	}

	public UnitProperties(){
		for(int i = 0; i < (int)CombatStat.Total; i++){
			baseStats[i] = 1;
			statBuffs[i] = 0;
			statDebuffs[i] = 0;
		}
	}

	/* sets a stat to the desired stat */
	public void SetStat(CombatStat stat, int amount){
		amount = Mathf.Clamp(amount,0,99);
		baseStats[(int)stat] = amount;
	}

	/* returns a given stat with no modifiers */
	public int GetStatUnmodified(CombatStat stat){
		return baseStats[(int)stat];
	}

	/* returns a given stat with mods applied */
	public int GetStat(CombatStat stat, bool ignoreBuffs = false, bool ignoreDebuffs = false){
		return baseStats[(int)stat] 
			+ (ignoreBuffs ? 0 : statBuffs[(int)stat])
			+ (ignoreDebuffs ? 0 : statDebuffs[(int)stat]);
	}

	public int GetStat(int stat, bool ignoreBuffs = false, bool ignoreDebuffs = false){
		return baseStats[stat] 
			+ (ignoreBuffs ? 0 : statBuffs[(int)stat])
			+ (ignoreDebuffs ? 0 : statDebuffs[(int)stat]);
	}

	/* randomizes stats */
	public void RandomizeStats(){
		baseStats[0] = 40 + Random.Range(0,20);
		baseStats[1] = 30 + Random.Range(0,20);
		baseStats[2] = 30 + Random.Range(0,20);
		baseStats[3] = 30 + Random.Range(0,20);
		baseStats[4] = 20 + Random.Range(0,20);
		baseStats[5] = 20 + Random.Range(0,20);
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
		foreach(int i in baseStats){
			baseStats[i] = 1;
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

	/* set unit's skill (slot automatically chosen based on skilltype) */
	public void SetSkill(Skill skill){
		int type = (int)skill.skillType;
		skillIDs[type] = skill.skillID;
		skills[type] = skill;
	}

	/* set unit's skill via ID (slot automatically chosen based on skilltype) */
	public void SetSkill(int skillID){
		if(skillID == 0){
			return;
		}
		Skill skill = SkillTable.GetSkill(skillID);
		int type = (int)skill.skillType;
		if(type == 0){	//null type, something went wrong
			//just write to slot 0
			skillIDs[type] = skillID;
			skills[type] = skill;
		}
		else{
			skillIDs[type - 1] = skillID;
			skills[type - 1] = skill;
		}
		//Debug.Log(":)");
	}

	/* remove all of unit's skills */
	public void ClearSkills(){
		for(int i = 0; i < 7; i++){
			skillIDs[i] = 0;
			skills[i] = null;
		}
	}

	public void Clear(){
		ClearStats();
		ClearBuffs();
		ClearDebuffs();
		ClearSkills();
	}
	
	/* save unit properties to file */
	public void Save (BinaryWriter writer) {
		writer.Write((int)movementClass);
		writer.Write((int)weaponType);
		writer.Write(affiliation);
		for(int i = 0; i < (int)CombatStat.Total; i++){
			writer.Write(baseStats[i]);
		}
		for(int i = 0; i < 7; i++){
			writer.Write(skillIDs[i]);
		}
	}

	/* load unit properties from file */
	public void Load (BinaryReader reader) {
		movementClass = (MovementClass)reader.ReadInt32();
		weaponType = (WeaponType)reader.ReadInt32();
		affiliation = reader.ReadInt32();
		for(int i = 0; i < (int)CombatStat.Total; i++){
			baseStats[i] = reader.ReadInt32();
		}
		for(int i = 0; i < 7; i++){
			skillIDs[i] = reader.ReadInt32();
			SetSkill(skillIDs[i]);
		}

	}
}
