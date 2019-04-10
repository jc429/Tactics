using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public static class SkillDBReader{

	static bool DEBUG_LOG_QUERIES = true;

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
		public const string naturalLeftOuterJoin = " NATURAL LEFT OUTER JOIN ";
	
		public const string whereSkillNoEquals = " WHERE \"Skill No\" = ";
		public const string whereSkillIDEquals = " WHERE \"Skill ID\" = ";
		public const string whereCFIDEquals = " WHERE \"CF ID\" = ";
		public const string whereEFIDEquals = " WHERE \"EF ID\" = ";
	}

	private static string connectionString;


	public static void Initialize(){
        connectionString = "URI=file:" + Application.dataPath + "/Database/skills.db";
		SkillConditionDataList.InitializeSkillConditionsTable(connectionString);
		SkillEffectDataList.InitializeSkillEffectsTable(connectionString);

		ReadData();
    }


	static void ReadData(){
		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();

		//	PrintSkillInfo(dbConnection);
		//	PrintSkillLinks(dbConnection);

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

	public static Skill LoadSkill(int skillID){
		Skill skill = new Skill();
		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();

			LoadSkillInfo(skillID, dbConnection, ref skill);
			LoadConditionEffectPairLinks(dbConnection, ref skill);


			
			dbConnection.Close();
		}
		return skill;
	}

	/* loads all of the front-end skill info (name, description, etc) */
	static bool LoadSkillInfo(int skillID, IDbConnection dbConnection, ref Skill skill){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){

			/* first search for skill info */
			string sqlQuery = DBStrings.selectAll + DBStrings.skillInfo 
					//+ DBStrings.naturalLeftOuterJoin + DBStrings.skillLinks 
					+ DBStrings.whereSkillNoEquals + skillID;

			dbCmd.CommandText = sqlQuery;
			LogQuery(sqlQuery);
			using(IDataReader reader = dbCmd.ExecuteReader()){
				while(reader.Read()){
					Debug.Log("Found Skill " + reader.GetString(1));
					skill.skillID = reader.GetInt32(0);
					skill.skillIDString = reader.GetString(1);
					skill.skillType = SkillTypeExtensions.GetSkillType(reader.GetInt32(2));
					skill.name = reader.GetString(3);
					skill.description = reader.GetString(4);
					skill.spriteID = reader.GetInt32(5);

					skill.LogSkill();
				}
				reader.Close();
			}
		}
		return true;		//TODO: fail if something goes wrong
	}

	/* loads all relevant condition-effect pairs into the skill */
	static bool LoadConditionEffectPairLinks(IDbConnection dbConnection, ref Skill skill){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){

			/* check skill links */
			string sqlQuery = DBStrings.selectAll + DBStrings.skillLinks 
					+ DBStrings.whereSkillIDEquals + StringExtensions.Enquote(skill.skillIDString);
			dbCmd.CommandText = sqlQuery;
			LogQuery(sqlQuery);
			using(IDataReader reader = dbCmd.ExecuteReader()){
				//each row is a condition/effect pair
				while(reader.Read()){	
					int pairID = reader.GetInt32(0);
					ConditionEffectPair cePair = new ConditionEffectPair();
					cePair.pairID = pairID;
					//cePair.triggerType = reader.GetInt32(2); //FIXME LATER
					cePair.conditionsID = reader.GetString(3);
					cePair.effectsID = reader.GetString(4);

					LoadConditionFamily(dbConnection, ref cePair);
					LoadEffectFamily(dbConnection, ref cePair);

					skill.cePairs.Add(cePair);
				}
				reader.Close();
			}

		}
		return true;		//TODO: fail if something goes wrong
	}

	static bool LoadConditionFamily(IDbConnection dbConnection, ref ConditionEffectPair cePair){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){
			string sqlQuery = DBStrings.selectAll + DBStrings.conditionFamily 
					+ DBStrings.naturalLeftOuterJoin + DBStrings.conditions 
					+ DBStrings.whereCFIDEquals + StringExtensions.Enquote(cePair.conditionsID);
			dbCmd.CommandText = sqlQuery;
			LogQuery(sqlQuery);
			using(IDataReader reader = dbCmd.ExecuteReader()){
				int rowCt = 0;
				int ord = reader.GetOrdinal("Condition No");
				if(ord == -1){		//couldnt find condition no column
					return false;
				}
				while(reader.Read()){	
					int conditionNo = reader.GetInt32(ord);
					SkillConditionData data = SkillConditionDataList.GetSkillConditionData(conditionNo);
					SkillCondition condition = new SkillCondition(data);

					cePair.conditionFamily.AddSkillCondition(condition); 
					rowCt++;
				}
				LogQuery(rowCt + " results found");
				reader.Close();
			}
		}
		return true;
	}

	static bool LoadEffectFamily(IDbConnection dbConnection, ref ConditionEffectPair cePair){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){
			string sqlQuery = DBStrings.selectAll + DBStrings.effectFamily 
					+ DBStrings.naturalLeftOuterJoin + DBStrings.effects 
					+ DBStrings.whereEFIDEquals + StringExtensions.Enquote(cePair.effectsID);
			dbCmd.CommandText = sqlQuery;
			LogQuery(sqlQuery);
			using(IDataReader reader = dbCmd.ExecuteReader()){
				int rowCt = 0;	
				int ord = reader.GetOrdinal("Effect No");
				if(ord == -1){		//couldnt find effect no column
					return false;
				}
				while(reader.Read()){
					int effectNo = reader.GetInt32(ord);
					SkillEffectData data = SkillEffectDataList.GetSkillEffectData(effectNo);
					SkillEffect effect = new SkillEffect(data);

					cePair.effectFamily.AddSkillEffect(effect); 
					rowCt++;
				}
				LogQuery(rowCt + " results found");
				reader.Close();
			}
		}
		return true;
	}



	static void LogQuery(string query){
		if(DEBUG_LOG_QUERIES){
			Debug.Log(query);
		}
	}
}
