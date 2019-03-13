using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{

    Mesh hexMesh;
    List<Vector3> vertices;
    List<int> triangles;
    List<Color> colors;

    MeshCollider meshCollider;

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
    }


    /* Triangulates All Cells */
    public void TriangulateAll(HexCell[] cells){
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();

        foreach(HexCell c in cells){
            TriangulateCell(c);
        }

        hexMesh.vertices = vertices.ToArray();
		hexMesh.triangles = triangles.ToArray();
        hexMesh.colors = colors.ToArray();

		hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
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
        AddTriangle(center, v1, v2);
        AddTriangleColor(cell.color);

        if (direction <= HexDirection.SE) {
			TriangulateConnection(direction, cell, v1, v2);
		}

    }

    void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2){

        HexCell neighbor = cell.GetNeighbor(direction);
        if(neighbor == null){
            return;
        }

        // quad stretching to edge of tile
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
		Vector3 v4 = v2 + bridge;

		AddQuad(v1, v2, v3, v4);
        AddQuadColor(cell.color, neighbor.color);

        // remaining corner triangles
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction <= HexDirection.E && nextNeighbor != null) {
			AddTriangle(v2, v4, v2 + HexMetrics.GetBridge(direction.Next()));
			AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
		}
    }


    /* Creates a triangle from three points and adds it to the lists */
    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3){
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
    }

    /* Colors a triangle a solid color */
    void AddTriangleColor(Color c){
        colors.Add(c);
        colors.Add(c);
        colors.Add(c);
    }

    /* colors each vertex of a triangle a different color */
    void AddTriangleColor (Color c1, Color c2, Color c3) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}

    /* Creates a quad */
    void AddQuad (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		vertices.Add(v4);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
	}

    /* Colors each vertex of a quad  */
	void AddQuadColor (Color c1, Color c2, Color c3, Color c4) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
		colors.Add(c4);
	}

    /* Colors two edges of a quad */
    void AddQuadColor (Color c1, Color c2) {
		colors.Add(c1);
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c2);
	}
}
