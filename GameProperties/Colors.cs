using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Colors{
    
	public static Color GetColor(int r, int g, int b, int a = 255){
		return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	
	public static class UIColors{
		public static Color HoverColor = Color.magenta;
		public static Color PathColor = Color.green;
		public static Color StartColor = Color.blue;
		public static Color DestinationColor = Color.red;
		public static Color MoveRangeColor = Colors.GetColor(0,255,255);
		public static Color AttackRangeColor = Colors.GetColor(255,80,0);
		public static Color AssistRangeColor = Colors.GetColor(80,220,0);
	}

	public static class ArmyColors{
		public static Color NullColor = Color.gray;
		public static Color Army1Color = Color.blue;
		public static Color Army2Color = Color.red;

		public static Color GetArmyColor(int army){
			switch(army){
			case 1:
				return Army1Color;
				break;
			case 2:
				return Army2Color;
				break;
			default:
				return NullColor;
				break;
			}
		}
	}
}
