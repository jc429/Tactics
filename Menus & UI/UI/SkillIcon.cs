using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour{
    [SerializeField]
	Skill currentSkill;

	public Image iconSprite;

	public SkillInfoPanel skillPanel;

	public void SetSkill(Skill s){
		currentSkill = s;
		if(s != null && s.skillNo >= 0 && s.spriteID > 0){
			//set skill icon 
			iconSprite.sprite = SkillSpriteLibrary.GetSpriteByID(s.spriteID);
			iconSprite.gameObject.SetActive(true);
		}
		else{
			iconSprite.sprite = null;
			iconSprite.gameObject.SetActive(false);
		}
	}
	
	public void SetSkill(int sID){
		Skill s = SkillTable.GetSkill(sID);
		if(s != null){
			SetSkill(s);
		}
	}

	public Skill GetSkill(){
		return currentSkill;
	}

	/* sets the skill info panel to describe the current skill */
	public void SetPanelInfo(){
		skillPanel.SetSkillInfo(currentSkill);
		if(currentSkill.skillNo > 0){
			skillPanel.Open();
		}
		else{
			skillPanel.Close();
		}
	}

}
