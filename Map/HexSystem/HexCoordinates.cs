using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
	[SerializeField]
	private int x, z;

	public int X{
		get{
			return x;
		}
	}

	public int Y{
		get {
			return -X - Z;
		}
	}

	public int Z{
		get{
			return z;
		}
	}

	public HexCoordinates(int x, int z){
		this.x = x;
		this.z = z;
	}


	public static HexCoordinates FromOffsetCoordinates(int x, int z){
		return new HexCoordinates(x - z / 2, z);
	}

	public static HexCoordinates FromPosition(Vector3 position){
		float x = position.x / (HexMetrics.innerRadius * 2f);
		float y = -x;
			
		//compensate for movement along z axis
		float offset = position.z / (HexMetrics.outerRadius * 3f);
		x -= offset;
		y -= offset;

		//round to integers
		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(-x -y);

		//if they dont add up to zero, something went wrong
		if (iX + iY + iZ != 0) {
			//calculate deltas
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(-x -y - iZ);

			//discard largest delta (aka rounding error) & recalculate component based on the other two
			if (dX > dY && dX > dZ) {
				iX = -iY - iZ;
			}
			else if (dZ > dY) {
				iZ = -iX - iY;
			}
		}

		return new HexCoordinates(iX, iZ);
	}

	public int DistanceTo (HexCoordinates other) {
		return 
			((x < other.x ? other.x - x : x - other.x) +
			(Y < other.Y ? other.Y - Y : Y - other.Y) +
			(z < other.z ? other.z - z : z - other.z)) / 2;
	}



	public override string ToString () {
		return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
	}

	public string ToStringMultiLine () {
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}

	public void Save (BinaryWriter writer) {
		writer.Write(x);
		writer.Write(z);
	}

	public static HexCoordinates Load (BinaryReader reader) {
		HexCoordinates c;
		c.x = reader.ReadInt32();
		c.z = reader.ReadInt32();
		return c;
	}
}
