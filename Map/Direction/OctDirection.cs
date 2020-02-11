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

	public static OctDirection OctDirectionFromDegrees(int degrees){
		while(degrees < 0){
			degrees = degrees + 360;
		}
		degrees = degrees % 360;
		// y 0 = north, 90 = east
		return (OctDirection)(degrees / 45); 
	}
	
	public static QuadDirection GetNearestQuadDirection(this OctDirection dir8){
		return (QuadDirection)((int)dir8 / 2);
	}
}