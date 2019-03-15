using UnityEngine;

public static class HexMetrics
{
	/* radius of one hex tile */
    public const float outerRadius = 2f;
    public const float innerRadius = outerRadius * 0.866025404f;

	/* percentage of the hex tile that remains solid (for edge blending) */
    public const float solidFactor = 0.85f;
	public const float blendFactor = 1f - solidFactor;

	/* how high (in m) one step of elevation raises one tile */
	public const float elevationStep = 0.5f;

	/* subdivisions of a slope */
	public const int terracesPerSlope = 2;
	public const int terraceSteps = terracesPerSlope * 2 + 1;
	public const float horizontalTerraceStepSize = 1f / terraceSteps;
	public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

	/* locations of tile corners */
    static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius),                       //north
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),       //northeast
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),      //southeast
		new Vector3(0f, 0f, -outerRadius),                      //south
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),     //southwest
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),      //northwest
		new Vector3(0f, 0f, outerRadius)                        //north again
	};

	/* how many cells per chunk */
	public const int chunkSizeX = 5, chunkSizeZ = 5;



	public static Vector3 GetFirstCorner (HexDirection direction) {
		return corners[(int)direction];
	}

	public static Vector3 GetSecondCorner (HexDirection direction) {
		return corners[(int)direction + 1];
	}

	public static Vector3 GetFirstSolidCorner (HexDirection direction) {
		return corners[(int)direction] * solidFactor;
	}

	public static Vector3 GetSecondSolidCorner (HexDirection direction) {
		return corners[(int)direction + 1] * solidFactor;
	}

	public static Vector3 GetBridge(HexDirection direction){
		return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
	}

	public static HexEdgeType GetEdgeType (int elevation1, int elevation2) {
		if (elevation1 == elevation2) {
			return HexEdgeType.Flat;
		}
		int delta = elevation2 - elevation1;
		if (delta == 1 || delta == -1) {
			return HexEdgeType.Slope;
		}
		return HexEdgeType.Cliff;
	}

	public static Vector3 TerraceLerp (Vector3 a, Vector3 b, int step) {
		float h = step * HexMetrics.horizontalTerraceStepSize;
		a.x += (b.x - a.x) * h;
		a.z += (b.z - a.z) * h;
		float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
		a.y += (b.y - a.y) * v;
		return a;
	}
	
	public static Color TerraceLerp (Color a, Color b, int step) {
		float h = step * HexMetrics.horizontalTerraceStepSize;
		return Color.Lerp(a, b, h);
	}

}
