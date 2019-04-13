using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public static class SkillDBReader{

	static bool DEBUG_LOG_QUERIES = true;
	static bool DEBUG_LOG_RESULTS = true;

	public static class DBStrings{
		public const string skillInfo = "\"Skill Info\" ";
		public const string skillLinks = "\"Skill Links\" ";
		public const string conditions = "\"Conditions\" ";
		public const string conditionFamily = "\"Condition Family\" ";
		public const string conditionVars = "\"Condition Vars\" ";
		public const string effects = "\"Effects\" ";
		public const string effectFamily = "\"Effect Family\" ";
		public const string effectVars = "\"Effect Vars\" ";

		public const string selectAllFrom = "SELECT * FROM ";
		public const string naturalLeftOuterJoin = " NATURAL LEFT OUTER JOIN ";
	

		public const string skillNoEquals = " \"Skill No\" = ";
		public const string skillIDEquals = " \"Skill ID\" = ";
		public const string CFIDEquals = " \"CF ID\" = ";
		public const string EFIDEquals = " \"EF ID\" = ";
		public const string ConFamIDEquals = " \"Condition Family ID\" = ";
		public const string EffFamIDEquals = " \"Effect Family ID\" = ";
		public const string conIDEquals = " \"Condition ID\" = ";
		public const string effIDEquals = " \"Effect ID\" = ";
		public const string effTargetEquals = " \"Effect Target\" = ";
	}

	private static string connectionString;
	public static string ConnectionString{
		get{
			return connectionString;
		}
	}

	public static void Initialize(){
        connectionString = "URI=file:" + Application.dataPath + "/Database/skills.db";
		SkillConditionDataList.InitializeSkillConditionsTable(connectionString);
		SkillEffectDataList.InitializeSkillEffectsTable(connectionString);

    }

	
	/* loads a skill from the database. if infoOnly is set true, only loads the user-facing skill display info */
	public static Skill LoadSkill(int skillID, bool infoOnly = false){
		Skill skill = new Skill();
		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();
			bool success;
			success = LoadSkillInfo(skillID, dbConnection, ref skill);

			if(infoOnly){
				dbConnection.Close();
				return skill;
			}

			if(success){
				LoadConditionEffectPairLinks(dbConnection, ref skill);
			}
			dbConnection.Close();
		}
		return skill;
	}

	/* loads all of the front-end skill info (name, description, etc) */
	static bool LoadSkillInfo(int skillID, IDbConnection dbConnection, ref Skill skill){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){

			/* first search for skill info */
			string sqlQuery = DBStrings.selectAllFrom + DBStrings.skillInfo 
					+ "WHERE" + DBStrings.skillNoEquals + skillID;

			dbCmd.CommandText = sqlQuery;
			LogQuery(sqlQuery);
			using(IDataReader reader = dbCmd.ExecuteReader()){
				int ordSNo = reader.GetOrdinal("Skill No");
				int ordSID = reader.GetOrdinal("Skill ID");
				int ordSType = reader.GetOrdinal("Skill Type");
				int ordName = reader.GetOrdinal("Name");
				int ordDesc = reader.GetOrdinal("Description");
				int ordSprID = reader.GetOrdinal("Sprite ID");
				if(ordSNo < 0 || ordSID < 0 || ordSType < 0 || ordName < 0 || ordDesc < 0 || ordSprID < 0 ){
					Debug.Log("ERROR: Column not found, aborting");
					return false;
				}
				bool success = false;
				while(reader.Read()){
					Debug.Log("Found Skill " + reader.GetString(ordSID));
					success = true;
					skill.skillNo = reader.GetInt32(ordSNo);
					skill.skillIDString = reader.GetString(ordSID);
					skill.skillType = SkillTypeExtensions.GetSkillType(reader.GetInt32(ordSType));
					skill.name = reader.GetString(ordName);
					skill.description = reader.GetString(ordDesc);
					skill.spriteID = reader.GetInt32(ordSprID);

					skill.LogSkill();
				}
				reader.Close();
				if(!success){
					Debug.Log("Skill Load failed! Skill ID: " + skillID);
					return false;
				}
			}
		}
		return true;
	}

	/* loads all relevant condition-effect pairs into the skill */
	static bool LoadConditionEffectPairLinks(IDbConnection dbConnection, ref Skill skill){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){

			/* check skill links */
			string sqlQuery = DBStrings.selectAllFrom + DBStrings.skillLinks 
					+ "WHERE" + DBStrings.skillIDEquals + StringExtensions.Enquote(skill.skillIDString);
			dbCmd.CommandText = sqlQuery;
			LogQuery(sqlQuery);
			using(IDataReader reader = dbCmd.ExecuteReader()){
				//each row is a condition/effect pair
				int ordLink = reader.GetOrdinal("Link ID");
				int ordTrigger = reader.GetOrdinal("Trigger Type");
				int ordConF = reader.GetOrdinal("Condition Family ID");
				int ordEffF = reader.GetOrdinal("Effect Family ID");
				if(ordLink < 0 || ordTrigger < 0 || ordConF < 0 || ordEffF < 0){
					Debug.Log("ERROR: Column not found, aborting");
					return false;
				}
				while(reader.Read()){	
					int pairID = reader.GetInt32(ordLink);
					ConditionEffectPair cePair = new ConditionEffectPair();
					cePair.pairID = pairID;
					string triggerType = reader.GetString(ordTrigger);
					cePair.triggerType = SkillTriggerIDExtensions.GetTriggerID(triggerType);
					cePair.conditionFamilyID = reader.GetString(ordConF);
					cePair.effectFamilyID = reader.GetString(ordEffF);

					LoadConditionFamily(dbConnection, skill, ref cePair);
					LoadEffectFamily(dbConnection, skill, ref cePair);

					skill.cePairs.Add(cePair);
				}
				reader.Close();
			}

		}
		return true;		//TODO: fail if something goes wrong
	}

	static bool LoadConditionFamily(IDbConnection dbConnection, Skill skill, ref ConditionEffectPair cePair){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){
			string sqlQuery = DBStrings.selectAllFrom + DBStrings.conditionFamily 
					+ DBStrings.naturalLeftOuterJoin + DBStrings.conditions 
					+ "WHERE" + DBStrings.CFIDEquals + StringExtensions.Enquote(cePair.conditionFamilyID);
			dbCmd.CommandText = sqlQuery;
			LogQuery(sqlQuery);
			using(IDataReader reader = dbCmd.ExecuteReader()){
				int rowCt = 0;
				int ordConNo = reader.GetOrdinal("Condition No");
				if(ordConNo == -1){		//couldnt find condition no column
					Debug.Log("ERROR: Column not found, aborting");
					return false;
				}
				while(reader.Read()){	
					int conditionNo = reader.GetInt32(ordConNo);
					SkillConditionData data = SkillConditionDataList.GetSkillConditionData(conditionNo);
					SkillCondition condition = new SkillCondition(data);

					cePair.conditionFamily.AddSkillCondition(condition); 
					rowCt++;
				}
				LogResult(rowCt + " results found");
				reader.Close();
			}
			/* load variables into the conditions */
			foreach(SkillCondition condition in cePair.conditionFamily.skillConditions){
				//obviously no need to search for vars if the condition doesnt use any
				if(condition.GetVarCount() == 0){
					continue;
				}
				string conQuery = DBStrings.selectAllFrom + DBStrings.conditionVars
						+ "WHERE" + DBStrings.skillIDEquals + StringExtensions.Enquote(skill.skillIDString)
						+ " AND" + DBStrings.ConFamIDEquals + StringExtensions.Enquote(cePair.conditionFamilyID)
						+ " AND" + DBStrings.conIDEquals + StringExtensions.Enquote(condition.GetConditionIDString());
				dbCmd.CommandText = conQuery;
				LogQuery(conQuery);
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int rowCt = 0;	
					int ordCVID = reader.GetOrdinal("CV ID");
					int ordValue = reader.GetOrdinal("Value");
					int ordCVPos = reader.GetOrdinal("Condition Var Position");
					if(ordCVID < 0 || ordValue < 0 || ordCVPos < 0){
						Debug.Log("ERROR: Column not found, aborting");
						return false;
					}
					while(reader.Read()){
						string conditionVarID = reader.GetString(ordCVID);
						int varValue = reader.GetInt32(ordValue);
						int varPosition = reader.GetInt32(ordCVPos);
						condition.SetVariable(varValue,varPosition);
						rowCt++;
					}
					LogResult(rowCt + " results found");
					reader.Close();
				}
			}

		}
		return true;
	}

	static bool LoadEffectFamily(IDbConnection dbConnection, Skill skill, ref ConditionEffectPair cePair){
		using(IDbCommand dbCmd = dbConnection.CreateCommand()){
			string sqlQuery = DBStrings.selectAllFrom + DBStrings.effectFamily 
					+ DBStrings.naturalLeftOuterJoin + DBStrings.effects 
					+ "WHERE" + DBStrings.EFIDEquals + StringExtensions.Enquote(cePair.effectFamilyID);
			dbCmd.CommandText = sqlQuery;
			LogQuery(sqlQuery);
			using(IDataReader reader = dbCmd.ExecuteReader()){
				int rowCt = 0;	
				int ordEffNo = reader.GetOrdinal("Effect No");
				int ordEffTarget = reader.GetOrdinal("Effect Target");
				if(ordEffNo < 0 || ordEffTarget < 0){
					Debug.Log("ERROR: Column not found, aborting");
					return false;
				}
				while(reader.Read()){
					int effectNo = reader.GetInt32(ordEffNo);
					SkillEffectData data = SkillEffectDataList.GetSkillEffectData(effectNo);
					SkillTarget target = SkillTargetExtensions.GetSkillTarget(reader.GetString(ordEffTarget));
					SkillEffect effect = new SkillEffect(data, target);

					cePair.effectFamily.AddSkillEffect(effect); 
					rowCt++;
				}
				LogResult(rowCt + " results found");
				reader.Close();
			}
			/* load variables into the effects */
			foreach(SkillEffect effect in cePair.effectFamily.skillEffects){
				//obviously no need to search for vars if the effect doesnt use any
				if(effect.GetVarCount() == 0){
					continue;
				}
				string effQuery = DBStrings.selectAllFrom + DBStrings.effectVars
						+ "WHERE" + DBStrings.skillIDEquals + StringExtensions.Enquote(skill.skillIDString)
						+ " AND" + DBStrings.EffFamIDEquals + StringExtensions.Enquote(cePair.effectFamilyID)
						+ " AND" + DBStrings.effIDEquals + StringExtensions.Enquote(effect.GetEffectIDString())
						+ " AND" + DBStrings.effTargetEquals + StringExtensions.Enquote(effect.GetTargetString());
				dbCmd.CommandText = effQuery;
				LogQuery(effQuery);
				using(IDataReader reader = dbCmd.ExecuteReader()){
					int rowCt = 0;	
					int ordEVID = reader.GetOrdinal("EV ID");
					int ordValue = reader.GetOrdinal("Value");
					int ordEVPos = reader.GetOrdinal("Effect Var Position");
					if(ordEVID < 0 || ordValue < 0 || ordEVPos < 0){
						Debug.Log("ERROR: Column not found: " + Mathf.Min(ordEVID,ordEVPos,ordValue) + ", aborting");
						return false;
					}
					while(reader.Read()){
						string effectVarID = reader.GetString(ordEVID);
						int varValue = reader.GetInt32(ordValue);
						int varPosition = reader.GetInt32(ordEVPos);
						effect.SetVariable(varValue,varPosition);
						rowCt++;
					}
					LogResult(rowCt + " results found");
					reader.Close();
				}
			}
		}
		return true;
	}


	static void LogQuery(string query){
		if(DEBUG_LOG_QUERIES){
			Debug.Log("Query: " + query);
		}
	}

	static void LogResult(string result){
		if(DEBUG_LOG_RESULTS){
			Debug.Log(result);
		}
	}
}
