using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicMeter : MonoBehaviour
{
	RectTransform _rTransform;
	public Image meterFill;


	float maxValue;
	float currentValue;

	void Start(){
		_rTransform = GetComponent<RectTransform>();
	}


	public void SetMaxValue(float max){
		maxValue = max;		
		UpdateMeter();
	}

	public void SetCurrentValue(float value){
		if(currentValue == value){
			return;
		}
		currentValue = value;
		UpdateMeter();
	}

	void UpdateMeter(){
		float percentage;
		if(maxValue != 0){
			percentage = currentValue / maxValue;	
		}
		else{
			percentage = 1;
		}
		percentage = Mathf.Clamp(percentage,0,1);
		meterFill.fillAmount = percentage;
	}


}
