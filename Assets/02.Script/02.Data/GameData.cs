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
	public bool hasFlashlight;

	// ★ 추가: 씬 내 오브젝트 상태 저장
	// key = 오브젝트 고유 ID (ISaveableObject.SaveId)
	// value = 상태 문자열 (JSON 직렬화)
	public List<string> savedObjectIds;    // 상태 저장된 오브젝트 ID 목록
	public List<string> savedObjectStates; // 각 오브젝트의 직렬화된 상태

	public GameData()
	{
		collectedClues = new List<string>();
		solvedPuzzles = new bool[5];
		hasCamcorder = false;
		playTimeSeconds = 0f;
		sceneName = "";
		savedDisplayName = "";
		hasFlashlight = false;
		savedObjectIds = new List<string>();
		savedObjectStates = new List<string>();
	}

	/// <summary>오브젝트 상태를 저장합니다.</summary>
	public void SetObjectState(string id, string stateJson)
	{
		int idx = savedObjectIds.IndexOf(id);
		if (idx >= 0)
			savedObjectStates[idx] = stateJson;
		else
		{
			savedObjectIds.Add(id);
			savedObjectStates.Add(stateJson);
		}
	}

	/// <summary>오브젝트 상태를 불러옵니다. 없으면 null 반환.</summary>
	public string GetObjectState(string id)
	{
		int idx = savedObjectIds.IndexOf(id);
		return idx >= 0 ? savedObjectStates[idx] : null;
	}
}