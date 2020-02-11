using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ArmyColorProfile{
	public Color primaryColor;
	public Color accentColor;
	public Color desaturatedColor;
	public Color uiColorMain;
	public Color uiColorAccent;

	public void SaveProfile(BinaryWriter writer){
		Colors.SaveColor(writer, primaryColor);
		Colors.SaveColor(writer, accentColor);
		Colors.SaveColor(writer, desaturatedColor);
		Colors.SaveColor(writer, uiColorMain);
		Colors.SaveColor(writer, uiColorAccent);
	}

	public void LoadProfile(BinaryReader reader){
		//Debug.Log("Loading Profile");
		primaryColor = Colors.LoadColor(reader);
		accentColor = Colors.LoadColor(reader);
		desaturatedColor = Colors.LoadColor(reader);
		uiColorMain = Colors.LoadColor(reader);
		uiColorAccent = Colors.LoadColor(reader);
	}
}
