using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;


public static class SkillTable{
	
	static DataTable skillTable;


	public static void InitializeSkillTable(TextAsset csv){
		skillTable = new DataTable("Skills");
		string[,] temp = CSVReader.SplitCsvGrid(csv.text); 

		// id column
		DataColumn column = new DataColumn();
    	column.DataType = System.Type.GetType("System.Int32");
		column.ColumnName = "id";
		skillTable.Columns.Add(column);

		// name column
		column = new DataColumn();
		column.DataType = System.Type.GetType("System.String");
		column.ColumnName = "name";
		skillTable.Columns.Add(column);

		//description column
		column = new DataColumn();
		column.DataType = System.Type.GetType("System.String");
		column.ColumnName = "description";
		skillTable.Columns.Add(column);

		//y starts at 1 to skip the first row (which is just headers)
		for (int y = 1; y < temp.GetUpperBound(1); y++) {
			int id = -1;	
			if(System.Int32.TryParse(temp[0,y], out id)){
				DataRow row = skillTable.NewRow();
				row["id"] = id;
				row["name"] = temp[1,y];
				row["description"] = temp[2,y];
        		skillTable.Rows.Add(row);
			}
		}

		/*foreach(DataRow row in keywordTable.Rows){
			Debug.Log(row["id"] + ", " + row["name"] + ", " + row["description"]);
		}*/

		Debug.Log("Initialization complete; " + skillTable.Rows.Count + " rows of data processed");

	}


}
