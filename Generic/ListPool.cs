using System.Collections.Generic;
using UnityEngine;

public static class ListPool<T> {

	static Stack<List<T>> stack = new Stack<List<T>>();

	/* get the top list on the stack, or generate a fresh list */
	public static List<T> Get () {
		if (stack.Count > 0) {
			return stack.Pop();
		}
		return new List<T>();
	}

	/* add a new list to the stack, clearing it just in case */
	public static void Add (List<T> list) {
		if(list == null){
			Debug.Log("Hmmm");
		}
		list.Clear();
		stack.Push(list);
	}
}