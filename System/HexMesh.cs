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

    /* creates the 6 triangles that comprise a hex cell */
    void TriangulateCell(HexCell cell){
        Vector3 center = cell.transform.localPosition;
        for(int i = 0; i < 6; i++){
            AddTriangle(
                center,
                center + HexMetrics.corners[i],
                center + HexMetrics.corners[i + 1]
            );
            AddTriangleColor(cell.color);
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

    /* Colors a triangle */
    void AddTriangleColor(Color c){
        colors.Add(c);
        colors.Add(c);
        colors.Add(c);
    }
}
