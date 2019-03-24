public enum MovementClass{
    Infantry,
    Cavalry,
    Flying,
    Heavy,
    Aquatic
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