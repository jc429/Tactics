using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectID{
	EFF_NONE = 0,
	EFF_ADD_ATK = 1, 
	EFF_ADD_SPD = 2,
	EFF_ADD_DEF = 3,
	EFF_ADD_RES = 4,
	EFF_ADD_HP = 5,
	EFF_ADD_SKL = 6
}

public static class EffectIDExtensions{

	public static EffectID GetEffectID(string input){
		EffectID id;
		if(Enum.TryParse(input, true, out id)){
			return id;
		}
		else{
			return EffectID.EFF_NONE;
		}
	}
	
}