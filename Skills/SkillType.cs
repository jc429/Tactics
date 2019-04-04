public enum SkillType{
    Weapon = 0,
    MovementArt = 1,
    CombatArt = 2,
    Item = 3,
    PrimaryPassive = 4,
    SecondaryPassive = 5,
    AuraPassive = 6
}

/*
Skill Types:

Weapon - has a weapon might which is combined with the user's

Movement Art - a positioning skill

Combat Art - charges up via special meter, usually deals high damage or applies a powerful effect 

Item - applies a small effect, usually once per battle (e.g. heal a small amount of hp, deal minor AOE damage)

Passives - passive skills almost always influence combat in some way.
Primary passives generally buff the unit wielding them, usually applying stat bonuses under certain conditions.
Secondary passives apply debuffs or restrictions on the foe, or manipulate how combat is resolved.
Aura passives apply buffs and/or debuffs to other units on the map in an area of effect around the unit.

*/