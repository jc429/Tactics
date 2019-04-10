using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* one single skill effect, and all the variables it needs to perform its job */

[System.Serializable]
public class SkillEffect{
    SkillEffectData effectData;
	SkillTarget target;

	int[] vars;

	
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
}
