using UnityEngine;
using UnityEngine.EventSystems;

public class GameUI : MonoBehaviour {
	
	//public HexGrid hexGrid;
	public MapGrid mapGrid;

	public BasicMenu unitActionMenu;

	const float gridStepDuration = 0.3f;
	Timer gridStepTimer = new Timer(gridStepDuration);

	// cell the mouse cursor is over / dpad has selected
	MapCell currentCell;


	[Header("Selected Unit")]
	MapUnit selectedUnit;
	MapCell startCell; // cell the unit started their turn in
	OctDirection startFacing;

	void Awake(){
		GameController.gameUI = this;
	}

	void Start(){
		unitActionMenu.CloseMenu();
		
	}

	void Update () {
		switch(GameSettings.cursorInputType){
			case CursorInputType.DPad:
				UpdateInputsDPad();
				break;
			case CursorInputType.Mouse:
				UpdateInputsMouse();
				break;
		}
	}

	public void StartGame(){
		unitActionMenu.CloseMenu();
		if(GameSettings.cursorInputType == CursorInputType.DPad){
			MapCell c = mapGrid.GetCell(new MapCoordinates(0,0));
			UpdateCurrentCell(c);
		}
	}

	void UpdateInputsDPad(){
		// check for directional inputs first 
		Vector2 dirHeld = InputController.GetDirectionHeld();

		if(gridStepTimer.IsActive){
			gridStepTimer.AdvanceTimer(Time.deltaTime);
			if(gridStepTimer.IsFinished){
				gridStepTimer.Reset();
			}
		}
		else{
			if(dirHeld != Vector2.zero){
				MapCell newCell = currentCell;
				if(dirHeld.x != 0 && newCell != null){
					QuadDirection d = QuadDirectionExtensions.QuadDirectionFromVector(new Vector2(dirHeld.x, 0));
					Debug.Log(d);
					newCell = newCell.GetNeighbor(d);
				}
				if(dirHeld.y != 0 && newCell != null){
					QuadDirection d = QuadDirectionExtensions.QuadDirectionFromVector(new Vector2(0, dirHeld.y));
					Debug.Log(d);
					newCell = newCell.GetNeighbor(d);
				}
				// move if cell changed
				if(newCell != null && newCell != currentCell){
					UpdateCurrentCell(newCell);
					gridStepTimer.Start();
				}
			}
			else{	//no direction held

			}
		}
	}

	void UpdateInputsMouse(){
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if(selectedUnit != null){
				switch(selectedUnit.turnState){
				case TurnState.PreMove:
					if (Input.GetMouseButtonDown(0)) {
						DoMove();
					}
					else if(Input.GetMouseButtonDown(1)){
						mapGrid.ClearPath();
						selectedUnit.turnState = TurnState.Idle;
						DeselectCurrentUnit();
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
						UpdateCurrentCellMouse();
						if(currentCell != null && currentCell.Unit != null){
							if(selectedUnit.Properties.affiliation != currentCell.Unit.Properties.affiliation){
								CombatManager.PreCalculateCombat(selectedUnit, currentCell.Unit);
								CombatManager.combatForecast.Show(selectedUnit, currentCell.Unit);
							}
							else{
								CombatManager.combatForecast.Hide();
							}
						}
						else{
							CombatManager.combatForecast.Hide();
						}
					}
					break;
				case TurnState.Finished:
					if (Input.GetMouseButtonDown(0)) {
						DoSelectUnit();
					}
					else{
						UpdateCurrentCellMouse();
					}
					break;
				default:
					UpdateCurrentCellMouse();
					break;
				}
			}
			else{
				if (Input.GetMouseButtonDown(0)) {
					DoSelectUnit();
				}
				else{
					UpdateCurrentCellMouse();				
				}
			}

			
		}
	}

	public void SetEditMode (bool toggle) {
		enabled = !toggle;
		mapGrid.ShowUI(!toggle);
		mapGrid.ClearPath();
	}

	bool UpdateCurrentCell(MapCell cell){
		if(currentCell != null){
			currentCell.colorFlags.IsHoveredOn = false;
		}
		currentCell = cell;
		if(currentCell != null){
			currentCell.colorFlags.IsHoveredOn = true;
		}
		return (currentCell != null);
	}

	bool UpdateCurrentCellMouse () {
		MapCell cell = mapGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		return UpdateCurrentCell(cell);
	}

	void DoSelectUnit () {
		mapGrid.ClearPath();
		if(selectedUnit != null){
			if(selectedUnit.isTraveling){
				return;
			}
			DeselectCurrentUnit();
		}
		if(currentCell != null){
			currentCell.colorFlags.IsSelected = false;
		}
		UpdateCurrentCellMouse();
		if (currentCell) {
			MapUnit unit = currentCell.Unit;
			if(unit != null){
				if(unit.isTraveling){
					return;
				}
				selectedUnit = unit;
				startCell = unit.CurrentCell;
				startFacing = unit.Facing;
				mapGrid.CalculateMovementRange(currentCell,selectedUnit);
				mapGrid.CalculateTotalAttackRange(selectedUnit);
				selectedUnit.SelectUnit();
				GameController.unitInfoPanel.OpenPanel(selectedUnit);
				//currentCell.IsSelected = true;
			}
		}
	}

	public void DeselectCurrentUnit(){
		if(selectedUnit){
			selectedUnit.DeselectUnit();
		}
		unitActionMenu.CloseMenu();
		GameController.unitInfoPanel.ClosePanel();
	
		selectedUnit = null;
	}

	void ReturnUnitToStart(){
		if(selectedUnit){
			selectedUnit.CurrentCell = startCell;
			selectedUnit.Facing = startFacing;
			selectedUnit.turnState = TurnState.PreMove;
		}
	}
	
	/* calculate the path the selected unit would take to reach the tile the cursor is on */
	void DoPathfinding () {
		if (UpdateCurrentCellMouse()) {
			if (currentCell /*&& selectedUnit.IsValidDestination(currentCell)*/) {
				mapGrid.FindPath(selectedUnit.CurrentCell, currentCell, selectedUnit);
			}
			else {
				mapGrid.ClearPath();
			}
		}
	}

	void DoMove () {
		if (mapGrid.HasPath && !selectedUnit.isTraveling) {
			GameController.unitInfoPanel.ClosePanel();
			selectedUnit.Travel(mapGrid.GetPath());
			mapGrid.ClearPath();
			selectedUnit.HideDisplays();
		}
		else if(currentCell != null && currentCell.Unit == selectedUnit){
			mapGrid.ClearPath();
			selectedUnit.HideDisplays();
			selectedUnit.turnState = TurnState.PostMove;
			GameController.mapGrid.CalculateLocalAttackRange(currentCell,selectedUnit);
			OpenUnitActionMenu();
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
					selectedUnit.FaceCell(currentCell);
					CombatManager.CalculateAndPerformCombat(selectedUnit,currentCell.Unit);
					
				}
				EndSelectedUnitAction();
			}
		}
	}

	public void EndSelectedUnitAction(){
		if(selectedUnit == null){
			return;
		}
		selectedUnit.EndAction();
		DeselectCurrentUnit();
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
		mapGrid.FindPath(selectedUnit.CurrentCell, currentCell, selectedUnit);
	}
}