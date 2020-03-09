public enum CombatStat{
	HP	= 1<<0,				//0b00000001	= 1
	Atk	= 1<<1,				//0b00000010	= 2
	Spd	= 1<<2,				//0b00001000	= 8
	Def = 1<<3,				//0b00010000	= 16
	Res = 1<<4,				//0b00100000	= 32
}

public static class CombatStatExtensions{
	public static readonly string[] statStrings = new string[]{"HP", "Atk", "Spd", "Def", "Res"};

	public static string GetStatString(CombatStat stat){
		switch(stat){
			case CombatStat.HP:
				return "HP";
			case CombatStat.Atk:
				return "Atk";
			case CombatStat.Spd:
				return "Spd";
			case CombatStat.Def:
				return "Def";
			case CombatStat.Res:
				return "Res";
			default:
				return "";
		}
	}

	public static int StatCount{
		get{ return 5; }
	}
}

/*
Stat explanations:

HP - Health Pool, unit dies when this reaches zero

Atk - Attack, measure of units attacking power and directly contributes to damage dealt (both physical and magical)

Skl - Skill, influences how fast unit charges their special meter

Spd - Speed, affects natural followups during combat

Def - Defense, reduces damage taken from physical attacks

Res - Resistance, reduces damage taken from magical attacks

 */