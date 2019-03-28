﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HexUnit : MonoBehaviour {

	UnitColor _unitColor;

	public static HexUnit unitPrefab;

	public UnitProperties Properties;
	
	public int currentHP;
	public DynamicMeter hpBar;
	public bool isDead;

	//what this unit is currently doing
	public TurnState turnState;

	// cell unit is positioned in
	HexCell location;
	public HexCell Location {
		get {	return location;	}
		set {
			if (location) {
				location.Unit = null;
			}
			location = value;
			value.Unit = this;
			transform.localPosition = value.Position;
		}
	}

	// direction unit model is facing
	float orientation;
	float Orientation {
		get {	return orientation;	}
		set {
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	// hex direction unit is facing
	DodecDirection facing;
	public DodecDirection Facing {
		get {	return facing;	}
		set {
			facing = value;
			Orientation = value.DegreesOfRotation();
		}
	}

	// how many tiles unit can move (before factoring in cost)
	//public int moveRange = 7;
	//tiles unit can move to
	public List<HexCell> moveTiles;
	//tiles unit can attack in a given turn
	public List<HexCell> attackTiles;
	//tiles unit can attack from where they are standing
	public List<HexCell> localAttackTiles;

	// travel
	public bool isTraveling;
	const float travelSpeed = 4f;
	const float rotationSpeed = 360f;
	List<HexCell> pathToTravel;


	public HexUnit(){
		Properties = new UnitProperties();
	}
	
	void Awake(){
		_unitColor = GetComponent<UnitColor>();
	}

	void Start(){
		hpBar.SetMaxValue(Properties.GetStat(CombatStat.HP));
	}

	void OnEnable () {
		if (location) {
			transform.localPosition = location.Position;
		}
	}

	void LateUpdate(){
		if(isDead){
			Destroy(gameObject);
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
		Properties.RandomizeStats();
		ResetHP();
		GameController.hexGrid.CalculateMovementRange(location, this);
		GameController.hexGrid.CalculateTotalAttackRange(this);
	}

	/* handles prepping the unit during the start of their turn */
	public void StartTurn(){
		turnState = TurnState.Idle;
		_unitColor.SetColor(Colors.ArmyColors.GetArmyColor(Properties.affiliation));
	}

	/* called after unit does combat or other actions */
	public void EndTurn(){
		turnState = TurnState.Finished;
		DeselectUnit();
		_unitColor.SetColor(Colors.ArmyColors.GetArmyColor(0));
		if(GameProperties.DEBUG_INFINITE_ACTIONS){
			StartTurn();
		}
	}

	public bool IsFinished(){
		return turnState == TurnState.Finished;
	}

	/* selection  */
	public void SelectUnit(){
		if(turnState == TurnState.Idle){
			//if for some reason the unit doesnt have move/attack tiles calculated yet (but they always should)
			if(moveTiles != null && moveTiles.Count == 0){
				GameController.hexGrid.CalculateMovementRange(location,this);
			}
			if(attackTiles != null && attackTiles.Count == 0){
				GameController.hexGrid.CalculateTotalAttackRange(this);
			}
			
			MarkMovementRange(true);
			MarkAttackRange(true);
			turnState = TurnState.PreMove;
		}
	}

	public void DeselectUnit(){
		HideDisplays();
	}

	/* hide all movement and attack displays */
	public void HideDisplays(){
		MarkMovementRange(false);
		MarkAttackRange(false);
		MarkLocalAttackRange(false);
	}

	/* set which army unit is a member of */
	public void SetAffiliation(int aff){
		ArmyManager.RemoveUnitFromArmy(this, Properties.affiliation);
		ArmyManager.AssignUnitToArmy(this, aff);
		Properties.affiliation = aff;
		_unitColor.SetColor(Colors.ArmyColors.GetArmyColor(aff));
		
	}

	/* returns how far unit can move (currently entirely based on unit's class) */
	public int MovementRange(){
		return GameProperties.MovementProperties.ClassBaseMovement[(int)Properties.movementClass];
	}

	/* marks all tiles within movement range as either true or false */
	public void MarkMovementRange(bool b){
		if(moveTiles != null){
			foreach(HexCell c in moveTiles){
				c.colorFlags.InMovementRange = b;
			}
		}
	}

	/* marks all tiles within attack range as either true or false */
	public void MarkAttackRange(bool b){
		if(attackTiles != null){
			foreach(HexCell c in attackTiles){
				c.colorFlags.InAttackRange = b;
			}
		}
	}
	
	/* marks all tiles within attack range as either true or false */
	public void MarkLocalAttackRange(bool b){
		if(localAttackTiles != null){
			foreach(HexCell c in localAttackTiles){
				c.colorFlags.InAttackRange = b;
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
		turnState = TurnState.Moving;
		StartCoroutine(TravelPath());

	}

	IEnumerator TravelPath () {
		
		Vector3 a, b, c = pathToTravel[0].Position;
		transform.localPosition = c;
		yield return TurnToLookAt(pathToTravel[1].Position);

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
		Facing = DodecDirectionExtensions.DodecDirectionFromDegrees(Mathf.RoundToInt(orientation));

		isTraveling = false;
		//moveTiles.Clear();
		ListPool<HexCell>.Add(pathToTravel);
		pathToTravel = null;
		FinishTravel();
	}


	public IEnumerator TurnToLookAt (Vector3 point) {
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
		Facing = DodecDirectionExtensions.DodecDirectionFromDegrees(Mathf.RoundToInt(orientation));
	}

	/* called after unit moves to destination */
	void FinishTravel(){
		turnState = TurnState.PostMove;
		GameController.hexGrid.CalculateLocalAttackRange(Location,this);
		GameController.gameUI.OpenUnitActionMenu();
	}


	public void ResetHP(){
		currentHP = Properties.GetStat(CombatStat.HP);
		hpBar.SetCurrentValue(currentHP);
	}

	/* damage received during combat -- returns true if unit dies */
	public bool TakeDamage(int dmg){
		currentHP -= dmg;
		hpBar.SetCurrentValue(currentHP);
		if (currentHP <= 0){
			currentHP = 0;
			Die();
			return true;
		}
		return false;
	}

	/* preps unit for death */
	public void Die () {
		isDead = true;
		location.Unit = null;
	}

	/* save unit to file */
	public void Save (BinaryWriter writer) {
		location.coordinates.Save(writer);
		writer.Write((int)facing);
		Properties.Save(writer);
	}

	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		DodecDirection facing = (DodecDirection)reader.ReadInt32();
		HexUnit unit = Instantiate(unitPrefab);
		unit.Properties.Load(reader);
		unit.SetAffiliation(unit.Properties.affiliation);
		grid.AddUnit(unit, grid.GetCell(coordinates), facing);

	}
}