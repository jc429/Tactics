using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatForecast : MonoBehaviour
{
	public GameObject panel;
	public CombatForecastUnitPanel leftUnitPanel;
	public CombatForecastUnitPanel rightUnitPanel;

    void Awake()
    {
        CombatManager.combatForecast = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Show(){
		panel.SetActive(true);
	}

	public void Show(MapUnit left, MapUnit right){
		SetUnits(left,right);
		panel.SetActive(true);
	}

	public void Hide(){
		panel.SetActive(false);
	}

	public void SetUnits(MapUnit left, MapUnit right){
		leftUnitPanel.SetUnit(left);
		rightUnitPanel.SetUnit(right);
	}
}
