using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{

	/* size of map in cells */
	/* maps must currently be in a multiple of chunk sizes (5X, 5Z) */
	public int cellCountX = 10, cellCountZ = 10;
	int chunkCountX, chunkCountZ;

    [SerializeField]
    HexCell cellPrefab;
    [SerializeField]
    Text cellLabelPrefab;
	[SerializeField]
	HexGridChunk chunkPrefab;

	HexGridChunk[] chunks;
    HexCell[] cells;
	

    public Color defaultColor = Color.white;


    void Awake(){

		CreateMap(cellCountX,cellCountZ);
	}

	/* generates a new map */
	public void CreateMap(int sizeX, int sizeZ){
		if (
			sizeX <= 0 || sizeX % HexMetrics.chunkSizeX != 0 ||
			sizeZ <= 0 || sizeZ % HexMetrics.chunkSizeZ != 0
		) {
			Debug.LogError("Unsupported map size.");
			return;
		}

		if (chunks != null) {
			for (int i = 0; i < chunks.Length; i++) {
				Destroy(chunks[i].gameObject);
			}
		}

		chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;

		CreateChunks();
		CreateCells();
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
        cell.CellColor = defaultColor;

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
	//	label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);
		label.text = cell.coordinates.ToString();

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
}
