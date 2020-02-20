using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
	HexCell[] cellsArray;
	List<HexCell> cellsList;

	public CellMesh terrain, water, waterShore;
	Canvas gridCanvas;

	bool refreshQueued = false;

	/* colors for splat map */
	static Color color1 = new Color(1f, 0f, 0f);
	static Color color2 = new Color(0f, 1f, 0f);
	static Color color3 = new Color(0f, 0f, 1f);

	void Awake () {
		gridCanvas = GetComponentInChildren<Canvas>();

		cellsArray = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
		cellsList = new List<HexCell>();
	}
	
	void Start () {
		TriangulateAll();
	}

	void LateUpdate(){
		if(refreshQueued){
			TriangulateAll();
			refreshQueued = false;
		}
	}

	/* assigns a cell to this chunk */
	public void AddCell (/*int index, */HexCell cell) {
		//cellsArray[index] = cell;
		cellsList.Add(cell);
		cell.chunk = this;
		cell.transform.SetParent(transform, false);
		cell.uiRect.SetParent(gridCanvas.transform, false);
	}

	/* queues a chunk refresh for the end of the current frame */
	public void Refresh () {
		refreshQueued = true;
	}

	/* show or hide cell labels */
	public void ShowUI (bool visible) {
		gridCanvas.gameObject.SetActive(visible);
	}


	
	/* Triangulates All Cells */
	public void TriangulateAll(){
		terrain.ClearAll();
		water.ClearAll();
		waterShore.ClearAll();

		foreach(HexCell c in cellsList){
			if(!c.invalid){
				TriangulateCell(c);
			}
		}

		terrain.Apply();
		water.Apply();
		waterShore.Apply();
	
	}

	/* Creates the 6 triangles that comprise a hex cell */
	void TriangulateCell(HexCell cell){
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
			Triangulate(d, cell);
		}
	}

	/* Creates a single triangle */
	void Triangulate(HexDirection direction, HexCell cell){
	
		Vector3 center = cell.transform.localPosition;
		EdgeVertices e = new EdgeVertices(
			center + HexMetrics.GetFirstSolidCorner(direction),
			center + HexMetrics.GetSecondSolidCorner(direction)
		);

		TriangulateEdgeFan(center, e, cell.TerrainTypeIndex);
/*
		// solid core triangle
		terrain.AddTriangle(center, v1, v2);
		terrain.AddTriangleColor(color1);
		//terrain.AddTriangleColor(cell.CellColor);
		float type = cell.TerrainTypeIndex;
		Vector3 types;
		types.x = types.y = types.z = type;
		terrain.AddTriangleTerrainTypes(types);
*/
		if (direction <= HexDirection.SE) {
			TriangulateConnection(direction, cell, e);
		}

		if (cell.IsUnderwater) {
			TriangulateWater(direction, cell, center);
		}
	}

	/* triangulates the joints between tiles */
	void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1){

		HexCell neighbor = cell.GetNeighbor(direction);
		if(neighbor == null){
				return;
		}

		// quad stretching to edge of tile
		Vector3 bridge = HexMetrics.GetBridge(direction);
		bridge.y = neighbor.Position.y - cell.Position.y;
		EdgeVertices e2 = new EdgeVertices(
			e1.v1 + bridge,
			e1.v2 + bridge
		);

		if (cell.GetEdgeType(direction) == EdgeType.Terrace) {
			TriangulateEdgeTerraces(e1, cell, e2, neighbor);
		}
		else {
			// ???
			TriangulateEdgeStrip(e1, color1, cell.TerrainTypeIndex, e2, color2, neighbor.TerrainTypeIndex);
		}

		// remaining corner triangles
		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction <= HexDirection.E && nextNeighbor != null) {
			Vector3 v5 = e1.v2 + HexMetrics.GetBridge(direction.Next());
			v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;

			//start from lowest elevation
			if (cell.Elevation <= neighbor.Elevation) {
				if (cell.Elevation <= nextNeighbor.Elevation) {
					TriangulateCorner(e1.v2, cell, e2.v2, neighbor, v5, nextNeighbor);
				}
				else {
					TriangulateCorner(v5, nextNeighbor, e1.v2, cell, e2.v2, neighbor);
				}
			}
			else if (neighbor.Elevation <= nextNeighbor.Elevation) {
				TriangulateCorner(e2.v2, neighbor, v5, nextNeighbor, e1.v2, cell);
			}
			else {
				TriangulateCorner(v5, nextNeighbor, e1.v2, cell, e2.v2, neighbor);
			}
		}
	}

	void TriangulateEdgeTerraces(
		EdgeVertices begin, HexCell beginCell,
		EdgeVertices end, HexCell endCell
	) {
        
		EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
		Color c2 = HexMetrics.TerraceLerp(color1, color2, 1);
		float t1 = beginCell.TerrainTypeIndex;
		float t2 = endCell.TerrainTypeIndex;

		TriangulateEdgeStrip(begin, color1, t1, e2, c2, t2);

		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			EdgeVertices e1 = e2;
			Color c1 = c2;
			e2 = EdgeVertices.TerraceLerp(begin, end, i);
			//c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
			c2 = HexMetrics.TerraceLerp(color1, color2, i);
			TriangulateEdgeStrip(e1, c1, t1, e2, c2, t2);
		}

		TriangulateEdgeStrip(e2, c2, t1, end, color2, t2);
    }

	void TriangulateCorner (
		Vector3 bottom, HexCell bottomCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		EdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
		EdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

		if (leftEdgeType == EdgeType.Terrace) {
			if (rightEdgeType == EdgeType.Terrace) {
				TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
			else if (rightEdgeType == EdgeType.Flat) {
				TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
			}
			else{
				TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		else if (rightEdgeType == EdgeType.Terrace) {
			if (leftEdgeType == EdgeType.Flat) {
				TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
			}
			else{
				TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		else if (leftCell.GetEdgeType(rightCell) == EdgeType.Terrace) {
			if (leftCell.Elevation < rightCell.Elevation) {
				TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
			}
			else {
				TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
			}
		}
		else{ 
			terrain.AddTriangle(bottom, left, right);
			terrain.AddTriangleColor(color1, color2, color3);
			Vector3 types;
			types.x = bottomCell.TerrainTypeIndex;
			types.y = leftCell.TerrainTypeIndex;
			types.z = rightCell.TerrainTypeIndex;
			terrain.AddTriangleTerrainTypes(types);
		}
	}

	void TriangulateCornerTerraces (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		
		Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
		Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
		Color c3 = HexMetrics.TerraceLerp(color1, color2, 1);
		Color c4 = HexMetrics.TerraceLerp(color1, color3, 1);
		Vector3 types;
		types.x = beginCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;

		terrain.AddTriangle(begin, v3, v4);
		terrain.AddTriangleColor(beginCell.CellColor, c3, c4);
		terrain.AddTriangleTerrainTypes(types);

		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = HexMetrics.TerraceLerp(begin, left, i);
			v4 = HexMetrics.TerraceLerp(begin, right, i);
			c3 = HexMetrics.TerraceLerp(color1, color2, i);
			c4 = HexMetrics.TerraceLerp(color1, color3, i);
			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2, c3, c4);
			terrain.AddQuadTerrainTypes(types);
		}

		terrain.AddQuad(v3, v4, left, right);
		terrain.AddQuadColor(c3, c4, color2, color3);
		terrain.AddQuadTerrainTypes(types);
		
	}

	void TriangulateCornerTerracesCliff (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {

		float b = 1f / (rightCell.Elevation - beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 boundary = Vector3.Lerp(begin, right, b);
		Color boundaryColor = Color.Lerp(color1, color3, b);
		Vector3 types;
		types.x = beginCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;

		TriangulateBoundaryTriangle(begin, color1, left, color2, boundary, boundaryColor, types);

		if (leftCell.GetEdgeType(rightCell) == EdgeType.Terrace) {
			TriangulateBoundaryTriangle(left, color2, right, color3, boundary, boundaryColor, types);
		}
		else {
			terrain.AddTriangle(left, right, boundary);	
			terrain.AddTriangleColor(color2, color3, boundaryColor);
			terrain.AddTriangleTerrainTypes(types);
		}
	}

	void TriangulateCornerCliffTerraces (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {

		float b = 1f / (leftCell.Elevation - beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 boundary = Vector3.Lerp(begin, left, b);
		Color boundaryColor = Color.Lerp(color1, color2, b);
		Vector3 types;
		types.x = beginCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;

		TriangulateBoundaryTriangle(right, color3, begin, color1, boundary, boundaryColor, types);

		if (leftCell.GetEdgeType(rightCell) == EdgeType.Terrace) {
			TriangulateBoundaryTriangle(left, color2, right, color3, boundary, boundaryColor, types);
		}
		else {
			terrain.AddTriangle(left, right, boundary);
			terrain.AddTriangleColor(color2, color3, boundaryColor);
			terrain.AddTriangleTerrainTypes(types);
		}
	}

	void TriangulateBoundaryTriangle (
		Vector3 begin, Color beginColor,
		Vector3 left, Color leftColor,
		Vector3 boundary, Color boundaryColor, Vector3 types
	) {	

		Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
		Color c2 = HexMetrics.TerraceLerp(beginColor, leftColor, 1);

		terrain.AddTriangle(begin, v2, boundary);
		terrain.AddTriangleColor(beginColor, c2, boundaryColor);
		terrain.AddTriangleTerrainTypes(types);

		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v2;
			Color c1 = c2;
			v2 = HexMetrics.TerraceLerp(begin, left, i);
			c2 = HexMetrics.TerraceLerp(beginColor, leftColor, i);
			terrain.AddTriangle(v1, v2, boundary);
			terrain.AddTriangleColor(c1, c2, boundaryColor);
			terrain.AddTriangleTerrainTypes(types);
		}
		
		terrain.AddTriangle(v2, left, boundary);
		terrain.AddTriangleColor(c2, leftColor, boundaryColor);
		terrain.AddTriangleTerrainTypes(types);
	}


	void TriangulateWater (
		HexDirection direction, HexCell cell, Vector3 center
	) {
		center.y = cell.WaterSurfaceY;

		HexCell neighbor = cell.GetNeighbor(direction);
		if (neighbor != null && !neighbor.IsUnderwater) {
			TriangulateWaterShore(direction, cell, neighbor, center);
		}
		else {
			TriangulateOpenWater(direction, cell, neighbor, center);
		}
	}

	void TriangulateOpenWater (HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center) {
		Vector3 c1 = center + HexMetrics.GetFirstWaterCorner(direction);
		Vector3 c2 = center + HexMetrics.GetSecondWaterCorner(direction);

		water.AddTriangle(center, c1, c2);

		if (direction <= HexDirection.SE && neighbor != null) {
			
			Vector3 bridge = HexMetrics.GetWaterBridge(direction);
			Vector3 e1 = c1 + bridge;
			Vector3 e2 = c2 + bridge;

			water.AddQuad(c1, c2, e1, e2);

			if (direction <= HexDirection.E) {
				HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
				if (nextNeighbor == null || !nextNeighbor.IsUnderwater) {
					return;
				}
				water.AddTriangle(
					c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next())
				);
			}
		}
		
	}

	void TriangulateWaterShore (HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center){
		Vector3 corner1 = center + HexMetrics.GetFirstWaterCorner(direction);
		Vector3 corner2 = center + HexMetrics.GetSecondWaterCorner(direction);
		water.AddTriangle(center, corner1,corner2);
		
		//Vector3 bridge = HexMetrics.GetWaterBridge(direction);
		Vector3 center2 = neighbor.Position;
		center2.y = center.y;
		Vector3 bridge1 = center2 + HexMetrics.GetSecondSolidCorner(direction.Opposite());
		Vector3 bridge2 = center2 + HexMetrics.GetFirstSolidCorner(direction.Opposite());

		//Vector3 bridge1 = corner1 + bridge;
		//Vector3 bridge2 = corner2 + bridge;
		waterShore.AddQuad(corner1,corner2,bridge1,bridge2);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);
		
		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (nextNeighbor != null) {
			Vector3 v3 = nextNeighbor.Position + (nextNeighbor.IsUnderwater ?
				HexMetrics.GetFirstWaterCorner(direction.Previous()) :
				HexMetrics.GetFirstSolidCorner(direction.Previous()));
			v3.y = center.y;

			waterShore.AddTriangle(corner2, bridge2, v3);
			waterShore.AddTriangleUV(
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f)
			);
		}
	}

	
	void TriangulateEdgeFan (Vector3 center, EdgeVertices edge, float type) {
		terrain.AddTriangle(center, edge.v1, edge.v2);
		terrain.AddTriangleColor(color1);

		Vector3 types;
		types.x = types.y = types.z = type;
		terrain.AddTriangleTerrainTypes(types);
	}

	void TriangulateEdgeStrip (EdgeVertices e1, Color c1, float type1, EdgeVertices e2, Color c2, float type2) {
		terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
		terrain.AddQuadColor(c1, c2);

		Vector3 types;
		types.x = types.z = type1;
		types.y = type2;
		terrain.AddQuadTerrainTypes(types);
	}

}
