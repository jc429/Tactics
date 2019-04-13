using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* a condition family is a group of skill conditions that are checked together (ex: if HP < X% AND opponent initiates combat) */

[System.Serializable]
public class SkillConditionFamily {
	[System.NonSerialized]
	public ConditionEffectPair parentPair;

	public List<SkillCondition> skillConditions;

	HexUnit Unit{
		get{
			return parentPair.parentSkill.unit;
		}
	}
	
	public SkillConditionFamily(){
		skillConditions = new List<SkillCondition>();
	}

	public void AddSkillCondition(SkillCondition condition){
		skillConditions.Add(condition);
		condition.parentConditionFamily = this;
	}

	public void ClearSkillConditions(){
		if(skillConditions != null){
			skillConditions.Clear();
		}
		else{
			skillConditions = new List<SkillCondition>();
		}
	} 

	public bool CheckConditionsMet(){
		if(skillConditions == null || skillConditions.Count <= 0){
			Debug.Log("No conditions to check!");
			return true;
		}
		bool success = true;
		foreach(SkillCondition sc in skillConditions){
			success &= sc.EvaluateConditionMet();
		}
		return success;
	}
}
