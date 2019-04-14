using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* an effect family is a group of skill effects that are applied at the same time (ex: atk +5 and def +5)*/

[System.Serializable]
public class SkillEffectFamily{
	[System.NonSerialized]
	public ConditionEffectPair parentPair;
	
	public List<SkillEffect> skillEffects;

	HexUnit Unit{
		get{
			return parentPair.parentSkill.unit;
		}
	}

	public SkillEffectFamily(){
		skillEffects = new List<SkillEffect>();
	}

	public void AddSkillEffect(SkillEffect effect){
		skillEffects.Add(effect);
		effect.parentEffectFamily = this;
	}

	public void ClearSkillEffects(){
		if(skillEffects != null){
			skillEffects.Clear();
		}
		else{
			skillEffects = new List<SkillEffect>();
		}
	}

	public void ApplyEffects(){
		foreach(SkillEffect effect in skillEffects){
			effect.Apply();
		}
	}

}
