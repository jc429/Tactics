using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* provides all necessary info about a combat between two units, allowing for playback  */
public class CombatInfo {
	HexUnit attacker;
	HexUnit defender;
	public HexUnit Attacker{
		get{ return attacker; }
	}
	public HexUnit Defender{
		get{ return defender; }
	}
	//used to check when combat needs to be recalculated
	public HexCell attackerTile; 

	int attackerFinalHP;
	int defenderFinalHP;
	public int AttackerFinalHP{
		get { return attackerFinalHP; }
	}
	public int DefenderFinalHP{
		get { return defenderFinalHP; }
	}

	Queue<HitInfo> hits = new Queue<HitInfo>();



	

	public void Clear(){
		attacker = defender = null;
		attackerFinalHP = defenderFinalHP = 0;
		hits.Clear();
	}

	public void SetAttackerDefender(HexUnit attackUnit, HexUnit defendUnit){
		attacker = attackUnit;
		defender = defendUnit;
		attackerFinalHP = attackUnit.CurrentHP;
		defenderFinalHP = defendUnit.CurrentHP;
	}

	public void EnqueueHitInfo(HitInfo hit){
		hits.Enqueue(hit);
		attackerFinalHP = hit.attackerEndHP;
		defenderFinalHP = hit.defenderEndHP;
	}

	public HitInfo GetHitInfo(){
		if(hits.Count <= 0){
			return null;
		}
		return hits.Dequeue();
	}

	public int GetHitCount(){
		return hits.Count;
	}
}