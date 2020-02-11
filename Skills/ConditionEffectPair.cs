using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* a CE pair combines one condition family with one effect family, in other words, one Cause with one Effect */

[System.Serializable]
public class ConditionEffectPair{
	[System.NonSerialized]
	public Skill parentSkill;

	public int pairID;		//mostly for debugging purposes

	public SkillTriggerID triggerType;

	public string conditionFamilyID;
	public string effectFamilyID;
	public SkillConditionFamily conditionFamily;
	public SkillEffectFamily effectFamily;

	MapUnit Unit{
		get{
			return parentSkill.unit;
		}
	}

	public ConditionEffectPair(){
		conditionFamily = new SkillConditionFamily();
		conditionFamily.parentPair = this;
		effectFamily = new SkillEffectFamily();
		effectFamily.parentPair = this;
	}


	public bool Resolve(){
		bool conditionsMet = conditionFamily.CheckConditionsMet();
		if(conditionsMet){
			effectFamily.ApplyEffects();
		}
		return conditionsMet;
	}
}
