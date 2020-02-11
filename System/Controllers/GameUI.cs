﻿using UnityEngine;
using UnityEngine.EventSystems;

public class GameUI : MonoBehaviour {
	
	public HexGrid hexGrid;
	public MapGrid mapGrid;

	public BasicMenu unitActionMenu;

	// cell the mouse cursor is over
	MapCell currentCell;

	MapUnit selectedUnit;
	// cell the unit started their turn in
	MapCell startCell;
	OctDirection startFacing;

	void Awake(){
		GameController.gameUI = this;
	}

	void Start(){
		unitActionMenu.CloseMenu();
	}

	void Update () {
		mapGrid.StartMap();
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
						UpdateCurrentCell();
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
		mapGrid.ShowUI(!toggle);
		mapGrid.ClearPath();
	}

	bool UpdateCurrentCell () {
		MapCell cell = mapGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
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
		UpdateCurrentCell();
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
		if (UpdateCurrentCell()) {
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