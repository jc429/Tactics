using UnityEngine;

public class NewMapMenu : MonoBehaviour {

	public HexGrid hexGrid;

	public void Open () {
		gameObject.SetActive(true);
		GameController.mapCamera.LockCamera();
	}

	public void Close () {
		gameObject.SetActive(false);
		GameController.mapCamera.UnlockCamera();
	}

	void CreateRectMap (int x, int z) {
		hexGrid.CreateMapRect(x, z);
		GameController.mapCamera.ValidatePosition();
		Close();
	}

	public void CreateSmallMap () {
		CreateRectMap(8, 8);
	}

	public void CreateMediumMap () {
		CreateRectMap(16, 16);
	}

	public void CreateLargeMap () {
		CreateRectMap(24, 24);
	}

	void CreateCircleMap (int r) {
		hexGrid.CreateMapCircle(r);
		GameController.mapCamera.ValidatePosition();
		Close();
	}

	public void CreateRoundMap () {
		CreateCircleMap(5);
	}

}