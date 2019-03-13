using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    [SerializeField]
    HexGrid hexGrid;
    
    [SerializeField]
    Color[] colors;


    private Color activeColor;
    public int activeElevation;

    void Awake()
    {
        SelectColor(0);
    }

    // Update is called once per frame
    void Update(){
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
			HandleInput();
		}
    }

    void HandleInput(){
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			EditCell(hexGrid.GetCell(hit.point));
		}
    }

    public void SelectColor (int index) {
		activeColor = colors[index];
	}

    public void SelectElevation (float elevation) {
		activeElevation = (int)elevation;
	}

    void EditCell (HexCell cell) {
		cell.color = activeColor;
        cell.Elevation = activeElevation;
		hexGrid.Refresh();
	}
}
