using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
	HexUnit currentUnit;

	public bool isActive;

	[SerializeField]
	GameObject panel; 
	[SerializeField]
	Text unitName;
	[SerializeField]
	Text unitMoveType, unitWeaponType;

	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public Text[] statFields;

	void Awake(){
		GameController.unitUI = this;
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
			unitWeaponType.text = unit.Properties.weaponType.ToString();
			for(int i = 0; i < statFields.Length; i++){
				statFields[i].text = unit.Properties.GetStat(i).ToString();
			}
		}
	}

	public void Clear(){
		unitMoveType.text = "";
		unitWeaponType.text = "";
		for(int i = 0; i < statFields.Length; i++){
			statFields[i].text = "";
		}
	}
}
