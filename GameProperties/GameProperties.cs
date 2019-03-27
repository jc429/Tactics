﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameProperties
{
	public const bool DEBUG_INFINITE_ACTIONS = true;



	public static class Stats{
		public static readonly string[] statList = new string[]{"HP", "Str", "Skl", "Spd", "Def", "Res"};

		public static string GetStatString(CombatStat stat){
			if(stat == CombatStat.Total){
				return "Total";
			}
			return statList[(int)stat];
		}
	}
	
	public static class MovementProperties{

		public static readonly int[] ClassBaseMovement = new int[]
		//	inf		cav		fly		hvy		aqua
		{	4,		6,		4,		2,		6	};

		static readonly int[,] ClassTerrainMovementCostMatrix = new int[,]
		{
		//		inf		cav		fly		hvy		aqua
			{	1,		1,		1,		1,		2	},		//default
			{	1,		1,		1,		1,		2	},		//Road
			{	1,		1,		1,		1,		2	},		//Grass
			{	2,		3,		1,		1,		3	},		//Forest
			{	2,		4,		1,		1,		0	},		//Desert
			{	1,		1,		4,		1,		2	},		//Indoor
			{	2,		0,		1,		0,		1	},		//Water (Shallow)
			{	4,		0,		1,		0,		1	},		//Water (Deep)
			{	1,		4,		1,		1,		2	},		//Coast
			{	0,		0,		0,		0,		0	}		//Solid
		};
		/* move cost of 0 means "cannot enter tile no matter what" */

		public static int GetCostToEnter(TerrainType terrain, MovementClass movClass){
			return ClassTerrainMovementCostMatrix[(int)terrain,(int)movClass];
		}
	}
	
}
