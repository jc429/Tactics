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

	void CreateMap (int x, int z) {
		hexGrid.CreateMap(x, z);
		GameController.mapCamera.ValidatePosition();
		Close();
	}

	public void CreateSmallMap () {
		CreateMap(8, 8);
	}

	public void CreateMediumMap () {
		CreateMap(16, 16);
	}

	public void CreateLargeMap () {
		CreateMap(24, 24);
	}

}