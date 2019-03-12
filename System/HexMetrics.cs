using UnityEngine;

public static class HexMetrics
{
    public const float outerRadius = 2f;
    public const float innerRadius = outerRadius * 0.866025404f;

    public static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius),                       //north
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),       //northeast
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),      //southeast
		new Vector3(0f, 0f, -outerRadius),                      //south
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),     //southwest
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),      //northwest
		new Vector3(0f, 0f, outerRadius)                        //north again
	};
}
