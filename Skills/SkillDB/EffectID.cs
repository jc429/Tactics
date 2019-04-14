using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public enum EffectID{
	EFF_NONE = 0,
	EFF_MODIFY_STAT = 1,
	EFF_ADD_SPD = 2,
	EFF_ADD_DEF = 3,
	EFF_ADD_RES = 4,
	EFF_ADD_HP = 5,
	EFF_ADD_SKL = 6
}

public static class EffectIDExtensions{

	public static EffectID GetEffectID(string input){
		EffectID id;
		if(Enum.TryParse(input, true, out id)){
			return id;
		}
		else{
			return EffectID.EFF_NONE;
		}
	}
	

	/* opens up the database and prints all the EffectIDs, which can then be easily pasted above */
	public static void PrintAllEnumIDs(){
		using (IDbConnection dbConnection = new SqliteConnection(SkillDBReader.ConnectionString)){
			dbConnection.Open();
			using(IDbCommand dbCmd = dbConnection.CreateCommand()){

				string sqlQuery = "SELECT \"Effect ID\",\"Effect No\" FROM " + SkillDBReader.DBStrings.effects;
				dbCmd.CommandText = sqlQuery;
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int ordID = reader.GetOrdinal("Effect ID");
					int ordNo = reader.GetOrdinal("Effect No");
					if(ordNo < 0 || ordID < 0){
						Debug.Log("ERROR: Column not found, aborting");
						return;
					}
					int rowcount = 0;
					string result = "public enum EffectID{\n";
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