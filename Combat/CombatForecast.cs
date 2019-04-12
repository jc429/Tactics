using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatForecast : MonoBehaviour
{
	public GameObject panel;

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

	public void Hide(){
		panel.SetActive(false);
	}
}
