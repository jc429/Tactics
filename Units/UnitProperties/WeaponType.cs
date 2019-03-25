public enum WeaponType {
	None,
    Sword,
    Lance,
    Axe,
    Bow,
    Gun,
    Magic,
    Dagger,
    Staff
}

public static class WeaponTypeExtensions {
	static readonly string[] weaponTypeList = new string[]{
		"None",
		"Sword",
		"Lance",
		"Axe",
		"Bow",
		"Gun",
		"Magic",
		"Dagger",
		"Staff"
	};

	public static string ToString(this WeaponType weaponType){
		return weaponTypeList[(int)weaponType];
	}
}

/*
3 basic damage "types":
slash (sword, magic)
pierce (lance, bow)
crush (axe, gun)

Weapon Triangles: 
Melee:
sword > axe > lance > sword
Ranged:
bow > magic > gun > bow

sword,lance,axe,bow can all have physical or magical damage, depending on weapon (though they all lean physical)
gun is always physical damage (?)
magic is always magical damage (?)
dagger: physical debuff-type weapon, additional bonus damage granted for backstabs
staff: magical combat manipulation weapon
*/