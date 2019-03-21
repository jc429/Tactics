using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    

	// elevation of cell
    int elevation = int.MinValue;

	// water level of cell
    int waterLevel = int.MinValue;

	// cell terrain type
	int terrainTypeIndex;

	// cell label
    public RectTransform uiRect;

	// adjacent cells
    [SerializeField]
	HexCell[] neighbors;

	// chunk this cell is a member of 
	public HexGridChunk chunk;

	// distance to this cell from another cell
	int distanceToCell;


	//unit residing in this cell
	public HexUnit Unit { get; set; }

	bool isHoveredOn, isSelected, onMovementPath, inMovementRange, inAttackRange;

	public bool IsHoveredOn{
		get{ return isHoveredOn; }
		set{
			isHoveredOn = value;
			RefreshHighlight();
		}
	}
	public bool IsSelected{
		get{ return isSelected; }
		set{
			isSelected = value;
			RefreshHighlight();
		}
	} 
	public bool OnMovementPath{
		get{ return onMovementPath; }
		set{
			onMovementPath = value;
			RefreshHighlight();
		}
	} 
	public bool InMovementRange{
		get{ return inMovementRange; }
		set{
			inMovementRange = value;
			RefreshHighlight();
		}
	} 
	public bool InAttackRange{
		get{ return inAttackRange; }
		set{
			inAttackRange = value;
			RefreshHighlight();
		}
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
			RefreshPosition();

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

	/* Distance to this cell */
	public int DistanceToCell {
		get {
			return distanceToCell;
		}
		set {
			distanceToCell = value;
		//	UpdateDistanceLabel();
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

	/* is cell submerged? */
	public bool IsUnderwater {
		get {
			return waterLevel > elevation;
		}
	}

	/* returns terrain type of cell */
	public int TerrainTypeIndex {
		get {
			return terrainTypeIndex;
		}
		set {
			if (terrainTypeIndex != value) {
				terrainTypeIndex = value;
				Refresh();
			}
		}
	}

	public int CostToEnter(HexUnit unit){
		//TODO: calc actual movement costs
		return 2;
	}

	/* Color of cell */
	public Color CellColor {
		get {
			return HexMetrics.colors[terrainTypeIndex];
		}
		set {
		}
	}

	/* for calculating path from this cell back to search start */
	public HexCell PathParent { get; set; }

	public int SearchHeuristic { get; set; }

	/* for more intelligent pathfinding */
	public int SearchPriority {
		get {
			return distanceToCell + SearchHeuristic;
		}
	}

	/* 0 = not touched yet, 1 = currently in frontier, 2 = removed from frontier */
	public int SearchPhase { get; set; }

	/* linked list of cells sharing a priority level */
	public HexCell NextWithSamePriority { get; set; }

	
    // Start is called before the first frame update
    void Start()
    {
		DisableHighlight();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



	/* turns off cell highlight */
	public void DisableHighlight () {
		Image highlight = uiRect.GetChild(0).GetComponent<Image>();
		highlight.enabled = false;
		isHoveredOn = false;
		isSelected = false;
		onMovementPath = false;
		inMovementRange = false;
		inAttackRange = false;
	}
	
	/* turns on cell highlight and sets its color */
	void EnableHighlight (Color color) {
		Image highlight = uiRect.GetChild(0).GetComponent<Image>();
		highlight.color = color;
		highlight.enabled = true;
	}

	public void RefreshHighlight(){
		if(IsHoveredOn){
			EnableHighlight(GameProperties.UIColors.HoverColor);
		}
		else if(IsSelected){
			EnableHighlight(GameProperties.UIColors.StartColor);
		}
		else if(OnMovementPath){
			EnableHighlight(GameProperties.UIColors.PathColor);
		}
		else if(InMovementRange){
			EnableHighlight(GameProperties.UIColors.MoveRangeColor);
		}
		else if(InAttackRange){
			EnableHighlight(GameProperties.UIColors.AttackRangeColor);
		}
		else{
			DisableHighlight();
		}
	}

	/* updates cell label when calculating distance */
	void UpdateDistanceLabel () {
		SetLabel(distanceToCell == int.MaxValue ? "" : distanceToCell.ToString());
	}

	/* sets the celll label to whatever */
	public void SetLabel (string text) {
		Text label = uiRect.GetComponent<Text>();
		label.text = text;
	}

	/* Refreshes chunk (and, by extension, cell) and potentially neighbors */
	void Refresh () {
		if(chunk != null){
			chunk.Refresh();
			if (Unit) {
				Unit.ValidateLocation();
			}
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
		if (Unit) {
			Unit.ValidateLocation();
		}
	}

	/* adjusts cell height when elevation is changed */
	void RefreshPosition () {
		Vector3 position = transform.localPosition;
		position.y = elevation * HexMetrics.elevationStep;
		transform.localPosition = position;

		Vector3 uiPosition = uiRect.localPosition;
		uiPosition.z = elevation * -HexMetrics.elevationStep;
		uiRect.localPosition = uiPosition;
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




	public void SaveCell(BinaryWriter writer) {
		writer.Write((byte)terrainTypeIndex);
		writer.Write((byte)elevation);
		writer.Write((byte)waterLevel);
	}

	public void LoadCell(BinaryReader reader) {
		terrainTypeIndex = reader.ReadByte();
		elevation = reader.ReadByte();
		RefreshPosition();
		waterLevel = reader.ReadByte();	
	}
	
}
