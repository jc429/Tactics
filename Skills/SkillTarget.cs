using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillTarget{
    TARGET_NONE = 0,
    TARGET_SELF = 1,
	TARGET_FOE = 2
}

public static class SkillTargetExtensions{

	public static SkillTarget GetSkillTarget(string input){
		SkillTarget id;
		if(Enum.TryParse(input, true, out id)){
			return id;
		}
		else{
			return SkillTarget.TARGET_NONE;
		}
	}
	
}