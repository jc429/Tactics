using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSkillEventArgs : EventArgs {
	public ConditionEffectPair cePair;

	public UnitSkillEventArgs (ConditionEffectPair ce){
		cePair = ce;
	}

}