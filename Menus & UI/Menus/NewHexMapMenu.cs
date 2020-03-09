using UnityEngine;

public class NewHexMapMenu : MonoBehaviour {

	public HexGrid hexGrid;

	public void Open () {
		gameObject.SetActive(true);
		//GameController.hexCamera.LockCamera();
	}

	public void Close () {
		gameObject.SetActive(false);
		//GameController.hexCamera.UnlockCamera();
	}

	void CreateRectMap (int x, int z) {
		hexGrid.CreateMapRect(x, z);
		//GameController.hexCamera.ValidatePosition();
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
		//GameController.hexCamera.ValidatePosition();
		Close();
	}

	public void CreateRoundMap () {
		CreateCircleMap(5);
	}

}