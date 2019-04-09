using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

public class SQLReader : MonoBehaviour{

	private string connectionString;

	public Skill skill;

    // Start is called before the first frame update
    void Start()
    {
       SkillDBReader.Initialize();

		skill = SkillDBReader.GetSkill(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void ReadData(){
		using (IDbConnection dbConnection = new SqliteConnection(connectionString)){
			dbConnection.Open();

			using(IDbCommand dbCmd = dbConnection.CreateCommand()){
				string sqlQuery = "SELECT * FROM \"Skill Info\"";
				dbCmd.CommandText = sqlQuery;

				using(IDataReader reader = dbCmd.ExecuteReader()){
					while(reader.Read()){
						Debug.Log(reader.GetString(1));
					}
					reader.Close();
					dbConnection.Close();
				}
			}
		}
	}
}
