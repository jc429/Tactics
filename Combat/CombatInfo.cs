using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* provides all necessary info about a combat between two units, allowing for playback  */
public class CombatInfo {
	HexUnit attacker;
	HexUnit defender;

	Queue<HitInfo> hits = new Queue<HitInfo>();

	public HexUnit Attacker{
		get{ return attacker; }
	}
	public HexUnit Defender{
		get{ return defender; }
	}

	public void Clear(){
		attacker = defender = null;
		hits.Clear();
	}

	public void SetAttackerDefender(HexUnit attackUnit, HexUnit defendUnit){
		attacker = attackUnit;
		defender = defendUnit;
	}

	public void EnqueueHitInfo(HitInfo hit){
		hits.Enqueue(hit);
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