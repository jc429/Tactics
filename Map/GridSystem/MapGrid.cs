using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapGrid : MonoBehaviour
{

	/* texture to use to generate noise in QuadMetrics*/
	public Texture2D noiseSource;


	bool started = false;
	/* size of map in cells */
	/* maps must currently be in a multiple of chunk sizes (4X, 4Y) */
	public int cellCountX = 16, cellCountY = 16;
	int chunkCountX, chunkCountY;
	MapShape mapShape = MapShape.Rect;
	public int cellCountTotal = 0;
	//where the center of the map is located

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

	public MapUnit unitPrefab;
	/* all units on the map */
	List<MapUnit> units = new List<MapUnit>();

	void Awake(){
		QuadMetrics.noiseSource = noiseSource;
		QuadMetrics.colors = colors;
		MapUnit.unitPrefab = unitPrefab;
		GameController.mapGrid = this;

	}

	void OnEnable () {
		QuadMetrics.noiseSource = noiseSource;
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
	public bool CreateMapRect(int sizeX, int sizeY, bool debugMode = false){
		started = false;
		ClearPath();
		ClearUnits();
		mapShape = MapShape.Rect;

		if (
			sizeX <= 0 || sizeX % QuadMetrics.chunkSizeX != 0 ||
			sizeY <= 0 || sizeY % QuadMetrics.chunkSizeY != 0
		) {
			Debug.LogError("Unsupported map size: " + sizeX + "x" + sizeY);
			return false;
		}

		if (chunks != null) {
			for (int i = 0; i < chunks.Length; i++) {
				Destroy(chunks[i].gameObject);
			}
		}
		cellCountX = sizeX;
		cellCountY = sizeY;
		cellCountTotal = cellCountX * cellCountY;

		chunkCountX = sizeX / QuadMetrics.chunkSizeX;
		chunkCountY = sizeY / QuadMetrics.chunkSizeY;

		CreateChunks();
		CreateCells();

		return true;
	}

	/* Generate the chunks */
	void CreateChunks () {
		switch(mapShape){
		case MapShape.Circle:
			Debug.Log("ERROR: Unsupported Map Shape");
			break;
		case MapShape.Rect:
		default:
			chunks = new MapGridChunk[chunkCountX * chunkCountY];
			for (int z = 0, i = 0; z < chunkCountY; z++) {
				for (int x = 0; x < chunkCountX; x++) {
					MapGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
					chunk.transform.SetParent(transform);
				}
			}
			break;
		}
	}

	/* Generate the cells that will populate the grid */
	void CreateCells () {
		cells = new MapCell[cellCountX * cellCountY];
		for(int y = 0, i = 0; y < cellCountY; y++){
			for(int x = 0; x < cellCountX; x++){
				CreateCell(x,y,i++);
			}
		}
	}

	/* Creates and labels a Map Cell */
	MapCell CreateCell(int x, int y, int i, bool forceHeight = false, int height = 0){
		Vector3 pos;
		pos.x = x * QuadMetrics.cellWidth;
		pos.y = 0;
		pos.z = y * QuadMetrics.cellWidth;

		MapCell cell = cells[i] = Instantiate<MapCell>(cellPrefab);
		cell.name = "Cell  (" + x + ", " + y +")";
    //	cell.transform.SetParent(transform,false);
		cell.transform.localPosition = pos;
		cell.coordinates = new MapCoordinates(x,y);
		//cell.coordinates = new HexCoordinates(x,z);	

		//connect to neighbors
		if(x > 0){
			cell.SetNeighbor(QuadDirection.W, cells[i - 1]);
		}
		if(y > 0){
			cell.SetNeighbor(QuadDirection.S, cells[i - cellCountX]);
		}

		//label cell
		TextMeshPro label = Instantiate<TextMeshPro>(cellLabelPrefab);
		label.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);
		label.text = cell.coordinates.ToString();

		cell.uiRect = label.rectTransform;

		/* pretty randomization stuff */
		float r = Random.Range(0f,1f);
		if(forceHeight){
			cell.TerrainTypeIndex = (int)Random.Range(0,4);
			cell.Elevation = height;
		}
		else if(r > 0.4f){
			MapCell w = cell.GetNeighbor(QuadDirection.W);
			MapCell s = cell.GetNeighbor(QuadDirection.S);
			if(w != null && s != null){
				r = Random.Range(0f,1f);
				if(r > 0.5f){
					cell.TerrainTypeIndex = w.TerrainTypeIndex;
					cell.Elevation = w.Elevation;
				}
				else{
					cell.TerrainTypeIndex = s.TerrainTypeIndex;
					cell.Elevation = s.Elevation;
				}
			}
			else if(w != null){
				cell.TerrainTypeIndex = w.TerrainTypeIndex;
				cell.Elevation = w.Elevation;
			}
			else if(s != null){
				cell.TerrainTypeIndex = s.TerrainTypeIndex;
				cell.Elevation = s.Elevation;
			}
			else{
				cell.TerrainTypeIndex = (int)Random.Range(0,4);
				cell.Elevation = (int)Random.Range(0,3);
			}
		}
		else{
			cell.TerrainTypeIndex = (int)Random.Range(0,4);
			cell.Elevation = (int)Random.Range(0,3);
		}

		AddCellToChunk(x, y, cell);

		return cell;
	}

	/* what it says on the box */
	void AddCellToChunk (int x, int y, MapCell cell) {
		MapGridChunk chunk;
		//location of parent chunk
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkY = y / HexMetrics.chunkSizeZ;
		chunk = chunks[chunkX + chunkY * chunkCountX];

		// location of cell within chunk
		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = y - chunkY * HexMetrics.chunkSizeZ;
		chunk.AddCell(cell);
	}

	/* returns a cell at a given position */
	public MapCell GetCell(Vector3 position) {
		position = transform.InverseTransformPoint(position);
		MapCoordinates c = new MapCoordinates(position);
		int index = c.X + (c.Y * cellCountX);
		return cells[index];
	}
	
	/* returns a cell at a given position */
	public MapCell GetCell(MapCoordinates c) {
		int index = c.X + (c.Y * cellCountX);
		return cells[index];
	}

	/* Returns a cell based on raycast */
	public MapCell GetCell (Ray ray) {
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			return GetCell(hit.point);
		}
		return null;
	}

	public MapCoordinates GetCenterCoordinates(){
		return new MapCoordinates(cellCountX / 2, cellCountY / 2);
	}

	/* show or hide the labels on each cell */
	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	/* returns true if a given unit can enter a given tile */
	bool CheckValidCellEntry(MapCell cell, MapUnit unit){
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
	bool CheckValidBoundaryCrossing(MapCell cell, MapCell neighbor, MapUnit unit){
		if(cell == null || neighbor == null || unit == null){
			return false;
		}
		if (cell.GetEdgeType(neighbor) == EdgeType.Cliff) {
			return false;
		}
		return true;
	}

	/* calculates all possible cells that can be reached by a given unit from a given tile */
	public void CalculateMovementRange(MapCell start, MapUnit unit){
		if(unit == null){
			return;
		}

		for (int i = 0; i < cells.Length; i++) {
			cells[i].DistanceToCell = int.MaxValue;
		}

		// cells we can move to
		if(unit.moveTiles == null){
			unit.moveTiles = ListPool<MapCell>.Get();
		}
		else{
			unit.moveTiles.Clear();
		}
		
		Queue<MapCell> frontier = new Queue<MapCell>();
		start.DistanceToCell = 0;
		frontier.Enqueue(start);

		while (frontier.Count > 0) {
			MapCell current = frontier.Dequeue();		

			for (QuadDirection d = QuadDirection.N; d <= QuadDirection.W; d++) {
				MapCell neighbor = current.GetNeighbor(d);
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
			unit.attackTiles = ListPool<MapCell>.Get();
		}
		else{
			unit.attackTiles.Clear();
		}

		int unitAttackRange = unit.Properties.AttackRange;

		if(unitAttackRange <= 0){
			//Debug.Log("Unit cannot attack!");
			return;
		}

		foreach(MapCell cell in unit.moveTiles){
			CheckCellsWithinRange(cell,unit.attackTiles,unitAttackRange);
		}
	}

	/* after we have moved, which tiles can a unit attack from where they're currently standing */
	public void CalculateLocalAttackRange(MapCell cell, MapUnit unit){
		if(cell == null || unit == null){
			return;
		}

		if(unit.localAttackTiles == null){
			unit.localAttackTiles = ListPool<MapCell>.Get();
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
	void CheckCellsWithinRange(MapCell sourceCell, List<MapCell> validList, int range){
		for (QuadDirection d = QuadDirection.N; d <= QuadDirection.E; d++) {
			if(range == 1){
				//melee attack range is easy
				MapCell neighbor = sourceCell.GetNeighbor(d);
				if(neighbor != null){
					if(!validList.Contains(neighbor)){
						validList.Add(neighbor);
					}
				}
			}
			if(range == 2){
				MapCell neighbor = sourceCell.GetNeighbor(d);
				if(neighbor != null){
					MapCell neighbor2 = neighbor.GetNeighbor(d);
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

	/* finds a direct path between two tiles */
	public void FindPath (MapCell fromCell, MapCell toCell, MapUnit unit) {
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
	bool CellSearch (MapCell fromCell, MapCell toCell, MapUnit unit){
		searchFrontierPhase += 2;
		if (searchFrontier == null) {
			searchFrontier = new CellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		
		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.DistanceToCell = 0;
		searchFrontier.Enqueue(fromCell);
		
		while (searchFrontier.Count > 0) {
			MapCell current = (MapCell)searchFrontier.Dequeue();
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

			for (QuadDirection d = QuadDirection.N; d <= QuadDirection.W; d++) {
				MapCell neighbor = current.GetNeighbor(d);
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
			/*foreach(MapCell cell in cells){
				if(currentPathFrom.Unit && cell.DistanceToCell < currentPathFrom.Unit.moveRange){
					cell.SetLabel(cell.DistanceToCell.ToString());
				}
				else{
					cell.SetLabel("");
				}
			}*/

			MapCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel(current.DistanceToCell.ToString());
				current.colorFlags.OnMovementPath = true;
				current = (MapCell)current.PathParent;
			}
		}
		currentPathFrom.colorFlags.OnMovementPath = true;
		currentPathTo.colorFlags.OnMovementPath = true;
	}

	/* clear current path */
	public void ClearPath () {
		if (currentPathExists) {
			MapCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel(null);
				current.colorFlags.OnMovementPath = false;
				current = (MapCell)current.PathParent;
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
	public List<MapCell> GetPath () {
		if (!currentPathExists) {
			return null;
		}
		List<MapCell> path = ListPool<MapCell>.Get();
		for (MapCell c = currentPathTo; c != currentPathFrom; c = (MapCell)c.PathParent) {
			path.Add(c);
		}
		path.Add(currentPathFrom);
		path.Reverse();
		return path;
	}


	/* add a new unit */
	public void AddUnit (MapUnit unit, MapCell location, OctDirection direction) {
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
		writer.Write(cellCountY);
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
		mapShape = MapShape.Rect;
		ClearPath();
		ClearUnits();
		int x = reader.ReadInt32(); 
		int z = reader.ReadInt32();
		//Debug.Log("Creating Grid of size " + x + "x" + z);
		if (x != cellCountX || z != cellCountY) {
			if (!CreateMapRect(x, z)) {
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
				MapUnit.Load(reader, this);
			}
		}

		//GameController.mapCamera.ResetZoomAndCenterCamera();
	}

}
