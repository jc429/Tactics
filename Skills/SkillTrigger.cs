using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillTriggerType{
	AlwaysActive,
	OnTurnStart,
	OnTurnEnd,
	OnCombatStart,
	DuringCombat,
	OnCombatEnd,
	OnTakeDamage,
	OnSpecialActivate,
	OnAssistUsed
}