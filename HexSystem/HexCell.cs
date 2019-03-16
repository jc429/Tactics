using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    

	// elevation of cell
    int elevation = int.MinValue;

	// water level of cell
    int waterLevel = int.MinValue;

	// cell color
	Color color;

	// cell label
    public RectTransform uiRect;

	// adjacent cells
    [SerializeField]
	HexCell[] neighbors;

	// chunk this cell is a member of 
	public HexGridChunk chunk;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


	/* Position of cell */
	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}
    
	/* Elevation of cell */
    public int Elevation {
		get {
			return elevation;
		}
		set {
			if (elevation == value) {
				return;
			}

			elevation = value;
            Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.elevationStep;
			transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = elevation * -HexMetrics.elevationStep;
			uiRect.localPosition = uiPosition;

			Refresh();
		}
	}

	/* Water Level of cell */
	public int WaterLevel {
		get {
			return waterLevel;
		}
		set {
			if (waterLevel == value) {
				return;
			}
			waterLevel = value;
			Refresh();
		}
	}

	/* returns the physical location of the water surface */
	public float WaterSurfaceY {
		get {
			return
				(waterLevel + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	/* Color of cell */
	public Color CellColor {
		get {
			return color;
		}
		set {
			if (color == value) {
				return;
			}
			color = value;
			Refresh();
		}
	}



	/* Refreshes chunk (and, by extension, cell) and potentially neighbors */
	void Refresh () {
		if(chunk != null){
			chunk.Refresh();

			// if this cell is neighboring any other chunks, they must be refreshed too
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) {
					neighbor.chunk.Refresh();
				}
			}
		}
	}

	/* Refreshes only the chunk containing this cell */
	void RefreshSelfOnly () {
		chunk.Refresh();
	}

	public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}

	public HexEdgeType GetEdgeType (HexCell otherCell) {
		return HexMetrics.GetEdgeType(
			elevation, otherCell.elevation
		);
	}

    public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

    public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	/* is cell submerged? */
	public bool IsUnderwater {
		get {
			return waterLevel > elevation;
		}
	}

	
}
