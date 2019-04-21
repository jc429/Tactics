using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class UnitProperties : System.Object{
	public HexUnit unit;

	public MovementClass movementClass;
	public WeaponType weaponType;
	
	/* raw stats are the unit's stats with no skills applied */
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] rawStats = new int[(int)CombatStat.Total];
	/* any modifiers to stats provided by skills go here */
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] statModifiers = new int[(int)CombatStat.Total];
	/* buffs applied to each stat (range 0 to +7) */
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] fieldBuffs = new int[(int)CombatStat.Total];
	/* debuffs applied to each stat (range 0 to -7) */
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] fieldDebuffs = new int[(int)CombatStat.Total];
	/* manipulations to stats applied during combat */
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] combatBuffs = new int[(int)CombatStat.Total];


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
			rawStats[i] = 1;
			statModifiers[i] = 0;
			fieldBuffs[i] = 0;
			fieldDebuffs[i] = 0;
			combatBuffs[i] = 0;
		}
	}

	/* sets a stat to the desired stat */
	public void SetStat(CombatStat stat, int amount){
		amount = Mathf.Clamp(amount,0,99);
		rawStats[(int)stat] = amount;
	}

	/* applies a stat modifier */
	public void ApplyStatMod(CombatStat stat, int value){
		statModifiers[(int)stat] += value;
	}
	/* applies multiple stat modifiers */
	public void ApplyStatMods(byte statMatrix, int value){
		for(int i = 0; i < (int)CombatStat.Total; i++){
			if((statMatrix & (1 << i)) != 0){
				statModifiers[i] += value;
			}
		}
	}

	/* returns a given stat with no modifiers */
	public int GetRawStat(CombatStat stat){
		return rawStats[(int)stat];
	}

	/* returns a given stat with no buffs/debuffs */
	public int GetDisplayStat(CombatStat stat){
		return rawStats[(int)stat] + statModifiers[(int)stat];
	}

	/* returns a given stat with mods applied */
	public int GetStat(CombatStat stat, bool ignoreBuffs = false, bool ignoreDebuffs = false){
		return  rawStats[(int)stat] + statModifiers[(int)stat] 
			+ (ignoreBuffs ? 0 : fieldBuffs[(int)stat])
			+ (ignoreDebuffs ? 0 : fieldDebuffs[(int)stat]);
	}

	/* returns a given stat with mods applied */
	public int GetStat(int stat, bool ignoreBuffs = false, bool ignoreDebuffs = false){
		return rawStats[stat] + statModifiers[stat] 
			+ (ignoreBuffs ? 0 : fieldBuffs[(int)stat])
			+ (ignoreDebuffs ? 0 : fieldDebuffs[(int)stat]);
	}

	/* randomizes stats */
	public void RandomizeStats(){
		rawStats[0] = 40 + Random.Range(0,20);
		rawStats[1] = 30 + Random.Range(0,20);
		rawStats[2] = 30 + Random.Range(0,20);
		rawStats[3] = 30 + Random.Range(0,20);
		rawStats[4] = 20 + Random.Range(0,20);
		rawStats[5] = 20 + Random.Range(0,20);
	}

	/* applies a buff to the desired stat */
	public void ApplyBuff(CombatStat stat, int amount){
		amount = Mathf.Clamp(amount,0,7);
		fieldBuffs[(int)stat] = Mathf.Max(fieldBuffs[(int)stat],amount);
	}
	/* applies a buff to multiple stats */
	public void ApplyBuffs(byte statMatrix, int amount){
		amount = Mathf.Clamp(amount,0,7);
		for(int i = 0; i < (int)CombatStat.Total; i++){
			if((statMatrix & (1 << i)) != 0){
				fieldBuffs[i] += amount;
			}
		}
	}

	/* applies a debuff to the desired stat */
	public void ApplyDebuff(CombatStat stat, int amount){
		amount = Mathf.Clamp(amount,-7,0);
		fieldDebuffs[(int)stat] = Mathf.Min(fieldDebuffs[(int)stat],amount);
	}
	/* applies a debuff to multiple stats */
	public void ApplyDebuffs(byte statMatrix, int amount){
		amount = Mathf.Clamp(amount,-7,0);
		for(int i = 0; i < (int)CombatStat.Total; i++){
			if((statMatrix & (1 << i)) != 0){
				fieldDebuffs[i] += amount;
			}
		}
	}

	/* resets all stats to one (not zero bc hp of 0 would break the game) */
	void ClearStats(){
		foreach(int i in rawStats){
			rawStats[i] = 1;
		}
	}

	/* not really sure this should exist/be used, skills should handle the removal of modifiers on their own */
	void ClearStatModifiers(){
		foreach(int i in rawStats){
			statModifiers[i] = 0;
		}
	}

	/* resets all buffs to zero */
	public void ClearBuffs(){
		foreach(int i in fieldBuffs){
			fieldBuffs[i] = 0;
		}
	}

	/* resets all debuffs to zero */
	public void ClearDebuffs(){
		foreach(int i in fieldDebuffs){
			fieldDebuffs[i] = 0;
		}
	}

	/* resets all combat mods to zero */
	public void ClearCombatBuffs(){
		foreach(int i in combatBuffs){
			combatBuffs[i] = 0;
		}
	}

	/* set unit's skill (slot automatically chosen based on skilltype) */
	public void SetSkill(Skill skill){
		int type = (int)skill.skillType;
		skillIDs[type] = skill.skillNo;
		skills[type] = skill;
	}

	/* set unit's skill via ID (slot automatically chosen based on skilltype) */
	public void SetSkill(int skillID){
		if(skillID == 0){
			return;
		}
		Skill skill = SkillDBReader.LoadSkill(skillID);
		int type = (int)skill.skillType;
		skill.unit = unit;

		if(type == 0){	//null type, something went wrong
			Debug.Log("WARNING: This skill has no type! Unintended behavior may follow!");
			//just write to slot 0
			RemoveSkill(type);
			skillIDs[type] = skillID;
			skills[type] = skill;
		}
		else{
			RemoveSkill(type - 1);
			skillIDs[type - 1] = skillID;
			skills[type - 1] = skill;
		}
		//Debug.Log(":)");


		/* hook up event listeners and stuff here */
		foreach(ConditionEffectPair cePair in skill.cePairs){
			switch(cePair.triggerType){
			case SkillTriggerID.TT_ALWAYS_ACTIVE:
				//apply effect immediately
				cePair.Resolve();
				break;
			case SkillTriggerID.TT_TURN_START:
				unit.skillEventHandler.onTurnStart.Add(cePair);
				break;
			case SkillTriggerID.TT_TURN_END:
				unit.skillEventHandler.onTurnEnd.Add(cePair);
				break;
			case SkillTriggerID.TT_COMBAT_START:
				unit.skillEventHandler.onCombatStart.Add(cePair);
				break;
			case SkillTriggerID.TT_COMBAT_END:
				unit.skillEventHandler.onCombatEnd.Add(cePair);
				break;
			case SkillTriggerID.TT_ASSIST_USED:
				unit.skillEventHandler.onAssistUsed.Add(cePair);
				break;
			case SkillTriggerID.TT_RECEIVE_DAMAGE:
				unit.skillEventHandler.onTakeDamage.Add(cePair);
				break;
			case SkillTriggerID.TT_SPECIAL_ACTIVATE:
				unit.skillEventHandler.onSpecialActivate.Add(cePair);
				break;
			default:
				break;
			}
		}
	}

	public void RemoveSkill(int skillPos){
		skillIDs[skillPos] = 0;
		Skill s = skills[skillPos];
		if(s != null){
			/* remove the associated event handlers for the skill */
			foreach(ConditionEffectPair cePair in s.cePairs){
				switch(cePair.triggerType){
				case SkillTriggerID.TT_ALWAYS_ACTIVE:
					break;
				case SkillTriggerID.TT_TURN_START:
					unit.skillEventHandler.onTurnStart.Remove(cePair);
					break;
				case SkillTriggerID.TT_TURN_END:
					unit.skillEventHandler.onTurnEnd.Remove(cePair);
					break;
				case SkillTriggerID.TT_COMBAT_START:
					unit.skillEventHandler.onCombatStart.Remove(cePair);
					break;
				case SkillTriggerID.TT_COMBAT_END:
					unit.skillEventHandler.onCombatEnd.Remove(cePair);
					break;
				case SkillTriggerID.TT_ASSIST_USED:
					unit.skillEventHandler.onAssistUsed.Remove(cePair);
					break;
				case SkillTriggerID.TT_RECEIVE_DAMAGE:
					unit.skillEventHandler.onTakeDamage.Remove(cePair);
					break;
				case SkillTriggerID.TT_SPECIAL_ACTIVATE:
					unit.skillEventHandler.onSpecialActivate.Remove(cePair);
					break;
				default:
					break;
				}
			}
		}
		s.unit = null;
		skills[skillPos] = null;
	}

	/* remove all of unit's skills */
	public void ClearSkills(){
		for(int i = 0; i < skillIDs.Length; i++){
			RemoveSkill(i);
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
			writer.Write(rawStats[i]);
		}
		for(int i = 0; i < skillIDs.Length; i++){
			writer.Write(skillIDs[i]);
		}
	}

	/* load unit properties from file */
	public void Load (BinaryReader reader) {
		movementClass = (MovementClass)reader.ReadInt32();
		weaponType = (WeaponType)reader.ReadInt32();
		affiliation = reader.ReadInt32();
		for(int i = 0; i < (int)CombatStat.Total; i++){
			rawStats[i] = reader.ReadInt32();
			statModifiers[i] = 0;
		}
		for(int i = 0; i < skillIDs.Length; i++){
			skillIDs[i] = reader.ReadInt32();
			SetSkill(skillIDs[i]);
		}

	}
}
