using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class SkillEffect{
	public int scNo;
	public string scID;
	public int varCount;

	public SkillEffect(int no, string ID, int vCount){
		scNo = no;
		scID = ID;
		varCount = vCount;
	}
}


public static class SkillEffects{

	static Dictionary<int,SkillEffect> effectsTable;

	public static void InitializeSkillEffectsTable(string connectionString){
		effectsTable = new Dictionary<int, SkillEffect>();

		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();
			using(IDbCommand dbCmd = dbConnection.CreateCommand()){
				string sqlQuery = SkillDBReader.DBStrings.selectAll + SkillDBReader.DBStrings.effects;

				dbCmd.CommandText = sqlQuery;
				//Debug.Log(sqlQuery);
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int rowcount = 0;
					while(reader.Read()){
						SkillEffect effect = new SkillEffect(reader.GetInt32(0),reader.GetString(1),reader.GetInt32(2));
						effectsTable.Add(reader.GetInt32(0),effect);
						rowcount++;
					}
					Debug.Log("Skill Effects initialized: " + rowcount + " entries evaluated.");
				}

			}
			dbConnection.Close();
		}

	}

}