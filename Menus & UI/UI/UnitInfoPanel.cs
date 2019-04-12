﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitInfoPanel : MonoBehaviour
{
	HexUnit currentUnit;

	public bool isActive;

	public GameObject panel; 
	public SkillInfoPanel skillInfoPanel; 
	public TextMeshProUGUI unitName;
	public TextMeshProUGUI unitMoveType, unitWeaponType, unitMoveRange, unitWeaponRange;
	public DynamicMeter hpMeter, specialMeter;

	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public TextMeshProUGUI[] statFields;

	public SkillIcon[] skillIcons;

	void Awake(){
		GameController.unitInfoPanel = this;
		ClosePanel();
	}

	public void OpenPanel(HexUnit unit){
		SetUnit(unit);
		SetActive(true);
		//skillInfoPanel.Open();
	}

	public void ClosePanel(){
		Clear();
		SetActive(false);
		skillInfoPanel.Close();
	}
	
	void SetActive(bool active){
		isActive = active;
		panel.SetActive(active);
	}

	public void SetUnit(HexUnit unit){
		currentUnit = unit;
		if(unit == null){
			Clear();
		}
		else{
			unitMoveType.text = unit.Properties.movementClass.ToString();
			unitMoveRange.text = unit.Properties.movementClass.GetRange().ToString();
			unitWeaponType.text = unit.Properties.weaponType.ToString();
			unitWeaponRange.text = unit.Properties.AttackRange.ToString();

			for(int i = 0; i < statFields.Length; i++){
				int displayedStat = unit.Properties.GetStat(i);
				displayedStat = Mathf.Clamp(displayedStat,0,99);
				statFields[i].text = displayedStat.ToString();
			}
			int curHP = unit.CurrentHP;
			int maxHP = unit.MaxHP;
			statFields[0].text =  curHP.ToString() + "/" + maxHP.ToString();
			hpMeter.SetMaxValue(maxHP);
			hpMeter.SetCurrentValue(curHP);

			for(int i = 0; i < 7; i++){
				if(i >= skillIcons.Length){
					break;
				}
				if(skillIcons[i] != null){
					skillIcons[i].SetSkill(unit.Properties.skills[i]);
				}
			}
		}
	}

	public void Clear(){
		unitMoveType.text = "";
		unitMoveRange.text = "";
		unitWeaponType.text = "";
		unitWeaponRange.text = "";
		for(int i = 0; i < statFields.Length; i++){
			statFields[i].text = "";
		}
		hpMeter.SetMaxValue(1);
		hpMeter.SetCurrentValue(1);
		specialMeter.SetMaxValue(1);
		specialMeter.SetCurrentValue(1);
	}
}
