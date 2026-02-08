using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 저장 데이터
/// </summary>
[System.Serializable]
public class GameData
{
	public int currentStage;
	public Vector3 playerPosition;
	public List<string> collectedClues;
	public int health;
	public bool[] solvedPuzzles;
	public bool hasCamcorder; // 진엔딩 조건

	public GameData()
	{
		collectedClues = new List<string>();
		solvedPuzzles = new bool[5];
		hasCamcorder = false;
	}
}