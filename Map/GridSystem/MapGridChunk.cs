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

	/* splatColors for splat map */
	static Color splatColorR = new Color(1f, 0f, 0f, 0f);
	static Color splatColorG = new Color(0f, 1f, 0f, 0f);
	static Color splatColorB = new Color(0f, 0f, 1f, 0f);
	static Color splatColorA = new Color(0f, 0f, 0f, 1f);
	/* splat mid tones*/ 
	static Color splatColorRG = new Color(0.5f, 0.5f, 0f, 0f);
	static Color splatColorRB = new Color(0.5f, 0f, 0.5f, 0f);
	static Color splatColorGB = new Color(0f, 0.5f, 0.5f, 0f);
	static Color splatColorRA = new Color(0.5f, 0f, 0f, 0.5f);
	static Color splatColorGA = new Color(0f, 0.5f, 0f, 0.5f);
	static Color splatColorBA = new Color(0f, 0f, 0.5f, 0.5f);
	static Color splatColorRGB = new Color(0.3333f, 0.3333f, 0.3333f, 0f);
	static Color splatColorRGBA = new Color(0.25f, 0.25f, 0.25f, 0.25f);


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
		terrain.AddTriangleColor(splatColor1);
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

		
		//TriangulateEdgeStrip(e1, splatColorR, cell.TerrainTypeIndex, e2, splatColorG, neighbor.TerrainTypeIndex);
		
		if (cell.GetEdgeType(direction) == EdgeType.Slope) {
			TriangulateEdgeTerraces(e1, cell, e2, neighbor);
		}
		else {
			TriangulateEdgeStrip(e1, splatColorR, cell.TerrainTypeIndex, e2, splatColorG, neighbor.TerrainTypeIndex);
		}
		

		/* unlike hex maps, square map cells have 3 neighbors to deal with at a given corner */
		// TODO: fill in corners
		MapCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction == QuadDirection.N && nextNeighbor != null) {
			Vector3 bridge2 = QuadMetrics.GetBridge(direction.Next());
			bridge2.y = nextNeighbor.Position.y - cell.Position.y;

			MapCell neighbor4 = nextNeighbor.GetNeighbor(direction);
			Vector3 bridge3 = QuadMetrics.GetBridge(direction);
			bridge3.y = neighbor4.Position.y - nextNeighbor.Position.y;

			//start from one of the lowest cells
			if(cell.Elevation <= neighbor.Elevation
			&& cell.Elevation <= nextNeighbor.Elevation
			&& cell.Elevation <= neighbor4.Elevation){
				TriangulateSquareCorner(
					e1.v2, cell, 
					e2.v2, neighbor,
					e1.v2 + bridge2, nextNeighbor,
					e1.v2 + bridge2 + bridge3, neighbor4
				);
			}
			else if(neighbor.Elevation <= cell.Elevation
			&& neighbor.Elevation <= nextNeighbor.Elevation
			&& neighbor.Elevation <= neighbor4.Elevation){
				TriangulateSquareCorner(
					e2.v2, neighbor,
					e1.v2 + bridge2 + bridge3, neighbor4,
					e1.v2, cell,
					e1.v2 + bridge2, nextNeighbor
				);
			}
			else if(nextNeighbor.Elevation <= cell.Elevation
			&& nextNeighbor.Elevation <= neighbor.Elevation
			&& nextNeighbor.Elevation <= neighbor4.Elevation){
				TriangulateSquareCorner(
					e1.v2 + bridge2, nextNeighbor,
					e1.v2, cell,
					e1.v2 + bridge2 + bridge3, neighbor4,
					e2.v2, neighbor
				);
			}
			else{
				TriangulateSquareCorner(
					e1.v2 + bridge2 + bridge3, neighbor4,
					e1.v2 + bridge2, nextNeighbor,
					e2.v2, neighbor,
					e1.v2, cell
				);
			}
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
		Color c2 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, 1);
		float t1 = beginCell.TerrainTypeIndex;
		float t2 = endCell.TerrainTypeIndex;

		TriangulateEdgeStrip(begin, splatColorR, t1, e2, c2, t2);

		for (int i = 2; i < QuadMetrics.terraceSteps; i++) {
			EdgeVertices e1 = e2;
			Color c1 = c2;
			e2 = EdgeVertices.TerraceLerp(begin, end, i);
			//c2 = QuadMetrics.TerraceLerp(beginCell.color, endCell.color, i);
			c2 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, i);
			TriangulateEdgeStrip(e1, c1, t1, e2, c2, t2);
		}

		TriangulateEdgeStrip(e2, c2, t1, end, splatColorG, t2);
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
		float shortest = Mathf.Min(inner.y, left.y, right.y, outer.y);
		float tallest  = Mathf.Max(inner.y, left.y, right.y, outer.y);
		center.y = shortest + (tallest - shortest)*0.5f;
		
		
		Vector4 types = new Vector4();
		types.x = innerCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;
		types.w = outerCell.TerrainTypeIndex;


	/*** New Method: 8 way intersection ***/ 

		//terrace wrapping around a corner 
		if(innerCell.GetEdgeType(leftCell) == EdgeType.Slope
		&& innerCell.GetEdgeType(rightCell) == EdgeType.Slope
		&& leftCell.Elevation == rightCell.Elevation){
			if(innerCell.Elevation < leftCell.Elevation
			&& innerCell.Elevation < outerCell.Elevation){
				TriangulateSquareCornerTerraceWrap(
					inner, innerCell, 
					left, leftCell,
					right, rightCell,
					outer, outerCell
				);
			}
			else if(innerCell.Elevation < leftCell.Elevation
			&& innerCell.Elevation == outerCell.Elevation){
				TriangulateSquareCornerTerraceValley(
					left, leftCell,
					outer, outerCell,
					inner, innerCell, 
					right, rightCell
				);
			}
			return;
		}
		else if(outerCell.GetEdgeType(leftCell) == EdgeType.Slope
		&& outerCell.GetEdgeType(rightCell) == EdgeType.Slope
		&& leftCell.Elevation == rightCell.Elevation){
			if(outerCell.Elevation > leftCell.Elevation
			&& outerCell.Elevation > innerCell.Elevation){
				TriangulateSquareCornerTerraceWrap(
					outer, outerCell,
					right, rightCell,
					left, leftCell,
					inner, innerCell 
				);
			}
			return;
		}
		
		// terrace bridges 
		if(innerCell.GetEdgeType(leftCell) == EdgeType.Slope
		&& rightCell.GetEdgeType(outerCell) == EdgeType.Slope){
			if(innerCell.Elevation == rightCell.Elevation 
			&& leftCell.Elevation == outerCell.Elevation){
				TriangulateSquareCornerTerraceBridge(
					inner, innerCell, 
					left, leftCell,
					right, rightCell,
					outer, outerCell
				);
				return;
			}
		}
		else if(innerCell.GetEdgeType(rightCell) == EdgeType.Slope
		&& leftCell.GetEdgeType(outerCell) == EdgeType.Slope){
			if(innerCell.Elevation == leftCell.Elevation 
			&& rightCell.Elevation == outerCell.Elevation){
				TriangulateSquareCornerTerraceBridge(
					left, leftCell,
					outer, outerCell,
					inner, innerCell, 
					right, rightCell
				);
				return;
			}
		}

		// edge from inner cell to left cell
		if(innerCell.GetEdgeType(leftCell) == EdgeType.Slope){
			TriangulateSquareCornerTerraceToCenter(
				inner, innerCell, left, leftCell,
				center, rightCell, outerCell
			);
		}
		else {
			TriangulateSquareCornerCliff(
				inner, innerCell, left, leftCell,
				center, rightCell, outerCell
			);
		}
		
		// edge from left cell to outer cell
		if(leftCell.GetEdgeType(outerCell) == EdgeType.Slope){
			TriangulateSquareCornerTerraceToCenter(
				left, leftCell, outer, outerCell,
				center, innerCell, rightCell
			);
		}
		else {
			TriangulateSquareCornerCliff(
				left, leftCell, outer, outerCell,
				center, innerCell, rightCell
			);
		}

		// edge from outer cell to right cell 
		if(outerCell.GetEdgeType(rightCell) == EdgeType.Slope){
			TriangulateSquareCornerTerraceToCenter(
				outer, outerCell, right, rightCell,
				center, leftCell, innerCell
			);
		}
		else {
			TriangulateSquareCornerCliff(
				outer, outerCell, right, rightCell,
				center, leftCell, innerCell
			);
		}
		
		// edge from right cell to inner cell
		if(rightCell.GetEdgeType(innerCell) == EdgeType.Slope){
			TriangulateSquareCornerTerraceToCenter(
				right, rightCell, inner, innerCell,
				center, outerCell, leftCell
			);
		}
		else {
			TriangulateSquareCornerCliff(
				right, rightCell, inner, innerCell,
				center, outerCell, leftCell
			);
		}

	}


	/*** triangulates a square tile surrounded by terraces ***/
	void TriangulateSquareCornerTerraceWrap(
		Vector3 inner, MapCell innerCell,
		Vector3 left, MapCell leftCell,
		Vector3 right, MapCell rightCell,
		Vector3 outer, MapCell outerCell
	){
		
		Vector3 v1;
		Vector3 v2;
		Color c1;
		Color c2;
		Vector3 v3 = QuadMetrics.TerraceLerp(inner, left, 1);
		Vector3 v4 = QuadMetrics.TerraceLerp(inner, right, 1);
		Color c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, 1);
		Color c4 = QuadMetrics.TerraceLerp(splatColorR, splatColorB, 1);

		Vector4 types;
		types.x = innerCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;
		types.w = outerCell.TerrainTypeIndex;

		terrain.AddTriangle(inner, v3, v4);
		terrain.AddTriangleColor(splatColorR, c3, c4);
		terrain.AddTriangleTerrainTypes(types);

		for (int i = 2; i <= QuadMetrics.terraceSteps; i++) {
			v1 = v3;
			v2 = v4;
			c1 = c3;
			c2 = c4;
			v3 = QuadMetrics.TerraceLerp(inner, left, i);
			v4 = QuadMetrics.TerraceLerp(inner, right, i);
			c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, i);
			c4 = QuadMetrics.TerraceLerp(splatColorR, splatColorB, i);

			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2, c3, c4);
			terrain.AddQuadTerrainTypes(types);
		}

		if(outerCell.Elevation > leftCell.Elevation){
			v1 = v3;
			v2 = v4;
			c1 = c3;
			c2 = c4;
			v3 = QuadMetrics.TerraceLerp(left, outer, 1);
			v4 = QuadMetrics.TerraceLerp(right, outer, 1);
			c3 = QuadMetrics.TerraceLerp(splatColorG, splatColorA, 1);
			c4 = QuadMetrics.TerraceLerp(splatColorB, splatColorA, 1);

			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2, c3, c4);
			terrain.AddQuadTerrainTypes(types);

			for (int i = 2; i <= QuadMetrics.terraceSteps; i++) {
				v1 = v3;
				v2 = v4;
				c1 = c3;
				c2 = c4;
				v3 = QuadMetrics.TerraceLerp(left, outer, i);
				v4 = QuadMetrics.TerraceLerp(right, outer, i);
				c3 = QuadMetrics.TerraceLerp(splatColorG, splatColorA, i);
				c4 = QuadMetrics.TerraceLerp(splatColorB, splatColorA, i);

				terrain.AddQuad(v1, v2, v3, v4);
				terrain.AddQuadColor(c1, c2, c3, c4);
				terrain.AddQuadTerrainTypes(types);
			}

			terrain.AddTriangle(v3, v4, outer);
			terrain.AddTriangleColor(c3, c4, splatColorA);
			terrain.AddTriangleTerrainTypes(types);
		}
		else if(outerCell.Elevation == leftCell.Elevation){
			terrain.AddTriangle(outer, v4, v3);
			terrain.AddTriangleColor(splatColorA, c4, c3);
			terrain.AddTriangleTerrainTypes(types);
		}
	}


	/* terrace valley goes from a high tile across 2 low tiles back to a high tile  */ 
	void TriangulateSquareCornerTerraceValley(
		Vector3 inner, MapCell innerCell,
		Vector3 left, MapCell leftCell,
		Vector3 right, MapCell rightCell,
		Vector3 outer, MapCell outerCell
	){
		
		Vector3 v1 = Vector3.zero;
		Vector3 v2 = Vector3.zero;
		Color c1 = splatColorRGBA;
		Color c2 = splatColorRGBA;
		Vector3 v3 = QuadMetrics.TerraceLerp(inner, left, 1);
		Vector3 v4 = QuadMetrics.TerraceLerp(inner, right, 1);
		Color c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, 1);
		Color c4 = QuadMetrics.TerraceLerp(splatColorR, splatColorB, 1);

		Vector4 types;
		types.x = innerCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;
		types.w = outerCell.TerrainTypeIndex;


		// fall toward the middle of the intersection
		terrain.AddTriangle(inner, v3, v4);
		terrain.AddTriangleColor(splatColorR, c3, c4);
		terrain.AddTriangleTerrainTypes(types);

		for (int i = 2; i <= QuadMetrics.terraceSteps - 1; i++) {
			v1 = v3;
			v2 = v4;
			c1 = c3;
			c2 = c4;
			v3 = QuadMetrics.TerraceLerp(inner, left, i);
			v4 = QuadMetrics.TerraceLerp(inner, right, i);
			c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, i);
			c4 = QuadMetrics.TerraceLerp(splatColorR, splatColorB, i);

			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2, c3, c4);
			terrain.AddQuadTerrainTypes(types);
		}
		// skip reaching the very bottom of the valley for aesthetic reasons 

		// rise toward the next hill 
		v1 = v3;
		v2 = v4;
		c1 = c3;
		c2 = c4;


		v3 = QuadMetrics.TerraceLerp(left, outer, 1);
		v4 = QuadMetrics.TerraceLerp(right, outer, 1);
		c3 = QuadMetrics.TerraceLerp(splatColorG, splatColorA, 1);
		c4 = QuadMetrics.TerraceLerp(splatColorB, splatColorA, 1);
		
		terrain.AddTriangle(left, v3, v1);
		terrain.AddTriangleColor(splatColorG, c3, c1);
		terrain.AddTriangleTerrainTypes(types);

		terrain.AddTriangle(right, v2, v4);
		terrain.AddTriangleColor(splatColorB, c2, c4);
		terrain.AddTriangleTerrainTypes(types);

		terrain.AddQuad(v1, v2, v3, v4);
		terrain.AddQuadColor(c1, c2, c3, c4);
		terrain.AddQuadTerrainTypes(types);

		for (int i = 2; i <= QuadMetrics.terraceSteps; i++) {
			v1 = v3;
			v2 = v4;
			c1 = c3;
			c2 = c4;
			v3 = QuadMetrics.TerraceLerp(left, outer, i);
			v4 = QuadMetrics.TerraceLerp(right, outer, i);
			c3 = QuadMetrics.TerraceLerp(splatColorG, splatColorA, i);
			c4 = QuadMetrics.TerraceLerp(splatColorB, splatColorA, i);

			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2, c3, c4);
			terrain.AddQuadTerrainTypes(types);
		}

		terrain.AddTriangle(v3, v4, outer);
		terrain.AddTriangleColor(c3, c4, splatColorA);
		terrain.AddTriangleTerrainTypes(types);
		
	}

	/*** for terraces connecting to slopes ? ***/
	void TriangulateSquareCornerTerraceToCenter(
		Vector3 inner, MapCell innerCell,
		Vector3 left, MapCell leftCell,
		Vector3 center, 
		MapCell rightCell, MapCell outerCell
	){
		Vector3 v3 = QuadMetrics.TerraceLerp(inner, left, 1);
		Color c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, 1);

		Vector4 types;
		types.x = innerCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;
		types.w = outerCell.TerrainTypeIndex;

		terrain.AddTriangle(inner, v3, center);
		terrain.AddTriangleColor(splatColorR, c3, splatColorRGBA);
		terrain.AddTriangleTerrainTypes(types);

		for (int i = 2; i < QuadMetrics.terraceSteps; i++) {
			Vector3 v1 = v3;
			Color c1 = c3;
			v3 = QuadMetrics.TerraceLerp(inner, left, i);
			c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, i);
			terrain.AddTriangle(v1, v3, center);
			terrain.AddTriangleColor(c1, c3, splatColorRGBA);
			terrain.AddTriangleTerrainTypes(types);
		}
		
		terrain.AddTriangle(v3, left, center);
		terrain.AddTriangleColor(c3, splatColorG, splatColorRGBA);
		terrain.AddTriangleTerrainTypes(types);
	}

	void TriangulateSquareCornerCliff(
		Vector3 inner, MapCell innerCell,
		Vector3 left, MapCell leftCell,
		Vector3 center, 
		MapCell rightCell, MapCell outerCell
	){

		Vector3 midIL = inner + (left - inner)*0.5f;
		Vector4 types;
		types.x = innerCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;
		types.w = outerCell.TerrainTypeIndex;

		terrain.AddTriangle(inner, midIL, center);
		terrain.AddTriangleColor(splatColorR, splatColorRG, splatColorRGBA);
		terrain.AddTriangleTerrainTypes(types);
		
		terrain.AddTriangle(midIL, left, center);
		terrain.AddTriangleColor(splatColorRG, splatColorG, splatColorRGBA);
		terrain.AddTriangleTerrainTypes(types);
	}

	/* for corners that basically just continue edges */ 
	void TriangulateSquareCornerTerraceBridge(
		Vector3 lowerLeft, MapCell lowLeftCell,
		Vector3 upperLeft, MapCell upLeftCell,
		Vector3 lowerRight, MapCell lowRightCell,
		Vector3 upperRight, MapCell upRightCell
	){
		Vector3 v1 = lowerLeft;
		Vector3 v2 = lowerRight;
		Color c1 = splatColorR;
		Color c2 = splatColorB;
		Vector3 v3 = QuadMetrics.TerraceLerp(lowerLeft, upperLeft, 1);
		Vector3 v4 = QuadMetrics.TerraceLerp(lowerRight, upperRight, 1);
		Color c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, 1);
		Color c4 = QuadMetrics.TerraceLerp(splatColorB, splatColorA, 1);
		Vector4 types;
		types.x = lowLeftCell.TerrainTypeIndex;
		types.y = upLeftCell.TerrainTypeIndex;
		types.z = lowRightCell.TerrainTypeIndex;
		types.w = upRightCell.TerrainTypeIndex;

		terrain.AddQuad(v1, v2, v3, v4);
		terrain.AddQuadColor(c1, c2, c3, c4);
		terrain.AddQuadTerrainTypes(types);

		for (int i = 2; i <= QuadMetrics.terraceSteps; i++) {
			v1 = v3;
			v2 = v4;
			c1 = c3;
			c2 = c4;
			v3 = QuadMetrics.TerraceLerp(lowerLeft, upperLeft, i);
			v4 = QuadMetrics.TerraceLerp(lowerRight, upperRight, i);
			c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, i);
			c4 = QuadMetrics.TerraceLerp(splatColorB, splatColorA, i);

			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2, c3, c4);
			terrain.AddQuadTerrainTypes(types);
		}
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
			terrain.AddTriangleColor(splatColorR, splatColorG, splatColorB);
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
		Color c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, 1);
		Color c4 = QuadMetrics.TerraceLerp(splatColorR, splatColorB, 1);
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
			c3 = QuadMetrics.TerraceLerp(splatColorR, splatColorG, i);
			c4 = QuadMetrics.TerraceLerp(splatColorR, splatColorB, i);
			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2, c3, c4);
			terrain.AddQuadTerrainTypes(types);
		}

		terrain.AddQuad(v3, v4, left, right);
		terrain.AddQuadColor(c3, c4, splatColorG, splatColorB);
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
		Color boundaryColor = Color.Lerp(splatColorR, splatColorB, b);
		Vector3 types;
		types.x = beginCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;

		TriangulateBoundaryTriangle(begin, splatColorR, left, splatColorG, boundary, boundaryColor, types);

		if (leftCell.GetEdgeType(rightCell) == EdgeType.Slope) {
			TriangulateBoundaryTriangle(left, splatColorG, right, splatColorB, boundary, boundaryColor, types);
		}
		else {
			terrain.AddTriangle(left, right, boundary);	
			terrain.AddTriangleColor(splatColorG, splatColorB, boundaryColor);
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
		Color boundaryColor = Color.Lerp(splatColorR, splatColorG, b);
		Vector3 types;
		types.x = beginCell.TerrainTypeIndex;
		types.y = leftCell.TerrainTypeIndex;
		types.z = rightCell.TerrainTypeIndex;

		TriangulateBoundaryTriangle(right, splatColorB, begin, splatColorR, boundary, boundaryColor, types);

		if (leftCell.GetEdgeType(rightCell) == EdgeType.Slope) {
			TriangulateBoundaryTriangle(left, splatColorG, right, splatColorB, boundary, boundaryColor, types);
		}
		else {
			terrain.AddTriangle(left, right, boundary);
			terrain.AddTriangleColor(splatColorG, splatColorB, boundaryColor);
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
		terrain.AddTriangleColor(splatColorR);

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
