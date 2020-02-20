using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cell : MonoBehaviour
{

	//set true to disable this cell entirely
	public bool invalid;	

	/* Elevation of cell */
	protected int elevation = int.MinValue;
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
	protected int waterLevel = int.MinValue;
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

	// cell terrain type
	protected TerrainType terrain = TerrainType.Default;
	protected int terrainTypeIndex;

	// cell label
	public RectTransform uiRect;

	// chunk this cell is a member of 
	// public MapGridChunk chunk;

	/* Distance to this cell */
	protected int distanceToCell;
	public int DistanceToCell {
		get {
			return distanceToCell;
		}
		set {
			distanceToCell = value;
		//	UpdateDistanceLabel();
		}
	}

	// unit residing in this cell
	public MapUnit Unit { get; set; }
	
	// flags for determining how to color cell 
	public ColorFlags colorFlags;

	/* Position of cell */
	public Vector3 Position {
		get {
			return transform.localPosition;
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
			return QuadMetrics.colors[terrainTypeIndex];
		}
		set {
		}
	}

	/* for calculating path from this cell back to search start */
	public Cell PathParent { get; set; }

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
	public Cell NextWithSamePriority { get; set; }


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

	protected virtual void Refresh(){
		if (Unit) {
			Unit.ValidateLocation();
		}
	}

	/* Refreshes only the chunk containing this cell */
	protected virtual void RefreshSelfOnly () {
		if (Unit) {
			Unit.ValidateLocation();
		}
	}

	/* adjusts cell height when elevation is changed */
	protected void RefreshPosition () {
		Vector3 position = transform.localPosition;
		position.y = elevation * HexMetrics.elevationStep;
		transform.localPosition = position;

		Vector3 uiPosition = uiRect.localPosition;
		uiPosition.z = elevation * -HexMetrics.elevationStep;
		uiRect.localPosition = uiPosition;
	}

	public virtual void SaveCell(BinaryWriter writer) {
		writer.Write(invalid);
		writer.Write((byte)terrainTypeIndex);
		writer.Write((byte)elevation);
		writer.Write((byte)waterLevel);
	}

	public virtual void LoadCell(BinaryReader reader) {
		invalid = reader.ReadBoolean();
		terrainTypeIndex = reader.ReadByte();
		terrain = (TerrainType)terrainTypeIndex;
		elevation = reader.ReadByte();
		RefreshPosition();
		waterLevel = reader.ReadByte();	
	}
}
