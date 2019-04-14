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

	HexUnit Unit{
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
		HexUnit targetUnit = null;
		switch(target){
		case SkillTarget.TARGET_NONE:
			break;
		case SkillTarget.TARGET_SELF:
			targetUnit = Unit;
			break;
		default:
			break;
		}

		if(targetUnit == null){
			return;
		}

		switch(effectData.eID){
		case EffectID.EFF_NONE:
			return;
		case EffectID.EFF_MODIFY_STAT:
			Unit.Properties.ApplyStatMod(vars[0], vars[1]);
			Debug.Log("Effect applied! Added " + vars[1] + " to stat " + vars[0] + "!");
			return;

		default:
			return;
		}
	}
}
