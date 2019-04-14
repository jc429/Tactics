using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public enum SkillTriggerID{
	TT_NONE = 0,
	TT_ALWAYS_ACTIVE = 1,
	TT_TURN_START = 2,
	TT_TURN_END = 3,
	TT_COMBAT_START = 4,
	TT_COMBAT_END = 5,
	TT_RECEIVE_DAMAGE = 6,
	TT_SPECIAL_ACTIVATE = 7,
	TT_ASSIST_USED = 8,
}

public static class SkillTriggerIDExtensions{
	public static SkillTriggerID GetTriggerID(string input){
		SkillTriggerID id;
		if(Enum.TryParse(input, true, out id)){
			return id;
		}
		else{
			return SkillTriggerID.TT_NONE;
		}
	}

	/* opens up the database and prints all the TriggerIDs, which can then be easily pasted above */
	public static void PrintAllEnumIDs(){
		using (IDbConnection dbConnection = new SqliteConnection(SkillDBReader.ConnectionString)){
			dbConnection.Open();
			using(IDbCommand dbCmd = dbConnection.CreateCommand()){

				string sqlQuery = "SELECT * FROM " + SkillDBReader.DBStrings.triggers;
				dbCmd.CommandText = sqlQuery;
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int ordID = reader.GetOrdinal("Trigger ID");
					int ordNo = reader.GetOrdinal("Trigger No");
					if(ordNo < 0 || ordID < 0){
						Debug.Log("ERROR: Column not found, aborting");
						return;
					}
					int rowcount = 0;
					string result = "public enum SkillTriggerID{\n";
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