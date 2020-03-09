using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapEditor : MonoBehaviour
{
	//public HexGrid hexGrid;
	public MapGrid mapGrid;
    
	public GameObject editorPanel;

	public Material terrainMaterial;

	/* tools for detecting click + drag inputs */
	bool isDrag = false;
	QuadDirection dragDirection;
	MapCell previousCell;

    
	int activeTerrainTypeIndex;

	int activeElevation;
	// whether or not to apply the selected elevation
	bool applyElevation = true;
	
	int activeWaterLevel;
	// whether or not to apply the selected water level
	bool applyWaterLevel = true;


	// size of edit brush
	int brushSize;


	/* for measuring cell distances */
//	MapCell searchFromCell, searchToCell;



	void Awake () {
		GameController.mapEditor = this;
		terrainMaterial.DisableKeyword("GRID_ON");
	}

    // Update is called once per frame
	void Update(){
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (Input.GetMouseButton(0)) {
				HandleInput();
				return;
			}
			if (Input.GetKeyDown(KeyCode.U)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					DestroyUnit();
				}
				else {
					CreateUnit();
				}
				return;
			}
		}
		previousCell = null;
	}

	public void SetMapEditorActive(bool b){
		SetEditMode(b);
		SetEditorPanelActive(b);
	}

	
	void HandleInput(){
		MapCell currentCell = GetCellUnderCursor();
		if (currentCell) {
			if (previousCell && previousCell != currentCell) {
				ValidateDrag(currentCell);
			}
			else {
				isDrag = false;
			}
			EditCells(currentCell);
			
			/*else if (Input.GetKey(KeyCode.LeftShift) && searchToCell != currentCell) {
				if (searchFromCell != currentCell) {
					if (searchFromCell) {
						searchFromCell.DisableHighlight();
					}
					searchFromCell = currentCell;
					searchFromCell.EnableHighlight(Color.blue);
					if (searchToCell) {
						hexGrid.FindPath(searchFromCell, searchToCell, 7);
					}
				}
			}
			else if (searchFromCell && searchFromCell != currentCell) {
				if (searchToCell != currentCell) {
					searchToCell = currentCell;
					hexGrid.FindPath(searchFromCell, searchToCell, 7);
				}
			}*/
			previousCell = currentCell;
		}
		else {
			previousCell = null;
		}
    }

	/* returns the cell the cursor is pointing at */
	MapCell GetCellUnderCursor () {
		return mapGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
	}

	/* check that the input is a click + drag */
	void ValidateDrag (MapCell currentCell) {
		for (
			dragDirection = QuadDirection.N;
			dragDirection <= QuadDirection.W;
			dragDirection++
		) {
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
	}

	public void SetEditMode (bool toggle) {
		//editMode = toggle;
		//hexGrid.ShowUI(!toggle);
		enabled = toggle;
	}

	public void SetEditorPanelActive(bool toggle){
		editorPanel.SetActive(toggle);
	}

	public void SetTerrainTypeIndex (int index) {
		activeTerrainTypeIndex = index;
	}

    public void SelectElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}
	
	public void SelectWaterLevel (float level) {
		activeWaterLevel = (int)level;
	}

	public void SetApplyWaterLevel (bool toggle) {
		applyWaterLevel = toggle;
	}


	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void ShowUI (bool visible) {
		mapGrid.ShowUI(visible);
	}

	public void ShowGrid (bool visible) {
		if (visible) {
			terrainMaterial.EnableKeyword("GRID_ON");
		}
		else {
			terrainMaterial.DisableKeyword("GRID_ON");
		}
	}

    void EditCell (MapCell cell) {
		if(cell != null){
			if (activeTerrainTypeIndex >= 0) {
				cell.TerrainTypeIndex = activeTerrainTypeIndex;
			}
			if(applyElevation){
				cell.Elevation = activeElevation;
			}
			if (applyWaterLevel) {
				cell.WaterLevel = activeWaterLevel;
			}
		}
	}

	void EditCells (MapCell center) {
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Y;

		// bottom to center
		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(mapGrid.GetCell(new MapCoordinates(x, z)));
			}
		}

		// top to row above center
		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(mapGrid.GetCell(new MapCoordinates(x, z)));
			}
		}
	}
	
	/* spawn a unit */
	void CreateUnit () {
		MapCell cell = GetCellUnderCursor();
		if (cell && !cell.Unit) {
			mapGrid.AddUnit(Instantiate(MapUnit.unitPrefab), cell, QuadDirectionExtensions.RandomDirection().ConvertToOctDirection()); 
		}
	}

	/* KILL */
	void DestroyUnit () {
		MapCell cell = GetCellUnderCursor();
		if (cell && cell.Unit) {
			cell.Unit.Die();
		}
	}
	
}
