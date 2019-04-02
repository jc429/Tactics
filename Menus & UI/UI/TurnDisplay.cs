﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnDisplay : MonoBehaviour
{
	public RectTransform turnUI;

	[SerializeField]
	float startX, endX;

	public AnimationCurve curve;

	Vector2 startPos, endPos;

	float speed = 1.125f;

	public TMP_Text turnText, phaseText;
	public Image background;
    // Start is called before the first frame update
    void Awake(){
		GameController.UIElements.turnDisplay = this;
		startPos = endPos = turnUI.anchoredPosition;
		startPos.x = startX;
		endPos.x = endX;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


	public void StartTurnAnimation(int turnNo, int armyPhaseNo){
		StopAllCoroutines();
		turnText.text = "Turn " + turnNo;
		phaseText.text = "Army " + armyPhaseNo + " Phase";
		phaseText.outlineColor = turnText.outlineColor = (Color32)Colors.ArmyColors.GetArmyColor(armyPhaseNo);
		phaseText.color = turnText.color = Colors.ArmyColors.GetAccentColor(armyPhaseNo);
		turnUI.anchoredPosition = startPos;
		Color c = background.color;
		c.a = 0;
		background.color = c;
		StartCoroutine(TurnAnim());
	}

	IEnumerator TurnAnim(){
		Color c = background.color;
		for (float t = speed * Time.deltaTime; t < 1f; t += speed * Time.deltaTime) {
			float curvePercent = curve.Evaluate(t);
			turnUI.anchoredPosition = Vector2.Lerp(startPos, endPos, curvePercent);
			c.a = (t < 0.5f) ? curvePercent : 1 - curvePercent;
			background.color = c;
			yield return null;
		}
		turnUI.anchoredPosition = endPos;
		c.a = 0;
		background.color = c;
	}

}