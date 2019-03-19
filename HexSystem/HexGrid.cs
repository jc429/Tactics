using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{

	/* size of map in cells */
	/* maps must currently be in a multiple of chunk sizes (4X, 4Z) */
	public int cellCountX = 16, cellCountZ = 16;
	int chunkCountX, chunkCountZ;

    [SerializeField]
    HexCell cellPrefab;
    [SerializeField]
    Text cellLabelPrefab;
	[SerializeField]
	HexGridChunk chunkPrefab;

	HexGridChunk[] chunks;
    HexCell[] cells;
	
	public Color[] colors;

	//priority queue for cell pathfinding
	HexCellPriorityQueue searchFrontier;

	int searchFrontierPhase;

	/* the currently highlighted path */
	HexCell currentPathFrom, currentPathTo;
	bool currentPathExists;


    void Awake(){
		HexMetrics.colors = colors;

		CreateMap(cellCountX,cellCountZ);
	}


	/* generates a new map */
	public bool CreateMap(int sizeX, int sizeZ){
		ClearPath();
		if (
			sizeX <= 0 || sizeX % HexMetrics.chunkSizeX != 0 ||
			sizeZ <= 0 || sizeZ % HexMetrics.chunkSizeZ != 0
		) {
			Debug.LogError("Unsupported map size: " + sizeX + "x" + sizeZ);
			return false;
		}

		if (chunks != null) {
			for (int i = 0; i < chunks.Length; i++) {
				Destroy(chunks[i].gameObject);
			}
		}
		cellCountX = sizeX;
		cellCountZ = sizeZ;

		chunkCountX = sizeX / HexMetrics.chunkSizeX;
		chunkCountZ = sizeZ / HexMetrics.chunkSizeZ;

		CreateChunks();
		CreateCells();

		return true;
	}


	/* Generate the chunks */
	void CreateChunks () {
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}

	/* Generate the cells that will populate the grid */
	void CreateCells () {
        cells = new HexCell[cellCountX * cellCountZ];

        for(int z = 0, i = 0; z < cellCountZ; z++){
            for(int x = 0; x < cellCountX; x++){
                CreateCell(x,z,i++);
            }
        }
    }

    /* Creates and labels a Hex Cell */
    void CreateCell(int x, int z, int i){
        Vector3 pos;
        pos.x = (x + (z * 0.5f - (z / 2))) * (HexMetrics.innerRadius * 2f);
        pos.y = 0;
        pos.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
    //	cell.transform.SetParent(transform,false);
        cell.transform.localPosition = pos;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x,z);

        //connect to neighbors
        if(x > 0){
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if(z > 0){
            if((z & 1) == 0){
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if(x > 0){
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else {
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1) {
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
			}
        }

        //label cell
        Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);
		//label.text = cell.coordinates.ToString();

        cell.uiRect = label.rectTransform;

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
    }

	/* what it says on the box */
	void AddCellToChunk (int x, int z, HexCell cell) {
		//location of parent chunk
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		// location of cell within chunk
		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}

    /* returns a cell at a given position */
    public HexCell GetCell(Vector3 position) {
		position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        return cells[index];
    }

	/* Returns a cell based on hex coordinates */
	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}

		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}

		return cells[x + z * cellCountX];
	}

	/* show or hide the labels on each cell */
	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	public void FindPath (HexCell fromCell, HexCell toCell, int movSteps) {
		ClearPath();
		currentPathFrom = fromCell;
		currentPathTo = toCell;
		currentPathExists = CellSearch(fromCell, toCell, movSteps);
		ShowPath();
	}

	/* depth-first search (A*) */
	bool CellSearch (HexCell fromCell, HexCell toCell, int movSteps){
		searchFrontierPhase += 2;
		if (searchFrontier == null) {
			searchFrontier = new HexCellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		/*for (int i = 0; i < cells.Length; i++) {
			cells[i].SetLabel(null);
			cells[i].DisableHighlight();
		}
		fromCell.EnableHighlight(Color.blue);
		//toCell.EnableHighlight(Color.red);
		*/
		
		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.DistanceToCell = 0;
		searchFrontier.Enqueue(fromCell);
		
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;

			if (current == toCell) {
				//current = current.PathParent;
				/*while (current != fromCell) {
					int turn = current.DistanceToCell;
					current.SetLabel(turn.ToString());
					current.EnableHighlight(Color.white);
					current = current.PathParent;
				}
				toCell.EnableHighlight(Color.red);
				break;*/
				return true;
			}

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
				if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase) {
					continue;
				}
				if (neighbor.IsUnderwater) {
					continue;
				}
				if (current.GetEdgeType(neighbor) == HexEdgeType.Cliff) {
					continue;
				}

				int moveCost;
				if (current.TerrainTypeIndex == (int)TerrainType.Road) {
					moveCost = 1;
				}
				else {
					moveCost = 2;
				}
				int distance = current.DistanceToCell + moveCost;
				
				//path is too long - cant calculate
				if(distance > movSteps){
					continue;
				}

				if(neighbor.SearchPhase < searchFrontierPhase){
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.DistanceToCell = distance;
					neighbor.PathParent = current;
					neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.DistanceToCell){
					int oldPriority = neighbor.SearchPriority;
					neighbor.DistanceToCell = distance;
					neighbor.PathParent = current;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return false;
	}

	/* display the found path */
	void ShowPath () {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel(current.DistanceToCell.ToString());
				current.EnableHighlight(Color.white);
				current = current.PathParent;
			}
		}
		currentPathFrom.EnableHighlight(Color.blue);
		currentPathTo.EnableHighlight(Color.red);
	}

	/* clear current path */
	void ClearPath () {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel(null);
				current.DisableHighlight();
				current = current.PathParent;
			}
			current.DisableHighlight();
			currentPathExists = false;
		}
		else if (currentPathFrom) {
			currentPathFrom.DisableHighlight();
			currentPathTo.DisableHighlight();
		}
		currentPathFrom = currentPathTo = null;
	}

	/* save the map */
	public void SaveGrid (BinaryWriter writer) {
		writer.Write(cellCountX);
		writer.Write(cellCountZ);
		for (int i = 0; i < cells.Length; i++) {
			cells[i].SaveCell(writer);
		}
	}

	public void LoadGrid (BinaryReader reader, int header) {
		ClearPath();
		int x = reader.ReadInt32(); 
		int z = reader.ReadInt32();
		Debug.Log("Creating Grid of size " + x + "x" + z);
		if (x != cellCountX || z != cellCountZ) {
			if (!CreateMap(x, z)) {
				return;
			}
		}

		for (int i = 0; i < cells.Length; i++) {
			cells[i].LoadCell(reader);
		}
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].Refresh();
		}
	}
}
