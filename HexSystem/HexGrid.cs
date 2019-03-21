using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
	bool started = false;
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
	public bool HasPath {
		get {
			return currentPathExists;
		}
	}

	/* all units on the map */
	List<HexUnit> units = new List<HexUnit>();

	public HexUnit unitPrefab;

    void Awake(){
		HexMetrics.colors = colors;
		HexUnit.unitPrefab = unitPrefab;
		GameController.hexGrid = this;

		CreateMap(cellCountX,cellCountZ);
	}

	public void StartMap(){
		if(started){
			return;
		}
		for (int i = 0; i < units.Count; i++) {
			units[i].StartUnit();
		}
		started = true;
	}

	/* generates a new map */
	public bool CreateMap(int sizeX, int sizeZ){
		started = false;
		ClearPath();
		ClearUnits();
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

	/* Returns a cell based on raycast */
	public HexCell GetCell (Ray ray) {
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			return GetCell(hit.point);
		}
		return null;
	}

	/* show or hide the labels on each cell */
	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	public void FindPath (HexCell fromCell, HexCell toCell, HexUnit unit) {
		ClearPath();
		currentPathFrom = fromCell;
		currentPathTo = toCell;
		
		currentPathExists = CellSearch(fromCell, toCell, unit);
		ShowPath();
	}

	/* calculates all possible cells that can be reached by a given unit from a given tile */
	public void CalculateMovementRange(HexCell start, HexUnit unit){
		if(unit == null || unit.moveTiles != null){
			return;
		}

		for (int i = 0; i < cells.Length; i++) {
			cells[i].DistanceToCell = int.MaxValue;
		}

		// cells we can move to
		List<HexCell> moveCells = ListPool<HexCell>.Get();
		
		Queue<HexCell> frontier = new Queue<HexCell>();
		start.DistanceToCell = 0;
		frontier.Enqueue(start);

		while (frontier.Count > 0) {
			HexCell current = frontier.Dequeue();		

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
				if (neighbor == null){
					continue;
				}
				if (neighbor.IsUnderwater || neighbor.Unit != null) {
					continue;
				}
				if (current.GetEdgeType(neighbor) == HexEdgeType.Cliff) {
					continue;
				}
				
				int moveCost = current.CostToEnter(unit);
				int distance = current.DistanceToCell + moveCost;

				if(distance > unit.moveRange){
					continue;
				}

				//from this point we know we have enough movement to reach the cell

				if(!moveCells.Contains(neighbor)){
					moveCells.Add(neighbor);
					frontier.Enqueue(neighbor);
				}

				if (distance < neighbor.DistanceToCell){
					neighbor.DistanceToCell = distance;
					neighbor.PathParent = current;
				}
				
			}
		}

		unit.moveTiles = moveCells;
	}

	/* all tiles a unit can attack from their starting tile */
	public void CaluclateTotalAttackRange(HexUnit unit){
		if(unit == null || unit.moveTiles == null){
			return;
		}

		List<HexCell> attackCells = ListPool<HexCell>.Get();
		int unitAttackRange = 1;	//TODO: allow other ranges

		foreach(HexCell cell in unit.moveTiles){
			/*if(attackCells.Contains(cell)){
				continue;
			}*/

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				//melee attack range is easy
				HexCell neighbor = cell.GetNeighbor(d);
				if(neighbor != null){
					if(!attackCells.Contains(neighbor)){
						attackCells.Add(neighbor);
					}
				}
			}
		}

		unit.attackTiles = attackCells;
	}

	/* depth-first search (A*) - finds a path between two cells relatively quickly */
	bool CellSearch (HexCell fromCell, HexCell toCell, HexUnit unit){
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
				if (neighbor.IsUnderwater || neighbor.Unit) {
					continue;
				}
				if (current.GetEdgeType(neighbor) == HexEdgeType.Cliff) {
					continue;
				}

				int moveCost = current.CostToEnter(unit);
				
				int distance = current.DistanceToCell + moveCost;
				
				//path is too long - cant calculate
				if(distance > unit.moveRange){
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
		currentPathFrom.OnMovementPath = true;
		if(currentPathFrom == currentPathTo){
			return;
		}
		if (currentPathExists) {
			//TODO: calc attack range and store in unit, then highlight attack range here 
			/*foreach(HexCell cell in cells){
				if(currentPathFrom.Unit && cell.DistanceToCell < currentPathFrom.Unit.moveRange){
					cell.SetLabel(cell.DistanceToCell.ToString());
				}
				else{
					cell.SetLabel("");
				}
			}*/

			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel(current.DistanceToCell.ToString());
				current.OnMovementPath = true;
				current = current.PathParent;
			}
		}
		currentPathFrom.OnMovementPath = true;
		currentPathTo.OnMovementPath = true;
	}

	/* clear current path */
	public void ClearPath () {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel(null);
				current.OnMovementPath = false;
				current = current.PathParent;
			}
			current.OnMovementPath = false;
			currentPathExists = false;
		}
		else if (currentPathFrom != null) {
			currentPathFrom.OnMovementPath = false;
			currentPathTo.OnMovementPath = false;
			currentPathFrom.IsSelected = false;
			currentPathTo.IsSelected = false;
		}
		currentPathFrom = currentPathTo = null;
	}

	/* return current movement path */
	public List<HexCell> GetPath () {
		if (!currentPathExists) {
			return null;
		}
		List<HexCell> path = ListPool<HexCell>.Get();
		for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathParent) {
			path.Add(c);
		}
		path.Add(currentPathFrom);
		path.Reverse();
		return path;
	}

	/* add a new unit */
	public void AddUnit (HexUnit unit, HexCell location, HexDirection direction) {
		units.Add(unit);
		unit.transform.SetParent(transform, false);
		unit.Location = location;
		unit.Facing = direction;
	}

	/* remove unit */
	public void RemoveUnit (HexUnit unit) {
		units.Remove(unit);
		unit.Die();
	}

	/* clear unit list */
	void ClearUnits () {
		for (int i = 0; i < units.Count; i++) {
			units[i].Die();
		}
		units.Clear();
	}

	/* save the map */
	public void SaveGrid (BinaryWriter writer) {
		writer.Write(cellCountX);
		writer.Write(cellCountZ);
		for (int i = 0; i < cells.Length; i++) {
			cells[i].SaveCell(writer);
		}
		writer.Write(units.Count);
		for (int i = 0; i < units.Count; i++) {
			units[i].Save(writer);
		}
	}

	public void LoadGrid (BinaryReader reader, int header) {
		started = false;
		ClearPath();
		ClearUnits();
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

		// save file version 1 adds unit support
		if (header >= 1) {
			int unitCount = reader.ReadInt32();
			for (int i = 0; i < unitCount; i++) {
				HexUnit.Load(reader, this);
			}
		}
	}
}
