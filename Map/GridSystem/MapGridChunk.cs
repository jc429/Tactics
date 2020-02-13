using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGridChunk : MonoBehaviour
{
	MapCell[] cellsArray;
	List<MapCell> cellsList;

	public CellMesh terrain, water, waterShore;
	Canvas gridCanvas;

	bool refreshQueued = false;

	/* colors for splat map */
	static Color colorR = new Color(1f, 0f, 0f, 0f);
	static Color colorG = new Color(0f, 1f, 0f, 0f);
	static Color colorB = new Color(0f, 0f, 1f, 0f);
	/* splat mid tones*/ 
	static Color colorRG = new Color(0.5f, 0.5f, 0f, 0f);
	static Color colorRB = new Color(0.5f, 0f, 0.5f, 0f);
	static Color colorGB = new Color(0f, 0.5f, 0.5f, 0f);
	static Color colorRA = new Color(0.5f, 0f, 0f, 0.5f);
	static Color colorBA = new Color(0f, 0.5f, 0f, 0.5f);
	static Color colorGA = new Color(0f, 0f, 0.5f, 0.5f);
	static Color colorRGB = new Color(0.3333f, 0.3333f, 0.3333f, 0f);
	static Color colorRGBA = new Color(0.25f, 0.25f, 0.25f, 0.25f);


	void Awake () {
		gridCanvas = GetComponentInChildren<Canvas>();

		cellsArray = new MapCell[QuadMetrics.chunkSizeX * QuadMetrics.chunkSizeY];
		cellsList = new List<MapCell>();
	}
	
	void Start () {
		TriangulateAll();
	}

	/* refresh during LateUpdate to avoid errors */
	void LateUpdate(){
		if(refreshQueued){
			TriangulateAll();
			refreshQueued = false;
		}
	}

	/* assigns a cell to this chunk */
	public void AddCell (MapCell cell) {
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

		foreach(MapCell c in cellsList){
			if(!c.invalid){
				TriangulateCell(c);
			}
		}

		terrain.Apply();
		water.Apply();
		waterShore.Apply();
	
	}

	/* Creates the 6 triangles that comprise a hex cell */
	void TriangulateCell(MapCell cell){
		for (QuadDirection d = QuadDirection.N; d <= QuadDirection.W; d++) {
			Triangulate(d, cell);
		}
	}

	/* Creates a single triangle */
	void Triangulate(QuadDirection direction, MapCell cell){
	
		Vector3 center = cell.transform.localPosition;
		EdgeVertices e = new EdgeVertices(
			center + QuadMetrics.GetFirstSolidCorner(direction),
			center + QuadMetrics.GetSecondSolidCorner(direction)
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
		if (direction <= QuadDirection.E) {
			TriangulateConnection(direction, cell, e);
		}

		if (cell.IsUnderwater) {
			TriangulateWater(direction, cell, center);
		}
	}

	/* triangulates the joints between tiles */
	void TriangulateConnection(QuadDirection direction, MapCell cell, EdgeVertices e1){

		MapCell neighbor = cell.GetNeighbor(direction);
		if(neighbor == null){
			return;
		}

		// quad stretching to edge of tile
		Vector3 bridge = QuadMetrics.GetBridge(direction);
		bridge.y = neighbor.Position.y - cell.Position.y;
		EdgeVertices e2 = new EdgeVertices(
			e1.v1 + bridge,
			e1.v2 + bridge
		);

		
		TriangulateEdgeStrip(e1, colorR, cell.TerrainTypeIndex, e2, colorG, neighbor.TerrainTypeIndex);
		/*
		if (cell.GetEdgeType(direction) == EdgeType.Slope) {
			TriangulateEdgeTerraces(e1, cell, e2, neighbor);
		}
		else {
			TriangulateEdgeStrip(e1, color1, cell.TerrainTypeIndex, e2, color2, neighbor.TerrainTypeIndex);
		}
		*/

		/* unlike hex maps, square map cells have 3 neighbors to deal with at a given corner */
		// TODO: fill in corners
		MapCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction == QuadDirection.N && nextNeighbor != null) {
			Vector3 bridge2 = QuadMetrics.GetBridge(direction.Next());
			bridge2.y = nextNeighbor.Position.y - cell.Position.y;

			MapCell neighbor4 = nextNeighbor.GetNeighbor(direction);
			Vector3 bridge3 = QuadMetrics.GetBridge(direction);
			bridge3.y = neighbor4.Position.y - nextNeighbor.Position.y;

			TriangulateSquareCorner(
				e1.v2, cell,
				e2.v2, neighbor,
				e1.v2 + bridge2, nextNeighbor,
				e1.v2 + bridge2 + bridge3, neighbor4
			);
		}

		/*
		// remaining corner triangles
		MapCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction <= QuadDirection.E && nextNeighbor != null) {
			Vector3 v5 = e1.v2 + QuadMetrics.GetBridge(direction.Next());
			v5.y = nextNeighbor.Elevation * QuadMetrics.elevationStep;

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
		*/
	}

	void TriangulateEdgeTerraces(
		EdgeVertices begin, MapCell beginCell,
		EdgeVertices end, MapCell endCell
	) {
        
		EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
		Color c2 = QuadMetrics.TerraceLerp(colorR, colorG, 1);
		float t1 = beginCell.TerrainTypeIndex;
		float t2 = endCell.TerrainTypeIndex;

		TriangulateEdgeStrip(begin, colorR, t1, e2, c2, t2);

		for (int i = 2; i < QuadMetrics.terraceSteps; i++) {
			EdgeVertices e1 = e2;
			Color c1 = c2;
			e2 = EdgeVertices.TerraceLerp(begin, end, i);
			//c2 = QuadMetrics.TerraceLerp(beginCell.color, endCell.color, i);
			c2 = QuadMetrics.TerraceLerp(colorR, colorG, i);
			TriangulateEdgeStrip(e1, c1, t1, e2, c2, t2);
		}

		TriangulateEdgeStrip(e2, c2, t1, end, colorG, t2);
	}

	/* for square cells*/
	void TriangulateSquareCorner ( 
		Vector3 inner, MapCell innerCell, 
		Vector3 left, MapCell leftCell,
		Vector3 right, MapCell rightCell,
		Vector3 outer, MapCell outerCell
	) {
		Vector3 midIL = inner + (left - inner)*0.5f;
		Vector3 midIR = inner + (right - inner)*0.5f;
		Vector3 midLO = left + (outer - left)*0.5f;
		Vector3 midRO = right + (outer - right)*0.5f;
		Vector3 center = inner + (outer - inner)*0.5f;
		
		Vector4 types = new Vector4();


		/*** Outer Triangles ***/
		terrain.AddTriangle(inner, midIL, midIR);
		terrain.AddTriangleColor(colorR, colorRG, colorRB);
		types.x = innerCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;
		terrain.AddTriangleTerrainTypes(types);

		terrain.AddTriangle(left, midLO, midIL);
		terrain.AddTriangleColor(colorR, colorRG, colorRB);
		types.x = leftCell.TerrainTypeIndex;
		types.y = outerCell.TerrainTypeIndex;
		types.z = innerCell.TerrainTypeIndex;
		terrain.AddTriangleTerrainTypes(types);
		
		terrain.AddTriangle(right, midIR, midRO);
		terrain.AddTriangleColor(colorR, colorRG, colorRB);
		types.x = rightCell.TerrainTypeIndex;
		types.y = innerCell.TerrainTypeIndex;
		types.z = outerCell.TerrainTypeIndex;
		terrain.AddTriangleTerrainTypes(types);
		
		terrain.AddTriangle(outer, midRO, midLO);
		terrain.AddTriangleColor(colorR, colorRG, colorRB);
		types.x = outerCell.TerrainTypeIndex;
		types.y = rightCell.TerrainTypeIndex;
		types.z = leftCell.TerrainTypeIndex;
		terrain.AddTriangleTerrainTypes(types);


		/*** Inner Triangles ***/
		terrain.AddTriangle(midIL, center, midIR);
		terrain.AddTriangleColor(colorRG, colorRGBA, colorRB);
		types.x = innerCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;
		types.w = outerCell.TerrainTypeIndex;
		terrain.AddTriangleTerrainTypes(types);

		terrain.AddTriangle(midLO, center, midIL);
		terrain.AddTriangleColor(colorRG, colorRGBA, colorRB);
		types.x = leftCell.TerrainTypeIndex;
		types.y = outerCell.TerrainTypeIndex;
		types.z = innerCell.TerrainTypeIndex;
		types.w = rightCell.TerrainTypeIndex;
		terrain.AddTriangleTerrainTypes(types);

		terrain.AddTriangle(midRO, center, midLO);
		terrain.AddTriangleColor(colorRG, colorRGBA, colorRB);
		types.x = outerCell.TerrainTypeIndex;
		types.y = rightCell.TerrainTypeIndex;
		types.z = leftCell.TerrainTypeIndex;
		types.w = innerCell.TerrainTypeIndex;
		terrain.AddTriangleTerrainTypes(types);

		terrain.AddTriangle(midIR, center, midRO);
		terrain.AddTriangleColor(colorRG, colorRGBA, colorRB);
		types.x = rightCell.TerrainTypeIndex;
		types.y = innerCell.TerrainTypeIndex;
		types.z = outerCell.TerrainTypeIndex;
		types.w = leftCell.TerrainTypeIndex;
		terrain.AddTriangleTerrainTypes(types);

		/*
		terrain.AddTriangle(midIL, midLO, midIR);
		terrain.AddTriangleColor(colorR, colorG, colorB);
		types.x = ((float)innerCell.TerrainTypeIndex + (float)leftCell.TerrainTypeIndex)*0.5f;
		types.y = ((float)outerCell.TerrainTypeIndex + (float)leftCell.TerrainTypeIndex)*0.5f;
		types.z = ((float)innerCell.TerrainTypeIndex + (float)rightCell.TerrainTypeIndex)*0.5f;
		terrain.AddTriangleTerrainTypes(types);

		terrain.AddTriangle(midRO, midIR, midLO);
		terrain.AddTriangleColor(colorRB, colorG, colorRG);
		types.x = outerCell.TerrainTypeIndex;
		types.y = rightCell.TerrainTypeIndex;
		types.z = leftCell.TerrainTypeIndex;
		terrain.AddTriangleTerrainTypes(types);
		*/

	}


	void TriangulateCorner (
		Vector3 bottom, MapCell bottomCell,
		Vector3 left, MapCell leftCell,
		Vector3 right, MapCell rightCell
	) {
		EdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
		EdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

		if (leftEdgeType == EdgeType.Slope) {
			if (rightEdgeType == EdgeType.Slope) {
				TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
			else if (rightEdgeType == EdgeType.Flat) {
				TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
			}
			else{
				TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		else if (rightEdgeType == EdgeType.Slope) {
			if (leftEdgeType == EdgeType.Flat) {
				TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
			}
			else{
				TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		else if (leftCell.GetEdgeType(rightCell) == EdgeType.Slope) {
			if (leftCell.Elevation < rightCell.Elevation) {
				TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
			}
			else {
				TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
			}
		}
		else{ 
			terrain.AddTriangle(bottom, left, right);
			terrain.AddTriangleColor(colorR, colorG, colorB);
			Vector3 types;
			types.x = bottomCell.TerrainTypeIndex;
			types.y = leftCell.TerrainTypeIndex;
			types.z = rightCell.TerrainTypeIndex;
			terrain.AddTriangleTerrainTypes(types);
		}
	}

	void TriangulateCornerTerraces (
		Vector3 begin, MapCell beginCell,
		Vector3 left, MapCell leftCell,
		Vector3 right, MapCell rightCell
	) {
		
		Vector3 v3 = QuadMetrics.TerraceLerp(begin, left, 1);
		Vector3 v4 = QuadMetrics.TerraceLerp(begin, right, 1);
		Color c3 = QuadMetrics.TerraceLerp(colorR, colorG, 1);
		Color c4 = QuadMetrics.TerraceLerp(colorR, colorB, 1);
		Vector3 types;
		types.x = beginCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;

		terrain.AddTriangle(begin, v3, v4);
		terrain.AddTriangleColor(beginCell.CellColor, c3, c4);
		terrain.AddTriangleTerrainTypes(types);

		for (int i = 2; i < QuadMetrics.terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = QuadMetrics.TerraceLerp(begin, left, i);
			v4 = QuadMetrics.TerraceLerp(begin, right, i);
			c3 = QuadMetrics.TerraceLerp(colorR, colorG, i);
			c4 = QuadMetrics.TerraceLerp(colorR, colorB, i);
			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2, c3, c4);
			terrain.AddQuadTerrainTypes(types);
		}

		terrain.AddQuad(v3, v4, left, right);
		terrain.AddQuadColor(c3, c4, colorG, colorB);
		terrain.AddQuadTerrainTypes(types);
		
	}

	void TriangulateCornerTerracesCliff (
		Vector3 begin, MapCell beginCell,
		Vector3 left, MapCell leftCell,
		Vector3 right, MapCell rightCell
	) {

		float b = 1f / (rightCell.Elevation - beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 boundary = Vector3.Lerp(begin, right, b);
		Color boundaryColor = Color.Lerp(colorR, colorB, b);
		Vector3 types;
		types.x = beginCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;

		TriangulateBoundaryTriangle(begin, colorR, left, colorG, boundary, boundaryColor, types);

		if (leftCell.GetEdgeType(rightCell) == EdgeType.Slope) {
			TriangulateBoundaryTriangle(left, colorG, right, colorB, boundary, boundaryColor, types);
		}
		else {
			terrain.AddTriangle(left, right, boundary);	
			terrain.AddTriangleColor(colorG, colorB, boundaryColor);
			terrain.AddTriangleTerrainTypes(types);
		}
	}

	void TriangulateCornerCliffTerraces (
		Vector3 begin, MapCell beginCell,
		Vector3 left, MapCell leftCell,
		Vector3 right, MapCell rightCell
	) {

		float b = 1f / (leftCell.Elevation - beginCell.Elevation);
		if (b < 0) {
			b = -b;
		}
		Vector3 boundary = Vector3.Lerp(begin, left, b);
		Color boundaryColor = Color.Lerp(colorR, colorG, b);
		Vector3 types;
		types.x = beginCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;

		TriangulateBoundaryTriangle(right, colorB, begin, colorR, boundary, boundaryColor, types);

		if (leftCell.GetEdgeType(rightCell) == EdgeType.Slope) {
			TriangulateBoundaryTriangle(left, colorG, right, colorB, boundary, boundaryColor, types);
		}
		else {
			terrain.AddTriangle(left, right, boundary);
			terrain.AddTriangleColor(colorG, colorB, boundaryColor);
			terrain.AddTriangleTerrainTypes(types);
		}
	}

	void TriangulateBoundaryTriangle (
		Vector3 begin, Color beginColor,
		Vector3 left, Color leftColor,
		Vector3 boundary, Color boundaryColor, Vector3 types
	) {	

		Vector3 v2 = QuadMetrics.TerraceLerp(begin, left, 1);
		Color c2 = QuadMetrics.TerraceLerp(beginColor, leftColor, 1);

		terrain.AddTriangle(begin, v2, boundary);
		terrain.AddTriangleColor(beginColor, c2, boundaryColor);
		terrain.AddTriangleTerrainTypes(types);

		for (int i = 2; i < QuadMetrics.terraceSteps; i++) {
			Vector3 v1 = v2;
			Color c1 = c2;
			v2 = QuadMetrics.TerraceLerp(begin, left, i);
			c2 = QuadMetrics.TerraceLerp(beginColor, leftColor, i);
			terrain.AddTriangle(v1, v2, boundary);
			terrain.AddTriangleColor(c1, c2, boundaryColor);
			terrain.AddTriangleTerrainTypes(types);
		}
		
		terrain.AddTriangle(v2, left, boundary);
		terrain.AddTriangleColor(c2, leftColor, boundaryColor);
		terrain.AddTriangleTerrainTypes(types);
	}


	void TriangulateWater (
		QuadDirection direction, MapCell cell, Vector3 center
	) {
		center.y = cell.WaterSurfaceY;

		MapCell neighbor = cell.GetNeighbor(direction);
		if (neighbor != null && !neighbor.IsUnderwater) {
			TriangulateWaterShore(direction, cell, neighbor, center);
		}
		else {
			TriangulateOpenWater(direction, cell, neighbor, center);
		}
	}

	void TriangulateOpenWater (QuadDirection direction, MapCell cell, MapCell neighbor, Vector3 center) {
		Vector3 c1 = center + QuadMetrics.GetFirstWaterCorner(direction);
		Vector3 c2 = center + QuadMetrics.GetSecondWaterCorner(direction);

		water.AddTriangle(center, c1, c2);

		if (direction <= QuadDirection.E && neighbor != null) {
			
			Vector3 bridge = QuadMetrics.GetWaterBridge(direction);
			Vector3 e1 = c1 + bridge;
			Vector3 e2 = c2 + bridge;

			water.AddQuad(c1, c2, e1, e2);

			if (direction <= QuadDirection.E) {
				MapCell nextNeighbor = cell.GetNeighbor(direction.Next());
				if (nextNeighbor == null || !nextNeighbor.IsUnderwater) {
					return;
				}
				water.AddTriangle(
					c2, e2, c2 + QuadMetrics.GetWaterBridge(direction.Next())
				);
			}
		}
		
	}

	void TriangulateWaterShore (QuadDirection direction, MapCell cell, MapCell neighbor, Vector3 center){
		Vector3 corner1 = center + QuadMetrics.GetFirstWaterCorner(direction);
		Vector3 corner2 = center + QuadMetrics.GetSecondWaterCorner(direction);
		water.AddTriangle(center, corner1,corner2);
		
		//Vector3 bridge = QuadMetrics.GetWaterBridge(direction);
		Vector3 center2 = neighbor.Position;
		center2.y = center.y;
		Vector3 bridge1 = center2 + QuadMetrics.GetSecondSolidCorner(direction.Opposite());
		Vector3 bridge2 = center2 + QuadMetrics.GetFirstSolidCorner(direction.Opposite());

		//Vector3 bridge1 = corner1 + bridge;
		//Vector3 bridge2 = corner2 + bridge;
		waterShore.AddQuad(corner1,corner2,bridge1,bridge2);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);
		
		MapCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (nextNeighbor != null) {
			Vector3 v3 = nextNeighbor.Position + (nextNeighbor.IsUnderwater ?
				QuadMetrics.GetFirstWaterCorner(direction.Previous()) :
				QuadMetrics.GetFirstSolidCorner(direction.Previous()));
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
		terrain.AddTriangleColor(colorR);

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
