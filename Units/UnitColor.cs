using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColor : MonoBehaviour
{
	public MeshRenderer _renderer;
	public MeshRenderer _accentRenderer;

	public void SetColor(Color c){
		_renderer.material.color = c;
	}
	
	public void SetColors(Color main, Color accent){
		_renderer.material.color = main;
		_accentRenderer.material.color = accent;
	}
}
