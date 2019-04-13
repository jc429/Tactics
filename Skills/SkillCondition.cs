using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* one single skill condition, and all the variables it needs to perform its job */

[System.Serializable]
public class SkillCondition{
	[System.NonSerialized]
	public SkillConditionFamily parentConditionFamily;

    SkillConditionData conditionData;

	int[] vars;

	HexUnit Unit{
		get{
			return parentConditionFamily.parentPair.parentSkill.unit;
		}
	}


	public SkillCondition(SkillConditionData data){
		this.conditionData = data;
		if(data.varCount > 0){
			vars = new int[data.varCount];
		}
	}


	public int GetVarCount(){
		return conditionData.varCount;
	}

	public ConditionID GetConditionID(){
		return conditionData.cID;
	}

	public string GetConditionIDString(){
		return conditionData.cID.ToString("G");
	}

	public bool SetVariable(int value, int pos){
		if(pos < 0 || pos >= vars.Length){
			Debug.Log("Invalid Position!");
			return false;
		}
		vars[pos] = value;
		return true;
	}

	/* returns true if condition is fulfilled, false otherwise */
	public bool EvaluateConditionMet(){
		switch(conditionData.cID){
		case(ConditionID.CN_NONE):
			return true;

		default:
			return false;
		}
	}
}
