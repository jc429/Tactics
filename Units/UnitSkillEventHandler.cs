using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.intertech.com/Blog/c-sharp-tutorial-understanding-c-events/


[System.Serializable]
public class UnitSkillEventHandler{
	public HexUnit unit;

	public delegate bool resolveSkill(ConditionEffectPair cePair);
	
	public event resolveSkill OnTurnStart;
	public event resolveSkill OnTurnEnd;
	public event resolveSkill OnCombatStart;
	public event resolveSkill OnCombatEnd;
	public event resolveSkill OnTakeDamage;
	public event resolveSkill OnSpecialActivate;
	public event resolveSkill OnAssistUsed;



}