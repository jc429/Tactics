using UnityEngine;

public static class QuadMetrics
{

	/* size of one tile */
	public const float cellWidth = 2f;
	public const float halfWidth = 1f;

	/* percentage of the tile that remains solid (for edge blending) */
  public const float solidFactor = 0.75f;
	public const float blendFactor = 1f - solidFactor;

	/* percentage of submerged tile that remains non-shore water */
	public const float waterFactor = 0.6f;
	public const float waterBlendFactor = 1f - waterFactor;
	
	/* how high (in m) one step of elevation raises one tile */
	public const float elevationStep = 0.5f;

	/* subdivisions of a slope */
	public const int terracesPerSlope = 2;
	public const int terraceSteps = terracesPerSlope * 2 + 1;
	public const float horizontalTerraceStepSize = 1f / terraceSteps;
	public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

	/* locations of tile corners */
	static Vector3[] corners = {
		new Vector3(-halfWidth, 0f, halfWidth),			//northwest
		new Vector3(halfWidth, 0f, halfWidth),			//northeast
		new Vector3(halfWidth, 0f, -halfWidth),			//southeast
		new Vector3(-halfWidth, 0f, -halfWidth),		//southwest
		new Vector3(-halfWidth, 0f, halfWidth),			//northwest again
	};

	/* how many cells per chunk */
	public const int chunkSizeX = 4, chunkSizeY = 4;

	/* how much lower water surface should be than land surface */
	public const float waterElevationOffset = -0.25f;

	/* cell color index */
	public static Color[] colors;

	/* noise sampling texture */ 
	public static Texture2D noiseSource;

	
	public static Vector4 SampleNoise (Vector3 position) {
		return noiseSource.GetPixelBilinear(position.x, position.z);
	}


	public static Vector3 GetFirstCorner (QuadDirection direction) {
		return corners[(int)direction];
	}

	public static Vector3 GetSecondCorner (QuadDirection direction) {
		return corners[(int)direction + 1];
	}

	public static Vector3 GetFirstSolidCorner (QuadDirection direction) {
		return corners[(int)direction] * solidFactor;
	}

	public static Vector3 GetSecondSolidCorner (QuadDirection direction) {
		return corners[(int)direction + 1] * solidFactor;
	}
	
	public static Vector3 GetFirstWaterCorner (QuadDirection direction) {
		return corners[(int)direction] * waterFactor;
	}

	public static Vector3 GetSecondWaterCorner (QuadDirection direction) {
		return corners[(int)direction + 1] * waterFactor;
	}

	public static Vector3 GetBridge(QuadDirection direction){
		return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
	}

	public static Vector3 GetWaterBridge (QuadDirection direction) {
		return (corners[(int)direction] + corners[(int)direction + 1]) * waterBlendFactor;
	}

	public static EdgeType GetEdgeType (int elevation1, int elevation2) {
		if (elevation1 == elevation2) {
			return EdgeType.Flat;
		}
		int delta = elevation2 - elevation1;
		if (delta == 1 || delta == -1) {
			return EdgeType.Terrace;
		}
		return EdgeType.Cliff;
	}

	public static Vector3 TerraceLerp (Vector3 a, Vector3 b, int step) {
		float h = step * QuadMetrics.horizontalTerraceStepSize;
		a.x += (b.x - a.x) * h;
		a.z += (b.z - a.z) * h;
		float v = ((step + 1) / 2) * QuadMetrics.verticalTerraceStepSize;
		a.y += (b.y - a.y) * v;
		return a;
	}
	
	public static Color TerraceLerp (Color a, Color b, int step) {
		float h = step * QuadMetrics.horizontalTerraceStepSize;
		return Color.Lerp(a, b, h);
	}

}
