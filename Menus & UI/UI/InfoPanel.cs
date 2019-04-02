using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InfoPanel : MonoBehaviour
{
	[SerializeField]
	protected TMP_Text nameText, descriptionText;

	RectTransform _rTransform;

	protected void Awake(){
		_rTransform = GetComponent<RectTransform>();
	}

	public void SetPosition(Vector2 pos){
		if(_rTransform == null){
			_rTransform = GetComponent<RectTransform>();
		}
		_rTransform.anchoredPosition = pos;
	}
	
	/* opens the panel */
	public void Open(){
		gameObject.SetActive(true);
	}

	/* clears and closes the info panel */
	public void Close(){
		Clear();
		gameObject.SetActive(false);
	}

	/* clears out the textboxes */
	public virtual void Clear(){
		nameText.text = "";
		descriptionText.text = "";
	}

	public void SetNameText(string name){
		nameText.text = name;
	}

	public void SetDescriptionText(string description){
		descriptionText.text = description;
	}

	/* sets both name and description text at once */
	public void SetText(string name, string description){
		nameText.text = name;
		descriptionText.text = description;
	}
}
