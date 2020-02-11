using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*** Color Flags for Map Cells ***/

public struct ColorFlags{
	Cell cell;
	bool isHoveredOn; 
	bool isSelected; 
	bool onMovementPath; 
	bool inMovementRange;
	bool inAttackRange;
	bool inAssistRange;

	public ColorFlags(Cell parent){
		cell = parent;
		isHoveredOn = isSelected = onMovementPath = inMovementRange = inAttackRange = inAssistRange = false;
	}

	public bool IsHoveredOn{
		get{ return isHoveredOn; }
		set{
			isHoveredOn = value;
			cell.RefreshHighlight();
		}
	}
	public bool IsSelected{
		get{ return isSelected; }
		set{
			isSelected = value;
			cell.RefreshHighlight();
		}
	} 
	public bool OnMovementPath{
		get{ return onMovementPath; }
		set{
			onMovementPath = value;
			cell.RefreshHighlight();
		}
	} 
	public bool InMovementRange{
		get{ return inMovementRange; }
		set{
			inMovementRange = value;
			cell.RefreshHighlight();
		}
	} 
	public bool InAttackRange{
		get{ return inAttackRange; }
		set{
			inAttackRange = value;
			cell.RefreshHighlight();
		}
	} 
	public bool InAssistRange{
		get{ return inAssistRange; }
		set{
			inAssistRange = value;
			cell.RefreshHighlight();
		}
	} 
	public void Clear(){
		isHoveredOn = isSelected = onMovementPath = inMovementRange = inAttackRange = inAssistRange = false;
	}
}