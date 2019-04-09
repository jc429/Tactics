using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public static class SkillDBReader{

	public static class DBStrings{
		public const string skillInfo = "\"Skill Info\"";
		public const string skillLinks = "\"Skill Links\"";
		public const string conditions = "\"Conditions\"";
		public const string conditionFamily = "\"Condition Family\"";
		public const string conditionVars = "\"Condition Vars\"";
		public const string effects = "\"Effects\"";
		public const string effectFamily = "\"Effect Family\"";
		public const string effectVars = "\"Effect Vars\"";

		public const string selectAll = "SELECT * FROM ";
		public const string naturalLeftOuterJoin = "NATURAL LEFT OUTER JOIN ";

		public const string whereSkillNoEquals = " WHERE \"Skill No\" = ";
	}

	private static string connectionString;


	public static void Initialize(){
        connectionString = "URI=file:" + Application.dataPath + "/Database/skills.db";
		SkillConditions.InitializeSkillConditionsTable(connectionString);
		SkillEffects.InitializeSkillEffectsTable(connectionString);

		ReadData();
    }


	static void ReadData(){
		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();

		//	PrintSkillInfo(dbConnection);
		//	PrintSkillLinks(dbConnection);

			GetSkill(1);

			dbConnection.Close();
		}
	}

	static void PrintSkillInfo(IDbConnection dbConnection){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){
			string sqlQuery = DBStrings.selectAll + DBStrings.skillInfo;
			dbCmd.CommandText = sqlQuery;

			using(IDataReader reader = dbCmd.ExecuteReader()){
				while(reader.Read()){	//reads a line
					Debug.Log(reader.GetString(1));
				}
				reader.Close();
			}
		}
	}

	static void PrintSkillLinks(IDbConnection dbConnection){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){
			string sqlQuery = DBStrings.selectAll + DBStrings.skillLinks;
			dbCmd.CommandText = sqlQuery;

			using(IDataReader reader = dbCmd.ExecuteReader()){
				while(reader.Read()){
					Debug.Log(reader.GetString(1));
				}
				reader.Close();
			}
		}
	}

	public static Skill GetSkill(int skillID){
		Skill skill = new Skill();
		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();
			using(IDbCommand dbCmd = dbConnection.CreateCommand()){

				/* search for skill info */
				string sqlQuery = DBStrings.selectAll + DBStrings.skillInfo 
						//+ DBStrings.naturalLeftOuterJoin + DBStrings.skillLinks 
						+ DBStrings.whereSkillNoEquals + skillID;

				dbCmd.CommandText = sqlQuery;
				Debug.Log(sqlQuery);
				using(IDataReader reader = dbCmd.ExecuteReader()){
					while(reader.Read()){
						Debug.Log("Found Skill " + reader.GetString(1));
						skill.skillID = reader.GetInt32(0);

						skill.skillType = SkillTypeExtensions.GetSkillType(reader.GetInt32(2));
						skill.name = reader.GetString(3);
						skill.description = reader.GetString(4);
						skill.spriteID = reader.GetInt32(5);

						skill.LogSkill();
					}
					reader.Close();
				}

				/* skill links */
				/*sqlQuery = DBStrings.selectAll + DBStrings.skillLinks + DBStrings.skillNoEquals + skillID;
				dbCmd.CommandText = sqlQuery;
				Debug.Log(sqlQuery);
				using(IDataReader reader = dbCmd.ExecuteReader()){
					while(reader.Read()){
						
					}
					reader.Close();
				}*/
			}
			dbConnection.Close();
		}
		return skill;
	}
}
