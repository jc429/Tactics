using UnityEngine;
using UnityEngine.EventSystems;

public class GameUI : MonoBehaviour {

	public HexGrid grid;

	//cell the mouse cursor is over
	HexCell currentCell;

	HexUnit selectedUnit;

	void Start(){
	}

	void Update () {
		grid.StartMap();
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if(selectedUnit != null){
				if(selectedUnit.turnState == TurnState.PreMove){
					if (Input.GetMouseButtonDown(0)) {
						DoMove();
					}
					else{
						DoPathfinding();
					}
				}
				else if(selectedUnit.turnState == TurnState.PostMove){
					if (Input.GetMouseButtonDown(0)) {
						DoSelectTarget();
					}
					else{
						UpdateCurrentCell();
					}
				}
				else if(selectedUnit.turnState == TurnState.Finished){
					if (Input.GetMouseButtonDown(0)) {
						DoSelectUnit();
					}
					else{
						UpdateCurrentCell();
					}
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
				currentCell.IsHoveredOn = false;
			}
			currentCell = cell;
			if(currentCell != null){
				currentCell.IsHoveredOn = true;
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
			currentCell.IsSelected = false;
		}
		UpdateCurrentCell();
		if (currentCell) {
			HexUnit unit = currentCell.Unit;
			if(unit != null){
				if(unit.isTraveling){
					return;
				}
				selectedUnit = unit;
				grid.CalculateMovementRange(currentCell,selectedUnit);
				grid.CalculateTotalAttackRange(selectedUnit);
				selectedUnit.SelectUnit();
				GameController.unitUI.OpenPanel(selectedUnit);
				//currentCell.IsSelected = true;
			}
		}
	}

	public void DeselectUnit(){
		if(selectedUnit){
			selectedUnit.DeselectUnit();
		}
		selectedUnit = null;
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
			GameController.unitUI.ClosePanel();
			selectedUnit.Travel(grid.GetPath());
			grid.ClearPath();
			selectedUnit.HideDisplays();

		}
	}

	void DoSelectTarget(){
		if(selectedUnit != null && selectedUnit.localAttackTiles != null){
			if(selectedUnit.localAttackTiles.Contains(currentCell)){
				if(currentCell.Unit != null && currentCell.Unit != selectedUnit){
					//do combat
					
					StartCoroutine(selectedUnit.TurnToLookAt(currentCell.Position));
					CombatManager.StartCombat(selectedUnit,currentCell.Unit,1);
					
				}
				selectedUnit.FinishAction();
				DeselectUnit();
			}
		}
	}
}