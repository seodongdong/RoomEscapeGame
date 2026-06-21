using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
	public int currentStage;
	public string sceneName;
	public string savedDisplayName;
	public Vector3 playerPosition;
	public List<string> collectedClues;
	public int health;
	public bool[] solvedPuzzles;
	public bool hasCamcorder;
	public float playTimeSeconds;
	public bool hasFlashlight; // ★ 추가

	public GameData()
	{
		collectedClues = new List<string>();
		solvedPuzzles = new bool[5];
		hasCamcorder = false;
		playTimeSeconds = 0f;
		sceneName = "";
		savedDisplayName = "";
		hasFlashlight = false;
	}
}