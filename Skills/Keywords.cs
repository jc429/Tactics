using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Keywords : MonoBehaviour
{
    public TextAsset keyFile; 

	
	private System.Data.DataSet dataSet;
	DataTable keywordTable;

	void Start () {
		InitializeKeywordTable(keyFile);
	}

	void InitializeKeywordTable(TextAsset csv){
		keywordTable = new DataTable("Keywords");
		string[,] temp = CSVReader.SplitCsvGrid(csv.text); 

		// id column
		DataColumn column = new DataColumn();
    	column.DataType = System.Type.GetType("System.Int32");
		column.ColumnName = "id";
		keywordTable.Columns.Add(column);

		// name column
		column = new DataColumn();
		column.DataType = System.Type.GetType("System.String");
		column.ColumnName = "name";
		keywordTable.Columns.Add(column);

		//description column
		column = new DataColumn();
		column.DataType = System.Type.GetType("System.String");
		column.ColumnName = "description";
		keywordTable.Columns.Add(column);

		//y starts at 1 to skip the first row (which is just headers)
		for (int y = 1; y < temp.GetUpperBound(1); y++) {
			int id = -1;	
			if(System.Int32.TryParse(temp[0,y], out id)){
				DataRow row = keywordTable.NewRow();
				row["id"] = id;
				row["name"] = temp[1,y];
				row["name"] = temp[2,y];
        		keywordTable.Rows.Add(row);
			}
		}

		Debug.Log("Initialization complete; " + keywordTable.Rows.Count + " rows of data processed");
	}


}
