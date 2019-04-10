using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConditionID{
	CN_NONE = 0,
	CN_USER_INITIATES_COMBAT = 1,
	CN_FOE_INITIATES_COMBAT = 2,
	CN_HP_LESS_EQUAL_X = 3,
	CN_HP_GREATER_EQUAL_X = 4,
	CN_STAT_X_IS_Y_GREATER_FOE = 5,
	CN_STAT_X_IS_Y_LESS_FOE = 6,
	CN_X_ALLIES_IN_Y_RANGE = 7,
	CN_X_FOES_IN_Y_RANGE = 8,
	CN_HAS_ADJACENT_ALLY = 9,
	CN_IS_BUFFED = 10,
	CN_IS_DEBUFFED = 11,
}

public static class ConditionIDExtensions{

	public static ConditionID GetConditionID(string input){
		ConditionID id;
		if(Enum.TryParse(input, true, out id)){
			return id;
		}
		else{
			return ConditionID.CN_NONE;
		}
	}
	
}