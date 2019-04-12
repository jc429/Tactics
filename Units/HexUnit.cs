using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HexUnit : MonoBehaviour {
	UnitAnimator _unitAnimator;
	UnitColor _unitColor;

	public static HexUnit unitPrefab;

	public UnitProperties Properties;
	
	int currentHP;
	public int CurrentHP{
		get{
			return currentHP;
		}
	}
	public int MaxHP{
		get{
			return Properties.GetStat(CombatStat.HP);
		}
	}
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

	// hex direction unit is facing
	DodecDirection facing;
	public DodecDirection Facing {
		get {	return facing;	}
		set {
			facing = value;
			_unitAnimator.Orientation = value.DegreesOfRotation();
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
	List<HexCell> pathToTravel;


	public HexUnit(){
		Properties = new UnitProperties();
	}
	
	void Awake(){
		_unitColor = GetComponent<UnitColor>();
		_unitAnimator = GetComponent<UnitAnimator>();
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
	public void EndAction(){
		turnState = TurnState.Finished;
		DeselectUnit();
		_unitColor.SetColor(Colors.ArmyColors.GetArmyColor(0));
		if(GameProperties.DEBUG_INFINITE_ACTIONS){
			StartTurn();
		}
		else{
			TurnManager.CheckPhase();
		}
	}
	
	public bool IsFinished(){
		return turnState == TurnState.Finished;
	}

	/* handles cleanup when the entire army's turn is over */
	public void EndTurn(){
		turnState = TurnState.Finished;
		_unitColor.SetColor(Colors.ArmyColors.GetArmyColor(Properties.affiliation));
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
		_unitColor.SetColors(Colors.ArmyColors.GetArmyColor(aff),Colors.ArmyColors.GetAccentColor(aff));
		
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
		yield return _unitAnimator.TurnToLookAt(pathToTravel[1].Position);

		float t = Time.deltaTime * travelSpeed;
		for (int i = 1; i < pathToTravel.Count; i++) {
			a = c;
			b = pathToTravel[i - 1].Position;
			c = (b + pathToTravel[i].Position) * 0.5f;
			for (; t < 1f; t += Time.deltaTime * travelSpeed) {
				transform.localPosition = Bezier.GetPointUnclamped(a, b, c, t);
				Vector3 d = Bezier.GetDerivative(a, b, c, t);
				d.y = 0f;
				_unitAnimator.SetRotation(Quaternion.LookRotation(d));
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
			_unitAnimator.SetRotation(Quaternion.LookRotation(d));
			yield return null;
		}

		transform.localPosition = location.Position;
		
		// set facing
		Facing = DodecDirectionExtensions.DodecDirectionFromDegrees(Mathf.RoundToInt(_unitAnimator.Orientation));

		isTraveling = false;
		//moveTiles.Clear();
		ListPool<HexCell>.Add(pathToTravel);
		pathToTravel = null;
		FinishTravel();
	}

	/* rotates unit to face a given cell */
	public void  FaceCell (HexCell cell) {
		StartCoroutine(_unitAnimator.TurnToLookAt(cell.Position));
	}

	/* called after unit moves to destination */
	void FinishTravel(){
		turnState = TurnState.PostMove;
		GameController.hexGrid.CalculateLocalAttackRange(Location,this);
		GameController.gameUI.OpenUnitActionMenu();
	}


	

	/* damage received/healed during combat -- returns true if unit dies */
	public bool SetCurrentHP(int hp){
		currentHP = Mathf.Clamp(hp, 0, MaxHP);
		hpBar.SetCurrentValue(currentHP);
		if(currentHP <= 0){
			Die();
			return true;
		}
		return false;
	}

	public void ResetHP(){
		currentHP = Properties.GetStat(CombatStat.HP);
		hpBar.SetCurrentValue(currentHP);
	}

	/* damage received during combat -- returns true if unit dies */
	public bool TakeDamage(int dmg){
		currentHP = Mathf.Clamp(currentHP - dmg, 0, MaxHP);
		hpBar.SetCurrentValue(currentHP);
		if (currentHP <= 0){
			currentHP = 0;
			Die();
			return true;
		}
		return false;
	}

	/* damage received out of combat -- will never drop hp below 1 */
	public void TakeChipDamage(int dmg){
		currentHP = Mathf.Clamp(currentHP - dmg, 1, MaxHP);
		hpBar.SetCurrentValue(currentHP);
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