using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
	HexCell[] cells;

	public HexMesh terrain, water, waterShore;
	Canvas gridCanvas;

	bool refreshQueued = false;

	void Awake () {
		gridCanvas = GetComponentInChildren<Canvas>();

		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
		ShowUI(false);
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
	public void AddCell (int index, HexCell cell) {
		cells[index] = cell;
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

        foreach(HexCell c in cells){
            TriangulateCell(c);
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
		Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
		Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        // solid core triangle
        terrain.AddTriangle(center, v1, v2);
        terrain.AddTriangleColor(cell.CellColor);

        if (direction <= HexDirection.SE) {
			TriangulateConnection(direction, cell, v1, v2);
		}

		if (cell.IsUnderwater) {
			TriangulateWater(direction, cell, center);
		}
    }

    /* triangulates the joints between tiles */
    void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2){

        HexCell neighbor = cell.GetNeighbor(direction);
        if(neighbor == null){
            return;
        }

        // quad stretching to edge of tile
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
		Vector3 v4 = v2 + bridge;
        v3.y = v4.y = neighbor.Elevation * HexMetrics.elevationStep;

		if (cell.GetEdgeType(direction) == HexEdgeType.Slope) {
			TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
		}
		else {
			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(cell.CellColor, neighbor.CellColor);
		}

        // remaining corner triangles
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction <= HexDirection.E && nextNeighbor != null) {
			Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
			v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;

			//start from lowest elevation
			if (cell.Elevation <= neighbor.Elevation) {
				if (cell.Elevation <= nextNeighbor.Elevation) {
					TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
				}
				else {
					TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
				}
			}
			else if (neighbor.Elevation <= nextNeighbor.Elevation) {
				TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
			}
			else {
				TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
			}
		}
    }

    void TriangulateEdgeTerraces(
    	Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
		Vector3 endLeft, Vector3 endRight, HexCell endCell){
        
		Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
		Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
		Color c2 = HexMetrics.TerraceLerp(beginCell.CellColor, endCell.CellColor, 1);

		terrain.AddQuad(beginLeft, beginRight, v3, v4);
		terrain.AddQuadColor(beginCell.CellColor, c2);

		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c2;
			v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
			v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
			c2 = HexMetrics.TerraceLerp(beginCell.CellColor, endCell.CellColor, i);
			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2);
		}

		terrain.AddQuad(v3, v4, endLeft, endRight);
		terrain.AddQuadColor(c2, endCell.CellColor);
    }

	void TriangulateCorner (
		Vector3 bottom, HexCell bottomCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
		HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

		if (leftEdgeType == HexEdgeType.Slope) {
			if (rightEdgeType == HexEdgeType.Slope) {
				TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
			else if (rightEdgeType == HexEdgeType.Flat) {
				TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
			}
			else{
				TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		else if (rightEdgeType == HexEdgeType.Slope) {
			if (leftEdgeType == HexEdgeType.Flat) {
				TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
			}
			else{
				TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
			}
		}
		else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
			if (leftCell.Elevation < rightCell.Elevation) {
				TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
			}
			else {
				TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
			}
		}
		else{ 
			terrain.AddTriangle(bottom, left, right);
			terrain.AddTriangleColor(bottomCell.CellColor, leftCell.CellColor, rightCell.CellColor);
		}
	}

	void TriangulateCornerTerraces (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		
		Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
		Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
		Color c3 = HexMetrics.TerraceLerp(beginCell.CellColor, leftCell.CellColor, 1);
		Color c4 = HexMetrics.TerraceLerp(beginCell.CellColor, rightCell.CellColor, 1);

		terrain.AddTriangle(begin, v3, v4);
		terrain.AddTriangleColor(beginCell.CellColor, c3, c4);

		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = HexMetrics.TerraceLerp(begin, left, i);
			v4 = HexMetrics.TerraceLerp(begin, right, i);
			c3 = HexMetrics.TerraceLerp(beginCell.CellColor, leftCell.CellColor, i);
			c4 = HexMetrics.TerraceLerp(beginCell.CellColor, rightCell.CellColor, i);
			terrain.AddQuad(v1, v2, v3, v4);
			terrain.AddQuadColor(c1, c2, c3, c4);
		}

		terrain.AddQuad(v3, v4, left, right);
		terrain.AddQuadColor(c3, c4, leftCell.CellColor, rightCell.CellColor);
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
		Color boundaryColor = Color.Lerp(beginCell.CellColor, rightCell.CellColor, b);

		TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

		if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
			TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
		}
		else {
			terrain.AddTriangle(left, right, boundary);
			terrain.AddTriangleColor(leftCell.CellColor, rightCell.CellColor, boundaryColor);
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
		Color boundaryColor = Color.Lerp(beginCell.CellColor, leftCell.CellColor, b);

		TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

		if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
			TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
		}
		else {
			terrain.AddTriangle(left, right, boundary);
			terrain.AddTriangleColor(leftCell.CellColor, rightCell.CellColor, boundaryColor);
		}
	}

	void TriangulateBoundaryTriangle (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 boundary, Color boundaryColor
	) {	

		Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
		Color c2 = HexMetrics.TerraceLerp(beginCell.CellColor, leftCell.CellColor, 1);

		terrain.AddTriangle(begin, v2, boundary);
		terrain.AddTriangleColor(beginCell.CellColor, c2, boundaryColor);

		for (int i = 2; i < HexMetrics.terraceSteps; i++) {
			Vector3 v1 = v2;
			Color c1 = c2;
			v2 = HexMetrics.TerraceLerp(begin, left, i);
			c2 = HexMetrics.TerraceLerp(beginCell.CellColor, leftCell.CellColor, i);
			terrain.AddTriangle(v1, v2, boundary);
			terrain.AddTriangleColor(c1, c2, boundaryColor);
		}
		
		terrain.AddTriangle(v2, left, boundary);
		terrain.AddTriangleColor(c2, leftCell.CellColor, boundaryColor);
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

}
