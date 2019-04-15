using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColor : MonoBehaviour
{
	public MeshRenderer _renderer;
	public MeshRenderer _accentRenderer;
	ArmyColorProfile colorProfile = new ArmyColorProfile();

	public void SetColor(Color c){
		_renderer.material.color = c;
	}
	
	public void SetColors(Color main, Color accent){
		_renderer.material.color = main;
		_accentRenderer.material.color = accent;
	}

	public void SetColorProfile(ArmyColorProfile acp){
		if(acp != null){
			colorProfile = acp;
			SetColors(colorProfile.primaryColor, colorProfile.accentColor);
		}
	}

	public void ResetColor(){
		if(colorProfile != null){
			SetColors(colorProfile.primaryColor, colorProfile.accentColor);
		}
	}

	public void SetInactiveColors(){
		if(colorProfile != null){
			SetColors(colorProfile.desaturatedColor, colorProfile.accentColor);
		}
	}


}
