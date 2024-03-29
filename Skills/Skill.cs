﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Skill{
	public int skillNo;
	public string skillIDString;
	public SkillType skillType;
	public string name;
	[TextArea(3,10)]
	public string description;
	public int spriteID;
	//public Sprite sprite;

	public List<ConditionEffectPair> cePairs;
	
	public MapUnit unit;

	public Skill(){
		ClearCEPairs();
	}
	
	/* cleans and initializes CE Pair list if needed */
	public void ClearCEPairs(){
		if(cePairs != null){
			cePairs.Clear();
		}
		else{
			cePairs = new List<ConditionEffectPair>();
		}
	}

	public void AddConditionEffectPair(ConditionEffectPair cePair){
		cePairs.Add(cePair);
		cePair.parentSkill = this;
	}

	public void ResolveConditionEffectPair(ConditionEffectPair cePair){
		cePair.Resolve();
	}

	/* prints the skill out to the debug logger */
	public void LogSkill(){
		string skillInfo = "ID: " + skillIDString + ", Name: " + name;
		Debug.Log(skillInfo);
	}
}
