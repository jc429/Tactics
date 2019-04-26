using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombatPropertyFlag{
	Null 				= 0, //0b00000000000000000000000000000000
	ForcePriority 		= 1,
	ForceAntiPriority 	= 2,
	ForceFollowup 		= 4,
	DenyFollowup		= 8,

}


/* this class contains all of the unit's properties that are specific to a single instance of combat */
[System.Serializable]
public class UnitCombatProperties{
	[System.NonSerialized]
	HexUnit unit;
	[System.NonSerialized] 
	UnitProperties unitProperties;

	public HexUnit foe;

	/* stats at the start of combat */
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] combatStats = new int[(int)CombatStat.Total];
	/* in-combat buffs/debuffs */
	[NamedArrayAttribute (new string[] {"HP", "Str", "Skl", "Spd", "Def", "Res"})]
	public int[] combatStatMods = new int[(int)CombatStat.Total];

	uint flags; 

	bool ignoreBuffs;
	bool ignoreDebuffs;

	public void Clear(){
		foe = null;
		for(int i = 0; i < (int)CombatStat.Total; i++){
			combatStats[i] = 0;
			combatStatMods[i] = 0;
		}
		flags = 0;
		ignoreBuffs = false;
		ignoreDebuffs = false;
	}	

	public void SetUnit(HexUnit hu){
		unit = hu;
		unitProperties = hu.Properties;
	}

	public void SetFlag(CombatPropertyFlag flag, bool value = true){
		if(value){
			flags |= (uint)flag;
		}
		else{
			flags &= ~(uint)flag;
		}
	}

	public bool CheckFlag(CombatPropertyFlag flag){
		return ((flags & (uint)flag) != 0);
	}

	/* sets up the combat stats (which will be further modified by in-combat buffs/debuffs) */
	public void CalculateStartingStats(bool ignoreBuffs = false, bool ignoreDebuffs = false){
		for(int i = 0; i < (int)CombatStat.Total; i++){
			int stat = unitProperties.rawStats[i] + unitProperties.statModifiers[i];
			if(ignoreBuffs){
				stat += unitProperties.fieldBuffs[i];
			}
			if(ignoreDebuffs){
				stat += unitProperties.fieldDebuffs[i];
			}
			combatStats[i] = stat;
		}
	}

	public void SetIgnoreBuffs(bool val = true){
		if(val != ignoreBuffs){
			CalculateStartingStats(val,ignoreDebuffs);
		}
		ignoreBuffs = val;
	}

	public void SetIgnoreDebuffs(bool val = true){
		if(val != ignoreDebuffs){
			CalculateStartingStats(ignoreBuffs,val);
		}
		ignoreDebuffs = val;
	}

	public void ModifyStat(CombatStat stat, int amount){
		combatStatMods[(int)stat] += amount;
	}
	public void ModifyStats(byte statMatrix, int amount){
		for(int i = 0; i < (int)CombatStat.Total; i++){
			if((statMatrix & (1 << i)) != 0){
				combatStatMods[i] += amount;
			}
		}
	}

}
