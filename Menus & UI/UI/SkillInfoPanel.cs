using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillInfoPanel : InfoPanel
{
	InfoPanel keywordPanel;

	Skill currentSkill;

	int currentLink = -1;

	Canvas canvas;

	void Start(){
		keywordPanel = GameController.instance.keywordInfoPanel;
		canvas = GetComponentInParent<Canvas>();
	}

	void LateUpdate(){
		CheckLinks();
	}

	void CheckLinks(){
		//if mouse is within text rect
		if (TMP_TextUtilities.IsIntersectingRectTransform(descriptionText.rectTransform, Input.mousePosition, null)){
		
			// Check if mouse intersects with any links.
			int linkIndex = TMP_TextUtilities.FindIntersectingLink(descriptionText, Input.mousePosition, null);
			// Handle new Link selection.
			if (linkIndex != currentLink){
				currentLink = linkIndex;
				// index of -1 means no link
				if(linkIndex == -1){
					//close keyword panel
					keywordPanel.Close();
				}
				else{
					// Get information about the link.
					TMP_LinkInfo linkInfo = descriptionText.textInfo.linkInfo[linkIndex];

					// Send the event to any listeners. 
					//Debug.Log(linkInfo.GetLinkID() + "," + linkInfo.GetLinkText() + "," + linkIndex);
					KeywordInfo keyword = KeywordTable.GetKeywordInfo(linkInfo.GetLinkText());
					keywordPanel.SetText(keyword.name,keyword.description);

					Vector2 mousePos;
					if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
					canvas.GetComponent<RectTransform>(),Input.mousePosition,null, out mousePos)){
						//Debug.Log(mousePos);
						mousePos += (0.5f * canvas.GetComponent<RectTransform>().sizeDelta);
						Vector2 offset = new Vector2(5,5);
						keywordPanel.SetPosition(mousePos + offset);
					}
					
					keywordPanel.Open();
				}
			}

		}
	}
   

	public void SetSkillInfo(Skill skill){
		if(skill == null){
			Clear();
			return;
		}
		currentSkill = skill;
		SetText(skill.name, skill.description);
	}

	public override void Clear(){
		currentSkill = null;
		nameText.text = "";
		descriptionText.text = "";
	}


}
