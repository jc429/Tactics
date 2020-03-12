using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class GameSettings
{
	[SerializeField]
	public static readonly bool DEBUG_MODE = true;


	public static readonly CursorInputType cursorInputType = CursorInputType.DPad; 
}
