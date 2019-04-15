using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Colors{
    
	public static Color GetColor(int r, int g, int b, int a = 255){
		return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	public static void SaveColor(BinaryWriter writer, Color c){
		writer.Write(c.r);
		writer.Write(c.g);
		writer.Write(c.b);
		writer.Write(c.a);
	}

	public static Color LoadColor(BinaryReader reader){
		Color c;
		c.r = reader.ReadSingle();
		c.g = reader.ReadSingle();
		c.b = reader.ReadSingle();
		c.a = reader.ReadSingle();
		return c;
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


}
