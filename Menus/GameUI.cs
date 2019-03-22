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
						
			if (Input.GetMouseButtonDown(0)) {
				DoSelection();
			}
			else if (selectedUnit) {
				if (Input.GetMouseButtonDown(1)) {
					DoMove();
				}
				else {
					DoPathfinding();
				}
			}
			else{
				HexCell prevCell = currentCell; 
				UpdateCurrentCell();				
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

	void DoSelection () {
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
			selectedUnit.Travel(grid.GetPath());
			grid.ClearPath();
			DeselectUnit();
		}
	}
}