using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public struct MapCoordinates {
	[SerializeField]
	private int x, y;

	public int X{
		get{
			return x;
		}
	}

	public int Y{
		get {
			return y;
		}
	}

	public MapCoordinates(int x, int y){
		this.x = x;
		this.y = y;
	}

	public MapCoordinates(Vector2 v){
		this.x = (int)v.x;
		this.y = (int)v.y;
	}

	public int DistanceTo (MapCoordinates other) {
		return 
			((x < other.x ? other.x - x : x - other.x) +
			(Y < other.Y ? other.Y - Y : Y - other.Y));
	}

	public override string ToString () {
		return "(" + X.ToString() + ", " + Y.ToString() + ")";
	}

	public void Save (BinaryWriter writer) {
		writer.Write(x);
		writer.Write(y);
	}

	public static MapCoordinates Load (BinaryReader reader) {
		MapCoordinates c;
		c.x = reader.ReadInt32();
		c.y = reader.ReadInt32();
		return c;
	}
}