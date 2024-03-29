﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCamera : MonoBehaviour
{

	/* pivot axes for camera */
	Transform swivel, stick;

	public float zoom = 1f;

	public float stickMinZoom, stickMaxZoom;

	public float swivelMinZoom, swivelMaxZoom;

	public float moveSpeedMinZoom, moveSpeedMaxZoom;

	public float rotationSpeed;
	public float rotationAngle;
	public float shiftSpeed;

	DodecDirection facing;
	bool atRest;

	public HexGrid grid;
	// public MapGrid grid;

	bool locked;

	void Awake () {
		//GameController.hexCamera = this;
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);
		facing = DodecDirection.N;
		atRest = true;
	}

	void Start(){
		AdjustZoom(0.15f);
		
		float xMax = (grid.cellCountX - 0.5f) * (2f * HexMetrics.innerRadius);
		float zMax = (grid.cellCountZ - 1) * (1.5f * HexMetrics.outerRadius);
		//Debug.Log(xMax + ", " + zMax);
		transform.localPosition = new Vector3(0.5f*xMax,0,0.5f*zMax);
		
	}

	void Update () {
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
		zoom = Mathf.Clamp01(zoom + delta);

		float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
		stick.localPosition = new Vector3(0f, 0f, distance);

		float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
		swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}

	void SetZoom(float z){
		zoom = Mathf.Clamp01(z);

		float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
		stick.localPosition = new Vector3(0f, 0f, distance);

		float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
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

	IEnumerator TurnToLookAt (DodecDirection dir){
		float rotation = dir.DegreesOfRotation();
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation = Quaternion.Euler(new Vector3(0,dir.DegreesOfRotation(),0));
		float angle = Quaternion.Angle(fromRotation, toRotation);
		float speed = shiftSpeed / angle;
		
		if( angle > 0){
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
		float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

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
		float xMax = (grid.cellCountX - 0.5f) * (2f * HexMetrics.innerRadius);
		position.x = Mathf.Clamp(position.x, 0f, xMax);

		float zMax = (grid.cellCountZ - 1) * (1.5f * HexMetrics.outerRadius);
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
		HexCell center = grid.GetCell(grid.GetCenterCoordinates());
		SetPosition(center.Position);
	}
}
