using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* an effect family is a group of skill effects that are applied at the same time (ex: atk +5 and def +5)*/

[System.Serializable]
public class SkillEffectFamily : MonoBehaviour
{
	List<SkillEffect> skillEffects;

	public SkillEffectFamily(){
		skillEffects = new List<SkillEffect>();
	}

	public void AddSkillEffect(SkillEffect effect){
		skillEffects.Add(effect);
	}

	public void ClearSkillEffects(){
		if(skillEffects != null){
			skillEffects.Clear();
		}
		else{
			skillEffects = new List<SkillEffect>();
		}
	}
}
