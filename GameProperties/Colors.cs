using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Colors{
    
	public static Color GetColor(int r, int g, int b, int a = 255){
		return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

}
