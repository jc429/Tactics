using UnityEngine;

public enum SkillType{
	ST_NULL = 0,
    ST_WEAPON = 1,
    ST_ASSIST = 2,
    ST_COMBATART = 3,
    ST_SPECIAL = 4,
    ST_PRIMARY = 5,
    ST_SECONDARY = 6,
    ST_AURA = 7
}

public static class SkillTypeExtensions{
	public static SkillType GetSkillType(int num){
		num = Mathf.Clamp(num, 0, 7);
		return (SkillType)num;
	}	
	
	public static SkillType GetSkillType(string input){
		SkillType type;
		if(System.Enum.TryParse(input, true, out type)){
			return type;
		}
		else{
			return SkillType.ST_NULL;
		}
	}
}

/*
Skill Types:

Weapon - has a weapon might which is combined with the user's

Movement Art - a positioning skill

Combat Art - skill that disrupts the enemy in some way

special - activates during combat 

Passives - passive skills almost always influence combat in some way.
Primary passives generally buff the unit wielding them, usually applying stat bonuses under certain conditions.
Secondary passives apply debuffs or restrictions on the foe, or manipulate how combat is resolved.
Aura passives apply buffs and/or debuffs to other units on the map in an area of effect around the unit.

*/