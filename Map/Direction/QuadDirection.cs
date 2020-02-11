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

	public static QuadDirection QuadDirectionFromDegrees(int degrees){
		while(degrees < 0){
			degrees = degrees + 360;
		}
		degrees = degrees % 360;
		return (QuadDirection)(degrees / 90); 
	}

	public static QuadDirection RandomDirection(){
		return (QuadDirection)Mathf.FloorToInt(Random.Range(0,4));
	}

	public static OctDirection ConvertToOctDirection(this QuadDirection direction){
		return (OctDirection)((int)direction * 2 + 1);
	}
}
