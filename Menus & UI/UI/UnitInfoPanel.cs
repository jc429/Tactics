using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitInfoPanel : MonoBehaviour
{
	HexUnit currentUnit;

	public bool isActive;

	[SerializeField]
	GameObject panel; 
	[SerializeField]
	TextMeshProUGUI unitName;
	public
	TextMeshProUGUI unitMoveType, unitWeaponType, unitMoveRange, unitWeaponRange;
	[SerializeField]
	DynamicMeter hpMeter, specialMeter;

	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public TextMeshProUGUI[] statFields;

	void Awake(){
		GameController.unitInfoPanel = this;
		ClosePanel();
	}

	public void OpenPanel(HexUnit unit){
		SetUnit(unit);
		SetActive(true);
	}

	public void ClosePanel(){
		Clear();
		SetActive(false);
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
				statFields[i].text = unit.Properties.GetStat(i).ToString();
			}
			int curHP = unit.currentHP;
			int maxHP = unit.Properties.GetStat(CombatStat.HP);
			statFields[0].text =  curHP.ToString() + "/" + maxHP.ToString();
			hpMeter.SetMaxValue(maxHP);
			hpMeter.SetCurrentValue(curHP);
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
