using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.intertech.com/Blog/c-sharp-tutorial-understanding-c-events/

[System.Serializable]
public class SkillEventListeners{
	public HexUnit unit;

	public delegate void TurnStart();
	public delegate void TurnEnd();
	public delegate void CombatStart();
	public delegate void CombatEnd();
	public delegate void TakeDamage();
	public delegate void SpecialActivate();
	public delegate void AssistUsed();


	public void InitializeSkillTriggers(){

	}
}