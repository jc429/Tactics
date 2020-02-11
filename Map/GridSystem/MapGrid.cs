using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapGrid : MonoBehaviour
{

	bool started = false;
	/* size of map in cells */
	/* maps must currently be in a multiple of chunk sizes (4X, 4Z) */
	public int cellCountX = 16, cellCountY = 16;
	int chunkCountX, chunkCountY;
	MapShape mapShape = MapShape.Rect;
	int mapRadius;		//for use with circular maps 
	public int cellCountTotal = 0;
	//where the center of the map is located
	MapCoordinates centerCoords;	//x = half of radius, z = radius - 1 

	public MapCell cellPrefab;
	public TextMeshPro cellLabelPrefab;
	public MapGridChunk chunkPrefab;

	MapGridChunk[] chunks;
	MapCell[] cells;
	
	public Color[] colors;

	//priority queue for cell pathfinding
	CellPriorityQueue searchFrontier;

	int searchFrontierPhase;

	/* the currently highlighted path */
	MapCell currentPathFrom, currentPathTo;
	bool currentPathExists;
	public bool HasPath {
		get {
			return currentPathExists;
		}
	}

	/* all units on the map */
	List<MapUnit> units = new List<MapUnit>();

	// Start is called before the first frame update
	void Start()
	{
			
	}

	// Update is called once per frame
	void Update()
	{
			
	}
}
