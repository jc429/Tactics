using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HexCell : Cell
{
	public HexCoordinates coordinates;
    
	// adjacent cells
	[SerializeField]
	[NamedArrayAttribute (new string[] {"NE", "E", "SE", "SW", "W", "NW"})]
	HexCell[] neighbors;

	// chunk this cell is a member of 
	public HexGridChunk chunk;

	public HexCell(){
		colorFlags = new ColorFlags(this);
	}

	/* Refreshes chunk (and, by extension, cell) and potentially neighbors */
	protected override void Refresh () {
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
	protected override void RefreshSelfOnly () {
		chunk.Refresh();
		if (Unit) {
			Unit.ValidateLocation();
		}
	}

	

	public EdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}

	public EdgeType GetEdgeType (HexCell otherCell) {
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




	public override void SaveCell(BinaryWriter writer) {
		writer.Write(invalid);
		writer.Write((byte)terrainTypeIndex);
		writer.Write((byte)elevation);
		writer.Write((byte)waterLevel);
	}

	public override void LoadCell(BinaryReader reader) {
		invalid = reader.ReadBoolean();
		terrainTypeIndex = reader.ReadByte();
		terrain = (TerrainType)terrainTypeIndex;
		elevation = reader.ReadByte();
		RefreshPosition();
		waterLevel = reader.ReadByte();	
	}
	
}
