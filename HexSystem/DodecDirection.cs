using UnityEngine;

/* used exclusively for unit facing */
public enum DodecDirection {
	NE, ENE, E, ESE, S, SW, WSW, W, WNW, NW, N
}

public static class DodecDirectionExtensions {

	public static DodecDirection Opposite (this DodecDirection direction) {
		return (int)direction < 6 ? (direction + 6) : (direction - 6);
	}

	public static DodecDirection Previous (this DodecDirection direction) {
		return direction == DodecDirection.NE ? DodecDirection.N : (direction - 1);
	}

	public static DodecDirection Next (this DodecDirection direction) {
		return direction == DodecDirection.N ? DodecDirection.NE : (direction + 1);
	}

	public static int DegreesOfRotation(this DodecDirection direction){
		return (30 * (int)direction) % 360;
	}

	public static DodecDirection DodecDirectionFromDegrees(int degrees){
		while(degrees < 0){
			degrees = degrees + 360;
		}
		degrees = degrees % 360;
		// y 0 = north, 90 = east
		return (DodecDirection)(degrees / 30); 
	}
	
	public static HexDirection GetNearestHexDirection(this DodecDirection dir12){
		return (HexDirection)((int)dir12 / 2);
	}
}