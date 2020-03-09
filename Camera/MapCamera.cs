using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
	public bool invertZoomAxis;

	/* the map this camera is focusing on */ 
	public MapGrid grid;

	/* pivot axes for camera */
	Transform swivel, stick;

	OctDirection facing;



	public float zoom = 1f;

	[Range(-5, 10)]
	public float zoomMin, zoomMax;

	[Range(-30, 45)]
	public float camPitchMin = 0, camPitchMax = 30;

	[Range(3, 30)]
	public float panSpeedZoomMax = 5, panSpeedZoomMin = 15;

	float rotationSpeed;
	float rotationAngle;
	float shiftSpeed = 100;

	/* is the camera moving or at rest */ 
	bool atRest;

	bool locked;
	public bool Locked{
		get{ return locked; }
	}

	void Awake(){
		GameController.mapCamera = this;
		stick = transform.GetChild(0);
		swivel = stick.GetChild(0);
		facing = OctDirection.N;
		atRest = true;
	}

	void Start(){
		AdjustZoom(0.15f);
		
		MapCoordinates c = grid.GetCenterCoordinates();
		transform.localPosition = new Vector3(c.X, 0, c.Y);

	}

	// Update is called once per frame
	void Update()
	{
		if(!locked){
			float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
			if (zoomDelta != 0f) {
				AdjustZoom(zoomDelta);
			}

			float rotationDelta = Input.GetAxis("CamRotation");
			if (rotationDelta != 0f) {
				ShiftRotation(rotationDelta);
			}

			float xDelta = Input.GetAxis("Horizontal");
			float zDelta = Input.GetAxis("Vertical");
			if (xDelta != 0f || zDelta != 0f) {
				AdjustPosition(xDelta, zDelta);
			}
		}
	}

	/* Freely adjust camera zoom */
	void AdjustZoom (float delta) {
		if(invertZoomAxis){
			delta *= -1;
		}
		zoom = Mathf.Clamp01(zoom + delta);

		float distance = Mathf.Lerp(zoomMin, zoomMax, zoom);
		stick.localPosition = new Vector3(0f, distance, 0f);

		float angle = Mathf.Lerp(camPitchMin, camPitchMax, zoom);
		swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}

	void SetZoom(float z){
		zoom = Mathf.Clamp01(z);

		float distance = Mathf.Lerp(zoomMin, zoomMax, zoom);
		stick.localPosition = new Vector3(0f, distance, 0f);

		float angle = Mathf.Lerp(camPitchMin, camPitchMax, zoom);
		swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}

	/* Freely rotates the camera */
	void AdjustRotation(float delta){
		rotationAngle += delta * rotationSpeed * Time.deltaTime;
		transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);

		if (rotationAngle < 0f) {
			rotationAngle += 360f;
		}
		else if (rotationAngle >= 360f) {
			rotationAngle -= 360f;
		}
	}

	/* shifts the camera direction one dodecdirection clockwise/counterclockwise */
	void ShiftRotation(float delta){
		if(!atRest){
			return;
		}
		if(delta > 0){
			facing = facing.Previous();
		}
		else{
			facing = facing.Next();
		}
		atRest = false;
		StartCoroutine(TurnToLookAt(facing));
	}

	IEnumerator TurnToLookAt (OctDirection dir){
		float rotation = dir.DegreesOfRotation();
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation = Quaternion.Euler(new Vector3(0,dir.DegreesOfRotation(),0));
		float angle = Quaternion.Angle(fromRotation, toRotation);
		float speed = shiftSpeed / angle;
		
		if( angle != 0){
			for (float t = speed * Time.deltaTime; t < 1f; t += speed * Time.deltaTime) {
				transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
				yield return null;
			}
		}
		atRest = true;
	}

	/* Freely pans the camera */
	void AdjustPosition (float xDelta, float zDelta) {
		Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
		float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
		float distance = Mathf.Lerp(panSpeedZoomMax, panSpeedZoomMin, zoom) * damping * Time.deltaTime;

		Vector3 pos = transform.localPosition;
		pos += distance * direction;
		transform.localPosition = ClampPosition(pos);
	}

	void SetPosition(float xPos, float zPos){
		Vector3 pos = transform.localPosition;
		pos.x = xPos;
		pos.z = zPos;
		transform.localPosition = ClampPosition(pos);
	}

	void SetPosition(Vector3 pos){
		transform.localPosition = ClampPosition(pos);
	}

	/* keeps camera within grid boundaries */
	Vector3 ClampPosition (Vector3 position) {
		float xMax = (grid.cellCountX - 1f) * QuadMetrics.cellWidth;
		position.x = Mathf.Clamp(position.x, 0f, xMax);

		float zMax = (grid.cellCountY - 2f) * QuadMetrics.cellWidth;
		position.z = Mathf.Clamp(position.z, 0f, zMax);

		return position;
	}

	/* forces camera to refresh and stay within map bounds */
	public void ValidatePosition () {
		AdjustPosition(0f, 0f);
	}

	/* Locks the camera from moving */
	public void LockCamera(){
		locked = true;
	}

	/* Unlocks the camera */
	public void UnlockCamera(){
		locked = false;
	}

	/* puts the camera in a default state, usually after reloading a map */
	public void ResetZoomAndCenterCamera(){
		SetZoom(0.15f);
		MapCell center = grid.GetCell(grid.GetCenterCoordinates());
		SetPosition(center.Position);
	}
}
