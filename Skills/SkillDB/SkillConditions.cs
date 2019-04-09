using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class SkillCondition{
	public int scNo;
	public string scID;
	public int varCount;

	public SkillCondition(int no, string ID, int vCount){
		scNo = no;
		scID = ID;
		varCount = vCount;
	}
}


public static class SkillConditions{

	static Dictionary<int,SkillCondition> conditionsTable;

	/* opens the sqlite database and reads in all the skill conditions */
	public static void InitializeSkillConditionsTable(string connectionString){
		conditionsTable = new Dictionary<int, SkillCondition>();

		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();
			using(IDbCommand dbCmd = dbConnection.CreateCommand()){
				string sqlQuery = SkillDBReader.DBStrings.selectAll + SkillDBReader.DBStrings.conditions;

				dbCmd.CommandText = sqlQuery;
				//Debug.Log(sqlQuery);
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int rowcount = 0;
					while(reader.Read()){
						SkillCondition condition = new SkillCondition(reader.GetInt32(0),reader.GetString(1),reader.GetInt32(2));
						conditionsTable.Add(reader.GetInt32(0),condition);
						rowcount++;
					}
					Debug.Log("Skill Conditions initialized: " + rowcount + " entries evaluated.");
				}

			}
			dbConnection.Close();
		}

	}
	
	/* grabs a skill condition from the table */
	public static SkillCondition GetSkillCondition(int conditionNo){
		return conditionsTable[conditionNo];
	}

}
