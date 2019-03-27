public enum MovementClass{
    Infantry,
    Cavalry,
    Flying,
    Heavy,
    Aquatic
}

public static class MovementClassExtensions {
	static readonly string[] movTypeList = new string[]{"Infantry", "Cavalry", "Flying", "Heavy", "Aquatic"};

	public static int GetRange(this MovementClass movClass){
		return GameProperties.MovementProperties.ClassBaseMovement[(int)movClass];
	}

	public static string ToString(this MovementClass movClass){
		return movTypeList[(int)movClass];
	}
}

/* 
Movement classes:

infantry
    + average movement
    - slight movement penalty on sandy terrain and in water

cavalry 
    + very high movement on fields and roads
    - harsh movement penalties on sand, forests, and indoors
    - cannot traverse water
    - cannot ascend/descend ledges

flying
    + long range movement
    + no movement penalties 
    - weakness to all long-range weapons

heavy armor
    + generally massive defenses
    - low movement
    - cannot ascend/descend ledges 
    
aquatic
    + very high movement in water
    - very low movement on land
    - weakness to melee weapons?

 */