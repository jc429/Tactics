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
		CreateCircleMap(4);
	}

	public void CreateMediumMap () {
		CreateCircleMap(6);
	}

	public void CreateLargeMap () {
		CreateCircleMap(8);
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