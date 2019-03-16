using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    [SerializeField]
    HexGrid hexGrid;
    
    


	/* tools for detecting click + drag inputs */
	bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;

    
	int activeTerrainTypeIndex;

    int activeElevation;
	// whether or not to apply the selected elevation
	bool applyElevation = true;
	
	int activeWaterLevel;
	// whether or not to apply the selected water level
	bool applyWaterLevel = true;


	// size of edit brush
	int brushSize;



    void Awake()
    {

    }

    // Update is called once per frame
    void Update(){
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
			HandleInput();
		}
		else {
			previousCell = null;
		}
    }


	
    void HandleInput(){
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			HexCell currentCell = hexGrid.GetCell(hit.point);
			if (previousCell && previousCell != currentCell) {
				ValidateDrag(currentCell);
			}
			else {
				isDrag = false;
			}
			EditCells(currentCell);
			previousCell = currentCell;
		}
		else {
			previousCell = null;
		}
    }

	/* check that the input is a click + drag */
	void ValidateDrag (HexCell currentCell) {
		for (
			dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
			dragDirection++
		) {
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
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
		hexGrid.ShowUI(visible);
	}

    void EditCell (HexCell cell) {
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

	void EditCells (HexCell center) {
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;

		// bottom to center
		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}

		// top to row above center
		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
	}
	
	public void SaveMap() {
		Debug.Log("Saving to: " + Application.persistentDataPath);
		string path = Path.Combine(Application.persistentDataPath, "test.map");
		using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create))) {
			hexGrid.SaveGrid(writer);
		}
	}

	public void LoadMap() {
		string path = Path.Combine(Application.persistentDataPath, "test.map");
		using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
			hexGrid.LoadGrid(reader);
		}
	}
	
}
