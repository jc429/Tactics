using System.Collections.Generic;

public class CellPriorityQueue {

	List<Cell> list = new List<Cell>();

	int count = 0;

	int minimum = int.MaxValue;

	public int Count {
		get {
			return count;
		}
	}

	/* add a cell to the list */
	public void Enqueue (Cell cell) {
		count += 1;
		int priority = cell.SearchPriority;
		if (priority < minimum) {
			minimum = priority;
		}

		// add null items to list to avoid going out of bounds
		while (priority >= list.Count) {
			list.Add(null);
		}
		cell.NextWithSamePriority = list[priority];
		list[priority] = cell;
	}

	/* remove a cell from the list */
	public Cell Dequeue () {
		count -= 1;
		for (; minimum < list.Count; minimum++) {
			Cell cell = list[minimum];
			if (cell != null) {
				list[minimum] = cell.NextWithSamePriority;
				return cell;
			}
		}
		return null;
	}
	
	/* change a cell's priority level */
	public void Change (Cell cell, int oldPriority) {
		Cell current = list[oldPriority];
		Cell next = current.NextWithSamePriority;
		if (current == cell) {
			list[oldPriority] = next;
		}
		else {
			while (next != cell) {
				current = next;
				next = current.NextWithSamePriority;
			}
			current.NextWithSamePriority = cell.NextWithSamePriority;
		}
		Enqueue(cell);
		count -= 1;		//compensate for count incrementing when enqueueing cell
	}
	
	/* clear the list */
	public void Clear () {
		list.Clear();
		count = 0;
		minimum = int.MaxValue;
	}
}