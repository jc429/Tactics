using System.IO;
using UnityEngine;

public class HexUnit : MonoBehaviour {

	public static HexUnit unitPrefab;

	// cell unit is positioned in
	HexCell location;

	// direction unit model is facing
	float orientation;

	// hex direction unit is facing
	HexDirection facing;

	public HexCell Location {
		get {
			return location;
		}
		set {
			if (location) {
				location.Unit = null;
			}
			location = value;
			value.Unit = this;
			transform.localPosition = value.Position;
		}
	}

	float Orientation {
		get {
			return orientation;
		}
		set {
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	/* hex direction unit is facing */
	public HexDirection Facing {
		get {
			return facing;
		}
		set {
			facing = value;
			Orientation = value.DegreesOfRotation();
		}
	}

	/* refresh unit's position to be standing on cell */
	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	/* true if unit can reach a tile */
	public bool IsValidDestination (HexCell cell) {
		return !cell.IsUnderwater && !cell.Unit;
	}

	/* unit cleanup */
	public void Die () {
		location.Unit = null;
		Destroy(gameObject);
	}

	/* save unit to file */
	public void Save (BinaryWriter writer) {
		location.coordinates.Save(writer);
		writer.Write((int)facing);
	}

	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		HexDirection facing = (HexDirection)reader.ReadInt32();
		grid.AddUnit(Instantiate(unitPrefab), grid.GetCell(coordinates), facing);
	}
}