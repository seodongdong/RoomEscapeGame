using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 저장 데이터
///
/// [추가]
/// - playTimeSeconds: 누적 플레이 시간(초). 슬롯 목록에 "플레이 후 지난 시간"으로 표시하기 위함.
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
	public float playTimeSeconds; // ★ 추가: 누적 플레이 시간(초)

	public GameData()
	{
		collectedClues = new List<string>();
		solvedPuzzles = new bool[5];
		hasCamcorder = false;
		playTimeSeconds = 0f;
	}
}