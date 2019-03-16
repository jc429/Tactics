using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    Mesh hexMesh;
	[System.NonSerialized] List<Vector3> vertices;
	[System.NonSerialized] List<Color> colors;
	[System.NonSerialized] List<int> triangles;

    MeshCollider meshCollider;
	public bool useCollider;
	public bool useColors;

	public bool useUVCoordinates;
	[System.NonSerialized] List<Vector2> uvs;

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        if (useCollider) {
			meshCollider = gameObject.AddComponent<MeshCollider>();
		}
        hexMesh.name = "Hex Mesh";
		
    }

	public void ClearAll() {
		hexMesh.Clear();
		vertices = ListPool<Vector3>.Get();
		if (useColors) {
			colors = ListPool<Color>.Get();
		}
		if (useUVCoordinates) {
			uvs = ListPool<Vector2>.Get();
		}
		triangles = ListPool<int>.Get();
	}

	public void Apply () {
		hexMesh.SetVertices(vertices);
		ListPool<Vector3>.Add(vertices);
		if(useColors){
			hexMesh.SetColors(colors);
			ListPool<Color>.Add(colors);
		}
		if (useUVCoordinates) {
			hexMesh.SetUVs(0, uvs);
			ListPool<Vector2>.Add(uvs);
		}
		hexMesh.SetTriangles(triangles, 0);
		ListPool<int>.Add(triangles);
		hexMesh.RecalculateNormals();
		if (useCollider) {
			meshCollider.sharedMesh = hexMesh;
		}
	}


    /* Creates a triangle from three points and adds it to the lists */
    public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3){
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
    }

    /* Colors a triangle a solid color */
    public void AddTriangleColor(Color c){
        colors.Add(c);
        colors.Add(c);
        colors.Add(c);
    }

    /* colors each vertex of a triangle a different color */
    public void AddTriangleColor (Color c1, Color c2, Color c3) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}

	/* Adds UVs to a triangle */
	public void AddTriangleUV (Vector2 uv1, Vector2 uv2, Vector3 uv3) {
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
	}

    /* Creates a quad */
    public void AddQuad (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
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
	public void AddQuadColor (Color c1, Color c2, Color c3, Color c4) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
		colors.Add(c4);
	}

    /* Colors two edges of a quad */
    public void AddQuadColor (Color c1, Color c2) {
		colors.Add(c1);
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c2);
	}
	
	/* adds UVs to a quad */
	public void AddQuadUV (Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4) {
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
		uvs.Add(uv4);
	}

	public void AddQuadUV (float uMin, float uMax, float vMin, float vMax) {
		uvs.Add(new Vector2(uMin, vMin));
		uvs.Add(new Vector2(uMax, vMin));
		uvs.Add(new Vector2(uMin, vMax));
		uvs.Add(new Vector2(uMax, vMax));
	}
}
