using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillTriggerID{
	TT_NONE,
	TT_ALWAYS_ACTIVE,
	TT_COMBAT_START,

	AlwaysActive,
	OnTurnStart,
	OnTurnEnd,
	OnCombatStart,
	OnCombatEnd,
	OnTakeDamage,
	OnSpecialActivate,
	OnAssistUsed
}

public static class SkillTriggerIDExtensions{
	public static SkillTriggerID GetTriggerID(string input){
		SkillTriggerID id;
		if(Enum.TryParse(input, true, out id)){
			return id;
		}
		else{
			return SkillTriggerID.TT_NONE;
		}
	}
}