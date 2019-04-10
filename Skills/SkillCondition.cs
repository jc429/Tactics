using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* one single skill condition, and all the variables it needs to perform its job */

[System.Serializable]
public class SkillCondition{
    SkillConditionData conditionData;

	List<int> vars = new List<int>();






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
