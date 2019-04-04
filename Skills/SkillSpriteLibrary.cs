using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillSpriteLibrary{
  
	static Dictionary<int, Sprite> spriteLibrary;

	public static void InitializeSpriteLibrary(Texture2D skillSpriteSheet){
		spriteLibrary = new Dictionary<int, Sprite>();

		Sprite[] sprites = Resources.LoadAll<Sprite>(skillSpriteSheet.name);

		for (int i = 0; i < sprites.Length; i++){
			spriteLibrary.Add(i, sprites[i]);
		}

		Debug.Log("Skill sprite library initialized; " + sprites.Length + " sprites loaded.");
	}

	public static Sprite GetSpriteByID(int spriteID){
		if(spriteID < 0 || spriteID >= spriteLibrary.Count){
			return null;
		}
		return spriteLibrary[spriteID];
	}
}
