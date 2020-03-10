using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACPInterface : MonoBehaviour{
	
	public ArmyColorProfile[] colorProfiles = new ArmyColorProfile[ArmyManager.numArmies];

	void Awake(){
		ArmyManager.acpInterface = this;
	}
	
}
