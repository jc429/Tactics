/* unit's current status on a given turn */
public enum TurnState{
	Idle,			//untouched, basic state at start of turn
	PreMove,		//selected, choosing where to move to
	Moving,			//while unit is in transit to destination
	PostMove, 		//after moving, open the action menu during this
	PreAttack,
	PreAssist,
	Finished		//after attacking or using item, unit cannot do anything else until next turn unless refreshed
}