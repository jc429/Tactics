using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    
	//set true to disable this cell entirely
	public bool invalid;	

	// elevation of cell
    int elevation = int.MinValue;

	// water level of cell
    int waterLevel = int.MinValue;

	// cell terrain type
	TerrainType terrain = TerrainType.Default;
	int terrainTypeIndex;

	// cell label
    public RectTransform uiRect;

	// adjacent cells
    [SerializeField]
	[NamedArrayAttribute (new string[] {"NE", "E", "SE", "SW", "W", "NW"})]
	HexCell[] neighbors;

	// chunk this cell is a member of 
	public HexGridChunk chunk;

	// distance to this cell from another cell
	int distanceToCell;


	//unit residing in this cell
	public MapUnit Unit { get; set; }

	public struct ColorFlags{
		HexCell cell;
		bool isHoveredOn; 
		bool isSelected; 
		bool onMovementPath; 
		bool inMovementRange;
		bool inAttackRange;
		bool inAssistRange;

		public ColorFlags(HexCell parent){
			cell = parent;
			isHoveredOn = isSelected = onMovementPath = inMovementRange = inAttackRange = inAssistRange = false;
		}

		public bool IsHoveredOn{
			get{ return isHoveredOn; }
			set{
				isHoveredOn = value;
				cell.RefreshHighlight();
			}
		}
		public bool IsSelected{
			get{ return isSelected; }
			set{
				isSelected = value;
				cell.RefreshHighlight();
			}
		} 
		public bool OnMovementPath{
			get{ return onMovementPath; }
			set{
				onMovementPath = value;
				cell.RefreshHighlight();
			}
		} 
		public bool InMovementRange{
			get{ return inMovementRange; }
			set{
				inMovementRange = value;
				cell.RefreshHighlight();
			}
		} 
		public bool InAttackRange{
			get{ return inAttackRange; }
			set{
				inAttackRange = value;
				cell.RefreshHighlight();
			}
		} 
		public bool InAssistRange{
			get{ return inAssistRange; }
			set{
				inAssistRange = value;
				cell.RefreshHighlight();
			}
		} 
		public void Clear(){
			isHoveredOn = isSelected = onMovementPath = inMovementRange = inAttackRange = inAssistRange = false;
		}


	}
	public ColorFlags colorFlags;

	

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
	public TerrainType Terrain {
		get {
			return terrain;
		}
		set {
			if (terrain != value) {
				terrain = value;
				Refresh();
			}
		}
	}
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


	public HexCell(){
		colorFlags = new ColorFlags(this);
	}

	
    // Start is called before the first frame update
    void Start()
    {
		DisableHighlight();
		if(invalid){
			uiRect.gameObject.SetActive(false);
		}
    }



	/* turns off cell highlight */
	public void DisableHighlight () {
		Image highlight = uiRect.GetChild(0).GetComponent<Image>();
		colorFlags.Clear();
		highlight.enabled = false;
	}
	
	/* turns on cell highlight and sets its color */
	void EnableHighlight (Color color) {
		Image highlight = uiRect.GetChild(0).GetComponent<Image>();
		highlight.color = color;
		highlight.enabled = true;
	}

	/* refreshes the cell's highlight so changes take place */
	public void RefreshHighlight(){
		if(colorFlags.IsHoveredOn){
			EnableHighlight(Colors.UIColors.HoverColor);
		}
		else if(colorFlags.IsSelected){
			EnableHighlight(Colors.UIColors.StartColor);
		}
		else if(colorFlags.OnMovementPath){
			EnableHighlight(Colors.UIColors.PathColor);
		}
		else if(colorFlags.InMovementRange){
			EnableHighlight(Colors.UIColors.MoveRangeColor);
		}
		else if(colorFlags.InAttackRange){
			EnableHighlight(Colors.UIColors.AttackRangeColor);
		}
		else if(colorFlags.InAssistRange){
			EnableHighlight(Colors.UIColors.AssistRangeColor);
		}
		else{
			DisableHighlight();
		}
	}

	/* updates cell label when calculating distance */
	void UpdateDistanceLabel () {
		SetLabel(distanceToCell == int.MaxValue ? "" : distanceToCell.ToString());
	}

	/* sets the cell label to whatever */
	public void SetLabel (string text) {
		TextMeshPro label = uiRect.GetComponent<TextMeshPro>();
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
		if(neighbors[(int)direction] != null && neighbors[(int)direction].invalid){
			return null;
		}
		return neighbors[(int)direction];
	}

    public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}




	public void SaveCell(BinaryWriter writer) {
		writer.Write(invalid);
		writer.Write((byte)terrainTypeIndex);
		writer.Write((byte)elevation);
		writer.Write((byte)waterLevel);
	}

	public void LoadCell(BinaryReader reader) {
		invalid = reader.ReadBoolean();
		terrainTypeIndex = reader.ReadByte();
		terrain = (TerrainType)terrainTypeIndex;
		elevation = reader.ReadByte();
		RefreshPosition();
		waterLevel = reader.ReadByte();	
	}
	
}
