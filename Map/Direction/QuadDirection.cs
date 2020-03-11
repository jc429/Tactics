using UnityEngine;

public enum QuadDirection {
	N, E, S, W
}


public static class QuadDirectionExtensions {
	public static QuadDirection Opposite (this QuadDirection direction) {
		return (int)direction < 2 ? (direction + 2) : (direction - 2);
	}

	public static QuadDirection Previous (this QuadDirection direction) {
		return direction == QuadDirection.N ? QuadDirection.W : (direction - 1);
	}

	public static QuadDirection Next (this QuadDirection direction) {
		return direction == QuadDirection.W ? QuadDirection.N : (direction + 1);
	}

	public static int DegreesOfRotation(this QuadDirection direction){
		return (90 * (int)direction) % 360;
	}

	public static QuadDirection QuadDirectionFromDegrees(float degrees){
		// rotational offset to capture wedge on both sides of desired direction
		degrees -= 45f;
		while(degrees < 0){
			degrees = degrees + 360;
		}
		degrees = degrees % 360;
		// y 0 = north, 90 = east
		degrees /= 90f;
		return (QuadDirection)degrees; 
	}

	public static QuadDirection QuadDirectionFromVector(Vector2 v){
		if(v == Vector2.zero){
			return QuadDirection.N;
		}
		return QuadDirectionFromDegrees(Mathf.RoundToInt(Vector2.Angle(Vector2.up, v)));
	}

	public static QuadDirection RandomDirection(){
		return (QuadDirection)Mathf.FloorToInt(Random.Range(0,4));
	}

	public static OctDirection ConvertToOctDirection(this QuadDirection direction){
		return (OctDirection)((int)direction * 2 + 1);
	}
}
