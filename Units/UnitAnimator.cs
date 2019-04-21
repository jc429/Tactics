using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
	public HexUnit _unit;
	public GameObject model;
	
	const float modelOffset = 0.5f;
	const float rotationSpeed = 360f;
	const float hopSpeed = 4f;

	
	// direction unit model is facing
	float orientation;
	public float Orientation {
		get {	return orientation;	}
		set {
			orientation = value;
			model.transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

 
	void Start(){

	}

	void Update(){

	}


	public void SetRotation(Quaternion rotation){
		model.transform.localRotation = rotation;
	}
	

	public IEnumerator TurnToLookAt (Vector3 point) {
		point.y = model.transform.localPosition.y;
		Quaternion fromRotation = model.transform.localRotation;
		Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
		float angle = Quaternion.Angle(fromRotation, toRotation);
		float speed = rotationSpeed / angle;
		
		if( angle > 0){
			for (float t = speed * Time.deltaTime; t < 1f; t += speed * Time.deltaTime) {
				model.transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
				yield return null;
			}
		}

		model.transform.LookAt(point);
		orientation = model.transform.localRotation.eulerAngles.y;
		_unit.Facing = DodecDirectionExtensions.DodecDirectionFromDegrees(Mathf.RoundToInt(orientation));
	}
	
	public IEnumerator PerformAttackAnimation(){
		Vector3 pos = model.transform.localPosition;
		
		for (float t = hopSpeed * Time.deltaTime; t < 1f; t += hopSpeed * Time.deltaTime) {
			pos.y = modelOffset + (0.5f * Mathf.Sin(t * Mathf.PI));
			model.transform.localPosition = pos;
			yield return null;
		}
		pos.y = modelOffset;
		model.transform.localPosition = pos;
	} 

	public void PlayAttackAnimation(){
		StartCoroutine(PerformAttackAnimation());
	}
	
}
