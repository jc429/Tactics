using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* a condition family is a group of skill conditions that are checked together (ex: if HP < X% AND opponent initiates combat) */

[System.Serializable]
public class SkillConditionFamily
{
	List<SkillCondition> skillConditions;

	public SkillConditionFamily(){
		skillConditions = new List<SkillCondition>();
	}

	public void AddSkillCondition(SkillCondition condition){
		skillConditions.Add(condition);
	}

	public void ClearSkillConditions(){
		if(skillConditions != null){
			skillConditions.Clear();
		}
		else{
			skillConditions = new List<SkillCondition>();
		}
	}
}
