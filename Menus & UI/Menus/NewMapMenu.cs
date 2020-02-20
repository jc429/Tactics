using UnityEngine;

public class NewMapMenu : MonoBehaviour {

	public MapGrid mapGrid;

	public void Open () {
		gameObject.SetActive(true);
		GameController.hexCamera.LockCamera();
	}

	public void Close () {
		gameObject.SetActive(false);
		GameController.hexCamera.UnlockCamera();
	}

	void CreateRectMap (int x, int z) {
		mapGrid.CreateMapRect(x, z);
		GameController.hexCamera.ValidatePosition();
		Close();
	}

	public void CreateSmallMap () {
		CreateRectMap(4, 4);
	}

	public void CreateMediumMap () {
		CreateRectMap(8, 8);
	}

	public void CreateLargeMap () {
		CreateRectMap(12, 12);
	}

	

}