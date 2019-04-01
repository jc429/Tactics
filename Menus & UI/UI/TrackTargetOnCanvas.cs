using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackTargetOnCanvas : MonoBehaviour
{
	RectTransform _rTransform;
	public RectTransform canvas;

	public Transform target;
	public Vector2 targetOffset;

    void Awake(){
		_rTransform = GetComponent<RectTransform>();
		canvas = GameController.instance.hpBarCanvasParent;
		_rTransform.SetParent(canvas);
		_rTransform.localPosition = Vector2.zero;
		_rTransform.localScale = Vector3.one;
		_rTransform.localRotation = Quaternion.identity;
    }

	void Update(){
		TrackTarget();
	}

	
	void TrackTarget(){
		if(target == null){
			Destroy(this.gameObject);
			return;
		}
		Vector2 screenPosition = Camera.main.WorldToViewportPoint(target.position);
		Vector2 canvasPosition = new Vector2(
		((screenPosition.x * canvas.sizeDelta.x) - (canvas.sizeDelta.x * 0.5f)),
        ((screenPosition.y * canvas.sizeDelta.y) - (canvas.sizeDelta.y * 0.5f)));
		canvasPosition += targetOffset;
		GetComponent<RectTransform>().anchoredPosition = canvasPosition;
	}
}
