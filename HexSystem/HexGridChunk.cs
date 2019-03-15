using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
	HexCell[] cells;

	HexMesh hexMesh;
	Canvas gridCanvas;

	bool refreshQueued = false;

	void Awake () {
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
		ShowUI(false);
	}
	
	void Start () {
		hexMesh.TriangulateAll(cells);
	}

	void LateUpdate(){
		if(refreshQueued){
			hexMesh.TriangulateAll(cells);
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
}
