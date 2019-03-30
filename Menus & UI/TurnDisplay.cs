using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnDisplay : MonoBehaviour
{
	[SerializeField]
	float startX, endX;

	public AnimationCurve curve;

	Vector2 startPos, endPos;

	float speed = 1.25f;
	RectTransform _rTransform;
    // Start is called before the first frame update
    void Awake(){
		GameController.UIElements.turnDisplay = this;
        _rTransform = GetComponent<RectTransform>();
		startPos = endPos = _rTransform.anchoredPosition;
		startPos.x = startX;
		endPos.x = endX;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


	public void StartTurnAnimation(){
		Debug.Log("Hi");
		StopAllCoroutines();
		_rTransform.anchoredPosition = startPos;
		StartCoroutine(TurnAnim());
	}

	IEnumerator TurnAnim(){
		for (float t = speed * Time.deltaTime; t < 1f; t += speed * Time.deltaTime) {
			float curvePercent = curve.Evaluate(t);
			_rTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, curvePercent);
			yield return null;
		}
		_rTransform.anchoredPosition = endPos;
	}

}
