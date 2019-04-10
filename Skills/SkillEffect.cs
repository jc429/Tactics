using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* one single skill effect, and all the variables it needs to perform its job */

[System.Serializable]
public class SkillEffect{
    SkillEffectData effectData;

	int[] vars;

	
	public SkillEffect(SkillEffectData data){
		this.effectData = data;
		if(data.varCount > 0){
			vars = new int[data.varCount];
		}
	}

}
