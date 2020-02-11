using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* one single skill effect, and all the variables it needs to perform its job */

[System.Serializable]
public class SkillEffect{
	[System.NonSerialized]
	public SkillEffectFamily parentEffectFamily;

	public int positionInFamily;

    SkillEffectData effectData;
	SkillTarget target;

	int[] vars;

	MapUnit Unit{
		get{
			return parentEffectFamily.parentPair.parentSkill.unit;
		}
	}
	
	public SkillEffect(SkillEffectData data, SkillTarget target = SkillTarget.TARGET_NONE){
		this.effectData = data;
		this.target = target;
		if(data.varCount > 0){
			vars = new int[data.varCount];
		}
	}

	public int GetVarCount(){
		return effectData.varCount;
	}

	public EffectID GetEffectID(){
		return effectData.eID;
	}

	public string GetEffectIDString(){
		return effectData.eID.ToString("G");
	}

	public string GetTargetString(){
		return target.ToString("G");
	}

	public bool SetVariable(int value, int pos){
		if(pos < 0 || pos >= vars.Length){
			Debug.Log("Invalid Position!");
			return false;
		}
		vars[pos] = value;
		return true;
	}

	public void Apply(){
		Queue<MapUnit> targetList = new Queue<MapUnit>();
		switch(target){
		case SkillTarget.TARGET_NONE:
			break;
		case SkillTarget.TARGET_SELF:
			targetList.Enqueue(Unit);
			break;
		case SkillTarget.TARGET_COMBAT_FOE:
			targetList.Enqueue(Unit.Properties.CombatProperties.foe);
			break;
		default:
			break;
		}

		while(targetList.Count > 0){
			MapUnit targetUnit = targetList.Dequeue();
			if(targetUnit == null){
				continue;
			}
			ApplyEffectToTarget(targetUnit);
		}

	}

	void ApplyEffectToTarget(MapUnit targetUnit){
		byte statChangeMatrix;

		switch(effectData.eID){
		case EffectID.EFF_NONE:
			return;
		case EffectID.EFF_MODIFY_STAT:
			statChangeMatrix = (byte)Mathf.Clamp(vars[0], 0, 255);
			targetUnit.Properties.ApplyStatMods(statChangeMatrix, vars[1]);
			Debug.Log("Effect applied! Added " + vars[1] + " to stat " + vars[0] + "!");
			return;
		case EffectID.EFF_BUFF_STAT:
			statChangeMatrix = (byte)Mathf.Clamp(vars[0], 0, 255);
			targetUnit.Properties.ApplyBuffs(statChangeMatrix, vars[1]);
			return;
		case EffectID.EFF_DEBUFF_STAT:
			statChangeMatrix = (byte)Mathf.Clamp(vars[0], 0, 255);
			targetUnit.Properties.ApplyDebuffs(statChangeMatrix, vars[1]);
			return;
		case EffectID.EFF_MODIFY_STAT_COMBAT:
			statChangeMatrix = (byte)Mathf.Clamp(vars[0], 0, 255);
			targetUnit.Properties.CombatProperties.ModifyStats(statChangeMatrix, vars[1]);
			return;

		default:
			return;
		}
	}
}
