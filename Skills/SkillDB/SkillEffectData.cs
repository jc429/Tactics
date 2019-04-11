using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class SkillEffectData{
	public int seNo;
	public EffectID eID;
	public int varCount;

	public SkillEffectData(int no, string ID, int vCount){
		seNo = no;
		eID = EffectIDExtensions.GetEffectID(ID);
		varCount = vCount;
	}
}


public static class SkillEffectDataList{

	static Dictionary<int,SkillEffectData> effectsTable;

	/* opens the sqlite database and reads in all the skill effects */
	public static void InitializeSkillEffectsTable(string connectionString){
		effectsTable = new Dictionary<int, SkillEffectData>();

		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();
			using(IDbCommand dbCmd = dbConnection.CreateCommand()){
				string sqlQuery = SkillDBReader.DBStrings.selectAllFrom + SkillDBReader.DBStrings.effects;

				dbCmd.CommandText = sqlQuery;
				//Debug.Log(sqlQuery);
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int ordEffNo = reader.GetOrdinal("Effect No");
					int ordEffID = reader.GetOrdinal("Effect ID");
					int ordVarCt = reader.GetOrdinal("Var Count");
					if(ordEffNo < 0 || ordEffID < 0 || ordVarCt < 0){
						Debug.Log("ERROR: Column not found, aborting");
						return;
					}
					int rowcount = 0;
					while(reader.Read()){
						SkillEffectData effect = new SkillEffectData(reader.GetInt32(ordEffNo),
								reader.GetString(ordEffID),reader.GetInt32(ordVarCt));
						effectsTable.Add(reader.GetInt32(ordEffNo),effect);
						rowcount++;
					}
					Debug.Log("Skill Effects initialized: " + rowcount + " entries evaluated.");
				}

			}
			dbConnection.Close();
		}

	}

	/* grabs a skill effect from the table */
	public static SkillEffectData GetSkillEffectData(int effectNo){
		return effectsTable[effectNo];
	}

}