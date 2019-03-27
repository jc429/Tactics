using UnityEngine;
using UnityEngine.EventSystems;

public class GameUI : MonoBehaviour {
	
	public HexGrid grid;

	public BasicMenu unitActionMenu;

	//cell the mouse cursor is over
	HexCell currentCell;

	HexUnit selectedUnit;
	//hex cell the unit started their turn in
	HexCell startCell;
	DodecDirection startFacing;

	void Awake(){
		GameController.gameUI = this;
	}

	void Start(){
		unitActionMenu.CloseMenu();
	}

	void Update () {
		grid.StartMap();
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if(selectedUnit != null){
				switch(selectedUnit.turnState){
				case TurnState.PreMove:
					if (Input.GetMouseButtonDown(0)) {
						DoMove();
					}
					else if(Input.GetMouseButtonDown(1)){
						grid.ClearPath();
						selectedUnit.turnState = TurnState.Idle;
						DeselectUnit();
					}
					else{
						DoPathfinding();
					}
					break;
				case TurnState.PostMove:
					if(Input.GetMouseButtonDown(1)){
						CancelUnitMovement();
						selectedUnit.turnState = TurnState.PreMove;
					}
					break;
				case TurnState.PreAttack:
					if (Input.GetMouseButtonDown(0)) {
						DoSelectTarget();
					}
					else if(Input.GetMouseButtonDown(1)){
						CloseUnitAttackRange();
						OpenUnitActionMenu();
					}
					else{
						UpdateCurrentCell();
					}
					break;
				case TurnState.Finished:
					if (Input.GetMouseButtonDown(0)) {
						DoSelectUnit();
					}
					else{
						UpdateCurrentCell();
					}
					break;
				default:
					UpdateCurrentCell();
					break;
				}
			}
			else{
				if (Input.GetMouseButtonDown(0)) {
					DoSelectUnit();
				}
				else{
					UpdateCurrentCell();				
				}
			}

			
		}
	}

	public void SetEditMode (bool toggle) {
		enabled = !toggle;
		grid.ShowUI(!toggle);
		grid.ClearPath();
	}

	bool UpdateCurrentCell () {
		HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell) {
			if(currentCell != null){
				currentCell.colorFlags.IsHoveredOn = false;
			}
			currentCell = cell;
			if(currentCell != null){
				currentCell.colorFlags.IsHoveredOn = true;
			}
			return true;
		}
		return false;
	}

	void DoSelectUnit () {
		grid.ClearPath();
		if(selectedUnit != null){
			if(selectedUnit.isTraveling){
				return;
			}
			selectedUnit.DeselectUnit();
		}
		if(currentCell != null){
			currentCell.colorFlags.IsSelected = false;
		}
		UpdateCurrentCell();
		if (currentCell) {
			HexUnit unit = currentCell.Unit;
			if(unit != null){
				if(unit.isTraveling){
					return;
				}
				selectedUnit = unit;
				startCell = unit.Location;
				startFacing = unit.Facing;
				grid.CalculateMovementRange(currentCell,selectedUnit);
				grid.CalculateTotalAttackRange(selectedUnit);
				selectedUnit.SelectUnit();
				GameController.unitInfoPanel.OpenPanel(selectedUnit);
				//currentCell.IsSelected = true;
			}
		}
	}

	public void DeselectUnit(){
		if(selectedUnit){
			selectedUnit.DeselectUnit();
		}
		unitActionMenu.CloseMenu();
		GameController.unitInfoPanel.ClosePanel();
	
		selectedUnit = null;
	}

	void ReturnUnitToStart(){
		if(selectedUnit){
			selectedUnit.Location = startCell;
			selectedUnit.Facing = startFacing;
			selectedUnit.turnState = TurnState.PreMove;
		}
	}
	
	/* calculate the path the selected unit would take to reach the tile the cursor is on */
	void DoPathfinding () {
		if (UpdateCurrentCell()) {
			if (currentCell /*&& selectedUnit.IsValidDestination(currentCell)*/) {
				grid.FindPath(selectedUnit.Location, currentCell, selectedUnit);
			}
			else {
				grid.ClearPath();
			}
		}
	}

	void DoMove () {
		if (grid.HasPath && !selectedUnit.isTraveling) {
			GameController.unitInfoPanel.ClosePanel();
			selectedUnit.Travel(grid.GetPath());
			grid.ClearPath();
			selectedUnit.HideDisplays();

		}
	}

	public void OpenUnitActionMenu(){
		unitActionMenu.OpenMenu();
	}

	public void OpenUnitAttackRange(){
		if(selectedUnit != null && selectedUnit.localAttackTiles != null){
			selectedUnit.MarkLocalAttackRange(true);
			selectedUnit.turnState = TurnState.PreAttack;
		}
		unitActionMenu.CloseMenu();
	}

	void CloseUnitAttackRange(){
		if(selectedUnit != null && selectedUnit.localAttackTiles != null){
			selectedUnit.MarkLocalAttackRange(false);
			selectedUnit.turnState = TurnState.PostMove;
		}
		unitActionMenu.OpenMenu();
	}

	void DoSelectTarget(){
		if(selectedUnit != null && selectedUnit.localAttackTiles != null){
			if(selectedUnit.localAttackTiles.Contains(currentCell)){
				if(currentCell.Unit != null && currentCell.Unit != selectedUnit){
					//do combat
					
					StartCoroutine(selectedUnit.TurnToLookAt(currentCell.Position));
					CombatManager.StartCombat(selectedUnit,currentCell.Unit,1);
					
				}
				EndSelectedUnitTurn();
			}
		}
	}

	public void EndSelectedUnitTurn(){
		if(selectedUnit == null){
			return;
		}
		selectedUnit.FinishAction();
		DeselectUnit();
		unitActionMenu.CloseMenu();
	}

	public void CancelUnitMovement(){
		if(selectedUnit == null){
			return;
		}
		ReturnUnitToStart();
		unitActionMenu.CloseMenu();
		GameController.unitInfoPanel.OpenPanel(selectedUnit);
		selectedUnit.MarkMovementRange(true);
		selectedUnit.MarkAttackRange(true);
	}
}