using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;


public static class SkillTable{
	const string s_ID = "ID";
	const string s_Type = "Type";
	const string s_Name = "Name";
	const string s_Desc = "Description";
	const string s_spriteID = "Sprite ID";


	static DataTable skillTable;


	public static void InitializeSkillTable(TextAsset csv){
		skillTable = new DataTable("Skills");
		string[,] temp = CSVReader.SplitCsvGrid(csv.text); 

		// id column
		DataColumn column = new DataColumn();
    	column.DataType = System.Type.GetType("System.Int32");
		column.ColumnName = s_ID;
		skillTable.Columns.Add(column);
		
		// skilltype column
		column = new DataColumn();
		column.DataType = System.Type.GetType("System.Int32");
		column.ColumnName = s_Type;
		skillTable.Columns.Add(column);

		// name column
		column = new DataColumn();
		column.DataType = System.Type.GetType("System.String");
		column.ColumnName = s_Name;
		skillTable.Columns.Add(column);

		//description column
		column = new DataColumn();
		column.DataType = System.Type.GetType("System.String");
		column.ColumnName = s_Desc;
		skillTable.Columns.Add(column);

		//sprite id column
		column = new DataColumn();
		column.DataType = System.Type.GetType("System.Int32");
		column.ColumnName = s_spriteID;
		skillTable.Columns.Add(column);
		
		//set id to primary key
		DataColumn[] pkeys = new DataColumn[1];
		pkeys[0] = skillTable.Columns[s_ID];
		skillTable.PrimaryKey = pkeys;  

		//y starts at 1 to skip the first row (which is just headers)
		for (int y = 1; y < temp.GetUpperBound(1); y++) {
			int id = -1;	
			if(System.Int32.TryParse(temp[0,y], out id)){
				DataRow row = skillTable.NewRow();
				row[s_ID] = id;
				row[s_Type] = ParseSkillType(temp[1,y]);
				row[s_Name] = temp[2,y];
				row[s_Desc] = temp[3,y];
				row[s_spriteID] = temp[4,y];
        		skillTable.Rows.Add(row);
			}
			else{
				Debug.Log(temp[0,y] + " is not a valid skill ID number, skipping row");
			}
		}

		/*foreach(DataRow row in keywordTable.Rows){
			Debug.Log(row["id"] + ", " + row["name"] + ", " + row["description"]);
		}*/

		Debug.Log("Skill Table initialization complete; " + skillTable.Rows.Count + " rows of data processed");

	}

	static int  ParseSkillType(string skilltype){
		int typeInt = 0;
		if(System.Int32.TryParse(skilltype, out typeInt)){
			if(typeInt > 7 || typeInt < 0){
				typeInt = 0;
			}
			return typeInt;
		}
		else{
			return 0;
		}
	}

	public static Skill GetSkill(int skillID){
		DataRow row = skillTable.Rows.Find(skillID);
		if(row == null){
			Debug.Log("No skills with ID of " + skillID + " found!");
			return null;
		}
		else{
			Skill skill = new Skill();
			skill.skillID = (int)row[s_ID];
			int skilltype = (int)row[s_Type];
			skill.skillType = (SkillType)skilltype;
			skill.name = (string)row[s_Name];
			skill.description = (string)row[s_Desc];
			skill.spriteID = (int)row[s_spriteID];

			return skill;
		}
	}

}
