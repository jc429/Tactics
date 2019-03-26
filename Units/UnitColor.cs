using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColor : MonoBehaviour
{
    [SerializeField]
	MeshRenderer _renderer;

	public void SetColor(Color c){
		_renderer.material.color = c;
	}
}
