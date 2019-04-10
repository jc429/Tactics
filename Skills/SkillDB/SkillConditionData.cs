using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class SkillConditionData{
	public int scNo;
	public ConditionID cID;
	public int varCount;

	public SkillConditionData(int no, string ID, int vCount){
		scNo = no;
		cID = ConditionIDExtensions.GetConditionID(ID);
		varCount = vCount;
	}
}


public static class SkillConditionDataList{

	static Dictionary<int,SkillConditionData> conditionsTable;

	/* opens the sqlite database and reads in all the skill conditions */
	public static void InitializeSkillConditionsTable(string connectionString){
		conditionsTable = new Dictionary<int, SkillConditionData>();

		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();
			using(IDbCommand dbCmd = dbConnection.CreateCommand()){
				string sqlQuery = SkillDBReader.DBStrings.selectAll + SkillDBReader.DBStrings.conditions;

				dbCmd.CommandText = sqlQuery;
				//Debug.Log(sqlQuery);
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int rowcount = 0;
					while(reader.Read()){
						SkillConditionData condition = new SkillConditionData(reader.GetInt32(0),reader.GetString(1),reader.GetInt32(2));
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
	public static SkillConditionData GetSkillConditionData(int conditionNo){
		return conditionsTable[conditionNo];
	}

}
