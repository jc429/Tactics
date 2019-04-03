using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeywordInfo{
	public string name;
	public string description;
}

public static class KeywordTable{

	static Dictionary<string,string> keywordDictionary;
	
	public static void InitializeKeywordTable(TextAsset csv){
		keywordDictionary = new Dictionary<string, string>(System.StringComparer.InvariantCultureIgnoreCase);

		
		string[,] temp = CSVReader.SplitCsvGrid(csv.text); 

		//y starts at 1 to skip the first row (which is just headers)
		for (int y = 1; y < temp.GetUpperBound(1); y++) {
			string skillName = temp[0,y];
			string skillDesc = temp[1,y];
			if(skillName != null){
				keywordDictionary.Add(skillName,skillDesc);
			}
		}

		
		Debug.Log("Initialization complete; " + keywordDictionary.Count + " key/value pairs processed");
	}


	public static KeywordInfo GetKeywordInfo(string keyName){
		KeywordInfo keyword = new KeywordInfo();
		keyword.name = keyName;
		keyword.description = keywordDictionary[keyName];

		return keyword;
	}
}
