using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MapShape{
	Rect,		//square-ish map based on x and z coords
	Circle		//semi-circular map based on a given radius
}

public class HexGrid : MonoBehaviour
{
	bool started = false;
	/* size of map in cells */
	/* maps must currently be in a multiple of chunk sizes (4X, 4Z) */
	public int cellCountX = 16, cellCountZ = 16;
	int chunkCountX, chunkCountZ;
	MapShape mapShape = MapShape.Rect;
	int mapRadius;		//for use with circular maps 
	public int cellCountTotal = 0;
	//where the center of the map is located
	HexCoordinates centerCoords;	//x = half of radius, z = radius - 1 

	public HexCell cellPrefab;
	public TextMeshPro cellLabelPrefab;
	public HexGridChunk chunkPrefab;

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
	List<MapUnit> units = new List<MapUnit>();

	public MapUnit unitPrefab;

    void Awake(){
		HexMetrics.colors = colors;
		MapUnit.unitPrefab = unitPrefab;
		GameController.hexGrid = this;

		CreateMapCircle(6);
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
	public bool CreateMapRect(int sizeX, int sizeZ){
		started = false;
		ClearPath();
		ClearUnits();
		mapShape = MapShape.Rect;
		mapRadius = 0;
		centerCoords = new HexCoordinates(sizeX / 2, sizeZ / 2);

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
		cellCountTotal = cellCountX * cellCountZ;

		chunkCountX = sizeX / HexMetrics.chunkSizeX;
		chunkCountZ = sizeZ / HexMetrics.chunkSizeZ;

		CreateChunks();
		CreateCells();

		return true;
	}

	/* generates a new map */
	public bool CreateMapCircle(int radius){
		started = false;
		ClearPath();
		ClearUnits();
		mapShape = MapShape.Circle;
		mapRadius = radius;
		centerCoords = new HexCoordinates(radius / 2, radius - 1);
		
		if (chunks != null) {
			for (int i = 0; i < chunks.Length; i++) {
				Destroy(chunks[i].gameObject);
			}
		}
		//just make a larger map and then disable certain cells
		cellCountX = (radius * 2) + 1;
		cellCountZ = (radius * 2) + 1;
		if(cellCountX % HexMetrics.chunkSizeX != 0){
			cellCountX += HexMetrics.chunkSizeX - (cellCountX % HexMetrics.chunkSizeX);
		}
		if(cellCountZ % HexMetrics.chunkSizeZ != 0){
			cellCountZ += HexMetrics.chunkSizeZ - (cellCountZ % HexMetrics.chunkSizeZ);
		}
		cellCountTotal = cellCountX * cellCountZ;
		chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;

			
		CreateChunks();
		CreateCells();

		//carve out the excess tiles to make this a hexagon
		foreach(HexCell cell in cells){
			int x = cell.coordinates.X;
			int z = cell.coordinates.Z;
			if((z >= (2 * radius) - 1) 							// top
			|| (x >= Mathf.Floor(1.5f * (float)radius))			//lower right
			|| ((x + z) >= (2 * radius) + (radius - 2)/2)		//upper right
			|| ((x + z) < (radius / 2))							//lower left
			|| (x <= -((float)radius * 0.5f))){					//upper left 

				cell.invalid = true;
				cell.SetLabel("");
			}
		}

		return true;
	}

	/* Generate the chunks */
	void CreateChunks () {
		switch(mapShape){
		case MapShape.Circle:
			/*//maybe split circular maps further one day but for now this should be fine
			chunks = new HexGridChunk[6];
			for(int i = 0; i < 6; i++){
				HexGridChunk chunk = chunks[i] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
			break;*/
		case MapShape.Rect:
		default:
			chunks = new HexGridChunk[chunkCountX * chunkCountZ];
			for (int z = 0, i = 0; z < chunkCountZ; z++) {
				for (int x = 0; x < chunkCountX; x++) {
					HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
					chunk.transform.SetParent(transform);
				}
			}
			break;
		}
	}

	/* Generate the cells that will populate the grid */
	void CreateCells () {
		switch(mapShape){
		case MapShape.Circle:
			/*cells = new HexCell[cellCountTotal];
			int ringLevel = 0;		//lv 0 = core cell
			int fillCells = 1;
			for(int cellNum = 0; cellNum < cellCountTotal; cellNum++){
				
				if(cellNum >= fillCells + (ringLevel * 6)){
					fillCells += ringLevel * 6;
					ringLevel++;
				}
		
				int cellRemainder = cellNum - fillCells;

				int x = 0;
				int z = 0;
				// cell 0 = (0,0,0)
				if(ringLevel > 0){
					//go clockwise radially around each ring
					if(cellRemainder == 0){
						x = 0;
						z = ringLevel;		//(0,r)
					}
					//if cellremainder <= ringlevel, approach (r,0)
					else if(cellRemainder <= ringLevel){
						x = cellRemainder;
						z = ringLevel - cellRemainder;
					}
					//if cellremainder <= 2 * ringlevel, approach (r,-r)
					else if(cellRemainder <= ringLevel * 2){
						x = ringLevel;
						z = ringLevel - cellRemainder;
					}
					//if cellremainder <= 3 * ringlevel, approach (0,-r)
					else if(cellRemainder <= ringLevel * 3){
						x = (ringLevel * 3) - cellRemainder;
						z = -ringLevel;
					}
					//if cellremainder <= 4 * ringlevel, approach (-r,0)
					else if(cellRemainder < ringLevel * 4){
						x = ringLevel;
						z = cellRemainder - (ringLevel * 4);
					}
					//if cellremainder <= 5 * ringlevel, approach (-r,r)
					else if(cellRemainder <= ringLevel * 5){
						x = -ringLevel;
						z = cellRemainder - (ringLevel * 4);
					}
					//if cellremainder < 6 * ringlevel, approach (0,r) 
					else if(cellRemainder < ringLevel * 6){
						x = cellRemainder - (ringLevel * 5);
						z = ringLevel;
					}
				}
				Debug.Log("x: " + x + ", z: " + z);
				
				CreateCell(x, z, cellNum);
			}
			break;*/
		case MapShape.Rect:
		default: 
			cells = new HexCell[cellCountX * cellCountZ];
			for(int z = 0, i = 0; z < cellCountZ; z++){
				for(int x = 0; x < cellCountX; x++){
					CreateCell(x,z,i++);
				}
			}
			break;
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
		//cell.coordinates = new HexCoordinates(x,z);	

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
        TextMeshPro label = Instantiate<TextMeshPro>(cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);
		//label.text = cell.coordinates.ToString();

        cell.uiRect = label.rectTransform;

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
    }

	/* what it says on the box */
	void AddCellToChunk (int x, int z, HexCell cell) {
		HexGridChunk chunk;
		switch(mapShape){
		case MapShape.Circle:
			/*int y = -x - z;
			int cnum = 0;
			if(x >= 0){
				if(z >= 0){
					cnum = 0;
				}
				else if(z < 0){
					if( y < 0){
						cnum = 1;
					}
					else if( y >= 0){
						cnum = 2;
					}
				}
			}
			else if(x < 0){
				if(z < 0){
					cnum = 3;
				}
				else if(z >= 0){
					if( y >= 0){
						cnum = 4;
					}
					else if( y < 0){
						cnum = 5;
					}
				}
			}
			chunk = chunks[cnum];
			chunk.AddCell(cell);

			break;*/
		case MapShape.Rect:
		default:
			//location of parent chunk
			int chunkX = x / HexMetrics.chunkSizeX;
			int chunkZ = z / HexMetrics.chunkSizeZ;
			chunk = chunks[chunkX + chunkZ * chunkCountX];

			// location of cell within chunk
			int localX = x - chunkX * HexMetrics.chunkSizeX;
			int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
			chunk.AddCell(/*localX + localZ * HexMetrics.chunkSizeX, */cell);
			
			break;
		}
		//cell.SetLabel(cell.coordinates.ToString());
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

	public HexCoordinates GetCenterCoordinates(){
		return centerCoords;
	}

	/* show or hide the labels on each cell */
	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	/* returns true if a given unit can enter a given tile */
	bool CheckValidCellEntry(HexCell cell, MapUnit unit){
		if(cell == null || unit == null){
			return false;
		}
		if(cell.Unit != null){
			return false;
		}
		if(cell.IsUnderwater 
		&& (unit.Properties.movementClass != MovementClass.Flying
			&& unit.Properties.movementClass != MovementClass.Aquatic
		)){
			return false;
		}
		return true;
	}

	/* returns true if a given unit can cross a cell boundary */
	bool CheckValidBoundaryCrossing(HexCell cell, HexCell neighbor, MapUnit unit){
		if(cell == null || neighbor == null || unit == null){
			return false;
		}
		if (cell.GetEdgeType(neighbor) == HexEdgeType.Cliff) {
			return false;
		}
		return true;
	}


	/* calculates all possible cells that can be reached by a given unit from a given tile */
	public void CalculateMovementRange(HexCell start, MapUnit unit){
		if(unit == null){
			return;
		}

		for (int i = 0; i < cells.Length; i++) {
			cells[i].DistanceToCell = int.MaxValue;
		}

		// cells we can move to
		if(unit.moveTiles == null){
			unit.moveTiles = ListPool<HexCell>.Get();
		}
		else{
			unit.moveTiles.Clear();
		}
		
		Queue<HexCell> frontier = new Queue<HexCell>();
		start.DistanceToCell = 0;
		frontier.Enqueue(start);

		while (frontier.Count > 0) {
			HexCell current = frontier.Dequeue();		

			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
				if(!CheckValidCellEntry(neighbor,unit)){
					continue;
				}
				if(!CheckValidBoundaryCrossing(current,neighbor,unit)){
					continue;
				}
				
				int moveCost = GameProperties.MovementProperties.GetCostToEnter(neighbor.Terrain, unit.Properties.movementClass);
				if(moveCost == 0){
					continue;
				}

				int distance = current.DistanceToCell + moveCost;
				if(distance > unit.MovementRange()){
					continue;
				}

				//from this point we know we can enter the cell

				if(!unit.moveTiles.Contains(neighbor)){
					unit.moveTiles.Add(neighbor);
					frontier.Enqueue(neighbor);
				}

				if (distance < neighbor.DistanceToCell){
					neighbor.DistanceToCell = distance;
					neighbor.PathParent = current;
				}
				
			}
		}
	}

	/* all tiles a unit can attack from their starting tile */
	public void CalculateTotalAttackRange(MapUnit unit){
		if(unit == null){
			return;
		}

		if(unit.attackTiles == null){
			unit.attackTiles = ListPool<HexCell>.Get();
		}
		else{
			unit.attackTiles.Clear();
		}

		int unitAttackRange = unit.Properties.AttackRange;

		if(unitAttackRange <= 0){
			//Debug.Log("Unit cannot attack!");
			return;
		}

		foreach(HexCell cell in unit.moveTiles){
			CheckCellsWithinRange(cell,unit.attackTiles,unitAttackRange);
		}
	}

	/* after we have moved, which tiles can a unit attack from where they're currently standing */
	public void CalculateLocalAttackRange(HexCell cell, MapUnit unit){
		if(cell == null || unit == null){
			return;
		}

		if(unit.localAttackTiles == null){
			unit.localAttackTiles = ListPool<HexCell>.Get();
		}
		else{
			unit.localAttackTiles.Clear();
		}
		
		int unitAttackRange = unit.Properties.AttackRange;
		
		if(unitAttackRange <= 0){
			Debug.Log("Unit cannot attack!");
		}

		CheckCellsWithinRange(cell,unit.localAttackTiles,unitAttackRange);

	}

	/* takes all cells within range of a given cell and appends them to a list (works for 1 or 2 range) */
	void CheckCellsWithinRange(HexCell sourceCell, List<HexCell> validList, int range){
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
			if(range == 1){
				//melee attack range is easy
				HexCell neighbor = sourceCell.GetNeighbor(d);
				if(neighbor != null){
					if(!validList.Contains(neighbor)){
						validList.Add(neighbor);
					}
				}
			}
			if(range == 2){
				HexCell neighbor = sourceCell.GetNeighbor(d);
				if(neighbor != null){
					HexCell neighbor2 = neighbor.GetNeighbor(d);
					if(neighbor2 != null){
						if(!validList.Contains(neighbor2)){
							validList.Add(neighbor2);
						}
					}
					neighbor2 = neighbor.GetNeighbor(d.Next());
					if(neighbor2 != null){
						if(!validList.Contains(neighbor2)){
							validList.Add(neighbor2);
						}
					}
					neighbor2 = neighbor.GetNeighbor(d.Previous());
					if(neighbor2 != null){
						if(!validList.Contains(neighbor2)){
							validList.Add(neighbor2);
						}
					}
				}
			}
		}
	}

	/* checks tiles in a straight line */
	public int CalculateLinearActionRange(HexCell startCell, HexDirection direction, int maxDistance){
		if(startCell == null){
			return 0;
		}
		HexCell currentCell = startCell;
		int range = 0;
		for(int i = 0; i < maxDistance; i++){
			HexCell neighbor = currentCell.GetNeighbor(direction);
			if(neighbor != null){
				if (currentCell.GetEdgeType(neighbor) == HexEdgeType.Cliff) {
					break;
				}
				else{
					range++;
					currentCell = neighbor;
					continue;
				}
			}
			else{
				break;
			}
		}
		return range;
	}

	/* creates a list of tiles in a straight line */
	public List<HexCell> GetLinearActionCells(HexCell startCell, HexDirection direction, int maxDistance){
		if(startCell == null){
			return null;
		}
		List<HexCell> actionCells = ListPool<HexCell>.Get();
		HexCell currentCell = startCell;
		int range = 0;
		for(int i = 0; i < maxDistance; i++){
			HexCell neighbor = currentCell.GetNeighbor(direction);
			if(neighbor != null){
				if (currentCell.GetEdgeType(neighbor) == HexEdgeType.Cliff) {
					break;
				}
				else{
					actionCells.Add(neighbor);
					range++;
					currentCell = neighbor;
					continue;
				}
			}
			else{
				break;
			}
		}
		return actionCells;
	}

	/* finds a direct path between two tiles */
	public void FindPath (HexCell fromCell, HexCell toCell, MapUnit unit) {
		ClearPath();
		if(fromCell == null || toCell == null || unit == null){
			return;
		}
		currentPathFrom = fromCell;
		currentPathTo = toCell;
		
		
		currentPathExists = CellSearch(fromCell, toCell, unit);
		ShowPath();
	}

	/* depth-first search (A*) - finds a path between two cells relatively quickly */
	bool CellSearch (HexCell fromCell, HexCell toCell, MapUnit unit){
		searchFrontierPhase += 2;
		if (searchFrontier == null) {
			searchFrontier = new HexCellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		
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
				if(!CheckValidCellEntry(neighbor,unit)){
					continue;
				}
				if(!CheckValidBoundaryCrossing(current,neighbor,unit)){
					continue;
				}

				int moveCost = GameProperties.MovementProperties.GetCostToEnter(neighbor.Terrain, unit.Properties.movementClass);
				if(moveCost == 0){
					continue;
				}
				
				int distance = current.DistanceToCell + moveCost;
				if(distance > unit.MovementRange()){
					//path is too long - cant calculate
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
		currentPathFrom.colorFlags.OnMovementPath = true;
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
				current.colorFlags.OnMovementPath = true;
				current = current.PathParent;
			}
		}
		currentPathFrom.colorFlags.OnMovementPath = true;
		currentPathTo.colorFlags.OnMovementPath = true;
	}

	/* clear current path */
	public void ClearPath () {
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel(null);
				current.colorFlags.OnMovementPath = false;
				current = current.PathParent;
			}
			current.colorFlags.OnMovementPath = false;
			currentPathExists = false;
		}
		else if (currentPathFrom != null) {
			currentPathFrom.colorFlags.OnMovementPath = false;
			currentPathTo.colorFlags.OnMovementPath = false;
			currentPathFrom.colorFlags.IsSelected = false;
			currentPathTo.colorFlags.IsSelected = false;
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
	public void AddUnit (MapUnit unit, HexCell location, DodecDirection direction) {
		units.Add(unit);
		unit.transform.SetParent(transform, false);
		unit.CurrentCell = location;
		unit.Facing = direction;
	}

	/* remove unit */
	public void RemoveUnit (MapUnit unit) {
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
		writer.Write(mapRadius);
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
		//Debug.Log("Creating Grid of size " + x + "x" + z);
		if (x != cellCountX || z != cellCountZ) {
			if (!CreateMapRect(x, z)) {
				return;
			}
		}
		
		mapRadius = reader.ReadInt32();
		if(mapRadius > 0){
			mapShape = MapShape.Circle;
			centerCoords = new HexCoordinates(mapRadius / 2, mapRadius - 1);
		}
		else{
			mapShape = MapShape.Rect;
			centerCoords = new HexCoordinates( x / 2, z / 2);
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
				MapUnit.Load(reader, this);
			}
		}

		GameController.mapCamera.ResetZoomAndCenterCamera();
	}
}
