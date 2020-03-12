using UnityEngine;

/* used exclusively for unit facing */
public enum OctDirection {
	N, NE, E, SE, S, SW, W, NW
}

public static class OctDirectionExtensions {

	public static OctDirection Opposite (this OctDirection direction) {
		return (int)direction < 4 ? (direction + 4) : (direction - 4);
	}

	public static OctDirection Previous (this OctDirection direction) {
		return direction == OctDirection.N ? OctDirection.NW : (direction - 1);
	}

	public static OctDirection Next (this OctDirection direction) {
		return direction == OctDirection.NW ? OctDirection.N : (direction + 1);
	}

	public static int DegreesOfRotation(this OctDirection direction){
		return (45 * (int)direction) % 360;
	}

	public static OctDirection OctDirectionFromDegrees(float degrees){
		// rotational offset to capture wedge on both sides of desired direction
		degrees += 22.5f;
		while(degrees < 0){
			degrees = degrees + 360;
		}
		degrees = degrees % 360;
		// y 0 = north, 90 = east
		degrees /= 45f;
		return (OctDirection)degrees; 
	}
	public static OctDirection OctDirectionFromVector(Vector2 v){
		if(v == Vector2.zero){
			Debug.Log("Error: Zero Vector provided");
			return OctDirection.N;
		}
		float angle = Vector2.SignedAngle(v, Vector2.up);
		if(angle < 0){
			angle += 360;
		}
		return OctDirectionFromDegrees(angle);
	}
	
	public static QuadDirection GetNearestQuadDirection(this OctDirection dir8){
		return (QuadDirection)((int)dir8 / 2);
	}
}