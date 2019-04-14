using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UnitSkillEventHandler{
	public HexUnit unit;

	public delegate bool resolveSkill();
	
	public List<ConditionEffectPair> onTurnStart;
	public List<ConditionEffectPair> onTurnEnd;
	public List<ConditionEffectPair> onCombatStart;
	public List<ConditionEffectPair> onCombatEnd;
	public List<ConditionEffectPair> onTakeDamage;
	public List<ConditionEffectPair> onSpecialActivate;
	public List<ConditionEffectPair> onAssistUsed;

	public UnitSkillEventHandler(){
		onTurnStart = new List<ConditionEffectPair>();
		onTurnEnd = new List<ConditionEffectPair>();
		onCombatStart = new List<ConditionEffectPair>();
		onCombatEnd = new List<ConditionEffectPair>();
		onTakeDamage = new List<ConditionEffectPair>();
		onSpecialActivate = new List<ConditionEffectPair>();
		onAssistUsed = new List<ConditionEffectPair>();
	}


	public void OnTurnStart(){
		foreach(ConditionEffectPair cePair in onTurnStart){
			cePair.Resolve();
		}
	}
	public void OnTurnEnd(){
		foreach(ConditionEffectPair cePair in onTurnEnd){
			cePair.Resolve();
		}
	}
	public void OnCombatStart(){
		foreach(ConditionEffectPair cePair in onCombatStart){
			cePair.Resolve();
		}
	}
	public void OnCombatEnd(){
		foreach(ConditionEffectPair cePair in onCombatEnd){
			cePair.Resolve();
		}
	}
	public void OnTakeDamage(){
		foreach(ConditionEffectPair cePair in onTakeDamage){
			cePair.Resolve();
		}
	}
	public void OnSpecialActivate(){
		foreach(ConditionEffectPair cePair in onSpecialActivate){
			cePair.Resolve();
		}
	}
	public void OnAssistUsed(){
		foreach(ConditionEffectPair cePair in onAssistUsed){
			cePair.Resolve();
		}
	}

}

