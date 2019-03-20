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
				//if(prevCell != currentCell){
					if(currentCell != null){
						currentCell.EnableHighlight(Color.white);
					}
					if(prevCell != null && prevCell != currentCell){
						prevCell.DisableHighlight();
					}
				//}
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
			currentCell = cell;
			return true;
		}
		return false;
	}

	void DoSelection () {
		grid.ClearPath();
		UpdateCurrentCell();
		if (currentCell) {
			selectedUnit = currentCell.Unit;
			if(selectedUnit != null){
				currentCell.EnableHighlight(Color.blue);
			}
		}
	}

	public void DeselectUnit(){
		selectedUnit = null;
	}

	void DoPathfinding () {
		if (UpdateCurrentCell()) {
			if (currentCell /*&& selectedUnit.IsValidDestination(currentCell)*/) {
				grid.FindPath(selectedUnit.Location, currentCell, selectedUnit.moveRange);
			}
			else {
				grid.ClearPath();
			}
		}
	}

	void DoMove () {
		if (grid.HasPath) {
			selectedUnit.Travel(grid.GetPath());
			grid.ClearPath();
			DeselectUnit();
		}
	}
}