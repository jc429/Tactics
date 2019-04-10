using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* a CE pair combines one condition family with one effect family, in other words, one Cause with one Effect */

[System.Serializable]
public class ConditionEffectPair{
	public int pairID;		//mostly for debugging purposes

	public SkillTriggerType triggerType;

	public string conditionFamilyID;
	public string effectFamilyID;
	public SkillConditionFamily conditionFamily;
	public SkillEffectFamily effectFamily;

	public ConditionEffectPair(){
		triggerType = SkillTriggerType.AlwaysActive;
		conditionFamily = new SkillConditionFamily();
		effectFamily = new SkillEffectFamily();
	}
}
