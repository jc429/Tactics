using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public enum SkillTarget{
	TARGET_NONE = 0,
	TARGET_SELF = 1,
	TARGET_COMBAT_FOE = 2,
	TARGET_ADJACENT_ALLIES = 3,
	TARGET_ADJACENT_FOES = 4,
	TARGET_ALLIES_WITHIN_2_TILES = 5,
	TARGET_FOES_WITHIN_2_TILES = 6,
	TARGET_ALLIES_ON_AXIS = 7,
	TARGET_FOES_ON_AXIS = 8,
}


public static class SkillTargetExtensions{

	public static SkillTarget GetSkillTarget(string input){
		SkillTarget id;
		if(Enum.TryParse(input, true, out id)){
			return id;
		}
		else{
			return SkillTarget.TARGET_NONE;
		}
	}
	/* opens up the database and prints all the Target IDs, which can then be easily pasted above */
	public static void PrintAllEnumIDs(){
		using (IDbConnection dbConnection = new SqliteConnection(SkillDBReader.ConnectionString)){
			dbConnection.Open();
			using(IDbCommand dbCmd = dbConnection.CreateCommand()){

				string sqlQuery = "SELECT * FROM " + SkillDBReader.DBStrings.targets;
				dbCmd.CommandText = sqlQuery;
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int ordID = reader.GetOrdinal("Target ID");
					int ordNo = reader.GetOrdinal("Target No");
					if(ordNo < 0 || ordID < 0){
						Debug.Log("ERROR: Column not found, aborting");
						return;
					}
					int rowcount = 0;
					string result = "public enum SkillTarget{\n";
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