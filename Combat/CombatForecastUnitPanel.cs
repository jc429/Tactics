using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatForecastUnitPanel : MonoBehaviour
{
	Image panelTint;
	public Image unitIcon;
	public TextMeshProUGUI unitNameTextbox;
	public TextMeshProUGUI hitDmgTextbox;
	public TextMeshProUGUI followupTextbox;
	public TextMeshProUGUI unitHPPreviewTextbox;
	public DynamicMeter hpBar;

	HexUnit currentUnit;

    // Start is called before the first frame update
    void Awake(){
		panelTint = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetUnit(HexUnit unit){
		currentUnit = unit;
		hpBar.SetMaxValue(unit.MaxHP);
		hpBar.SetCurrentValue(unit.CurrentHP);
		
	}
}
