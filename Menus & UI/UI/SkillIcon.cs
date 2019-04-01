using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillIcon : MonoBehaviour{
    [SerializeField]
	Skill currentSkill;

	public SkillInfoPanel skillPanel;

	public void SetSkill(Skill s){
		currentSkill = s;
	}

	public Skill GetSkill(){
		return currentSkill;
	}

	/* sets the skill info panel to describe the current skill */
	public void SetPanelInfo(){
		skillPanel.SetSkillInfo(currentSkill);
	}

}
