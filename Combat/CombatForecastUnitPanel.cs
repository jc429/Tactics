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

	MapUnit currentUnit;

    // Start is called before the first frame update
    void Awake(){
		panelTint = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetUnit(MapUnit unit){
		currentUnit = unit;
		unitNameTextbox.text = unit.name;

		hpBar.SetMaxValue(unit.MaxHP);
		hpBar.SetCurrentValue(unit.CurrentHP);
		unitHPPreviewTextbox.text = unit.CurrentHP + " > <b>" + unit.CurrentHP + "</b> / " + unit.MaxHP;
	}
}
