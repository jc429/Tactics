using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillInfoPanel : MonoBehaviour
{
	[SerializeField]
	TMP_Text skillName, skillDescription;

	Skill currentSkill;
   
	public void Open(){
		gameObject.SetActive(true);
	}

	public void Close(){
		Clear();
		gameObject.SetActive(false);
	}

	public void SetSkillInfo(Skill skill){
		if(skill == null){
			Clear();
			return;
		}
		currentSkill = skill;
		skillName.text = skill.name;
		skillDescription.text = skill.description;
	}

	public void Clear(){
		currentSkill = null;
		skillName.text = "";
		skillDescription.text = "";
	}
}
