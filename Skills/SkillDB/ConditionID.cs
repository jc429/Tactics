using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public enum ConditionID{
	CN_NONE = 0,
	CN_USER_INITIATES_COMBAT = 1,
	CN_FOE_INITIATES_COMBAT = 2,
	CN_HP_LESS_EQUAL_X = 3,
	CN_HP_GREATER_EQUAL_X = 4,
	CN_STAT_X_IS_Y_GREATER_FOE = 5,
	CN_STAT_X_IS_Y_LESS_FOE = 6,
	CN_X_ALLIES_IN_Y_RANGE = 7,
	CN_X_FOES_IN_Y_RANGE = 8,
	CN_HAS_ADJACENT_ALLY = 9,
	CN_IS_BUFFED = 10,
	CN_IS_DEBUFFED = 11,
}

public static class ConditionIDExtensions{

	public static ConditionID GetConditionID(string input){
		ConditionID id;
		if(Enum.TryParse(input, true, out id)){
			return id;
		}
		else{
			return ConditionID.CN_NONE;
		}
	}

	/* opens up the database and prints all the ConditionIDs, which can then be easily pasted above */
	public static void PrintAllEnumIDs(){
		using (IDbConnection dbConnection = new SqliteConnection(SkillDBReader.ConnectionString)){
			dbConnection.Open();
			using(IDbCommand dbCmd = dbConnection.CreateCommand()){

				string sqlQuery = "SELECT \"Condition ID\",\"Condition No\" FROM " + SkillDBReader.DBStrings.conditions;
				dbCmd.CommandText = sqlQuery;
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int ordID = reader.GetOrdinal("Condition ID");
					int ordNo = reader.GetOrdinal("Condition No");
					if(ordNo < 0 || ordID < 0){
						Debug.Log("ERROR: Column not found, aborting");
						return;
					}
					int rowcount = 0;
					string result = "public enum ConditionID{\n";
					while(reader.Read()){
						string s = "\t" + reader.GetString(ordID) + " = " + reader.GetInt32(ordNo) + ",\n";
						result += s;
						rowcount++;
					}
					result += "}";
					Debug.Log(result);
				}
			}

			dbConnection.Close();
		}
	}
	
}