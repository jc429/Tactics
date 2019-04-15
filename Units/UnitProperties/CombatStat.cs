public enum CombatStat{
    HP,					//0b00000001	= 1
    Atk,				//0b00000010	= 2
    Skl,				//0b00000100	= 4
    Spd,				//0b00001000	= 8
    Def,				//0b00010000	= 16
    Res,				//0b00100000	= 32
	Total
}

/*
Stat explanations:

HP - Health pool, unit dies when this reaches zero

Str - Strength, measure of units attacking power and directly contributes to damage dealt (both physical and magical)

Skl - Skill, influences how fast unit charges their special meter

Spd - Speed, affects natural followups during combat

Def - Defense, reduces damage taken from physical attacks

Res - Resistance, reduces damage taken from magical attacks

 */