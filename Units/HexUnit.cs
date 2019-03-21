using System.Collections;
using System.Collections.Generic;
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

	// how many tiles unit can move (before factoring in cost)
	//public int moveRange = 7;
	//tiles unit can move to
	public List<HexCell> moveTiles;
	//tiles unit can attack
	public List<HexCell> attackTiles;

	public MovementClass movementClass;

	// travel
	bool isTraveling;
	const float travelSpeed = 4f;
	const float rotationSpeed = 360f;
	List<HexCell> pathToTravel;



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

	


	void OnEnable () {
		if (location) {
			transform.localPosition = location.Position;
			
		}

	}


	/*
	void OnDrawGizmos () {
		if (pathToTravel == null || pathToTravel.Count == 0) {
			return;
		}

		Vector3 a, b, c = pathToTravel[0].Position;
		for (int i = 1; i < pathToTravel.Count; i++) {
			a = c;
			b = pathToTravel[i - 1].Position;
			c = (b + pathToTravel[i].Position) * 0.5f;
			for (float t = 0f; t < 1f; t += 0.1f) {
				Gizmos.DrawSphere(Bezier.GetPointUnclamped(a, b, c, t), 0.2f);
			}
		}
		a = c;
		b = pathToTravel[pathToTravel.Count - 1].Position;
		c = b;
		for (float t = 0f; t < 1f; t += 0.1f) {
			Gizmos.DrawSphere(Bezier.GetPointUnclamped(a, b, c, t), 0.2f);
		}
	}
	*/

	public void StartUnit(){
		GameController.hexGrid.CalculateMovementRange(location, this);
		GameController.hexGrid.CaluclateTotalAttackRange(this);
		Debug.Log(attackTiles.Count);
		//FIXME: for some reason a unit will have 0 move tiles and 0 attack tiles until they move once
	}


	/* selection  */
	public void SelectUnit(){
		
		MarkMovementRange(true);
		MarkAttackRange(true);
	}

	public void DeselectUnit(){
		MarkMovementRange(false);
		MarkAttackRange(false);
	}

	/* marks all tiles within movement range as either true or false */
	void MarkMovementRange(bool b){
		if(moveTiles != null){
			foreach(HexCell c in moveTiles){
				c.InMovementRange = b;
			}
		}
	}

	/* marks all tiles within attack range as either true or false */
	void MarkAttackRange(bool b){
		if(attackTiles != null){
			foreach(HexCell c in attackTiles){
				c.InAttackRange = b;
			}
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

	/* travel along a path */
	public void Travel (List<HexCell> path) {
		// abort if already in transit
		if(isTraveling){
			return;	
		}
		Location = path[path.Count - 1];
		pathToTravel = path;
		if(pathToTravel.Count <= 1){
			pathToTravel = null;
			return;
		}
		StopAllCoroutines();
		isTraveling = true;
		StartCoroutine(TravelPath());

	}

	IEnumerator TravelPath () {
		
		Vector3 a, b, c = pathToTravel[0].Position;
		transform.localPosition = c;
		yield return LookAt(pathToTravel[1].Position);

		float t = Time.deltaTime * travelSpeed;
		for (int i = 1; i < pathToTravel.Count; i++) {
			a = c;
			b = pathToTravel[i - 1].Position;
			c = (b + pathToTravel[i].Position) * 0.5f;
			for (; t < 1f; t += Time.deltaTime * travelSpeed) {
				transform.localPosition = Bezier.GetPointUnclamped(a, b, c, t);
				Vector3 d = Bezier.GetDerivative(a, b, c, t);
				d.y = 0f;
				transform.localRotation = Quaternion.LookRotation(d);
				yield return null;
			}
			t -= 1f;
		}

		a = c;
		b = pathToTravel[pathToTravel.Count - 1].Position;
		c = b;
		for (; t < 1f; t += Time.deltaTime * travelSpeed) {
			transform.localPosition = Bezier.GetPointUnclamped(a, b, c, t);
			Vector3 d = Bezier.GetDerivative(a, b, c, t);
			d.y = 0f;
			transform.localRotation = Quaternion.LookRotation(d);
			yield return null;
		}

		transform.localPosition = location.Position;
		orientation = transform.localRotation.eulerAngles.y;
		// set facing
		Facing = HexDirectionExtensions.HexDirectionFromDegrees(Mathf.RoundToInt(orientation));

		isTraveling = false;
		ListPool<HexCell>.Add(moveTiles);
		moveTiles = null;
		ListPool<HexCell>.Add(pathToTravel);
		pathToTravel = null;
	}


	IEnumerator LookAt (Vector3 point) {
		point.y = transform.localPosition.y;
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
		float angle = Quaternion.Angle(fromRotation, toRotation);
		float speed = rotationSpeed / angle;
		
		if( angle > 0){
			for (float t = speed * Time.deltaTime; t < 1f; t += speed * Time.deltaTime) {
				transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
				yield return null;
			}
		}

		transform.LookAt(point);
		orientation = transform.localRotation.eulerAngles.y;
		Facing = HexDirectionExtensions.HexDirectionFromDegrees(Mathf.RoundToInt(orientation));
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
		HexUnit unit = Instantiate(unitPrefab);
		unit movementClass = movementClass.Infantry;
		grid.AddUnit(unit, grid.GetCell(coordinates), facing);
	}
}