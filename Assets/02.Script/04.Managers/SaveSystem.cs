using UnityEngine;

/// <summary>
/// 저장 시스템
/// </summary>
public class SaveSystem : MonoBehaviour, ISaveSystem
{
	private const string SAVE_KEY = "HorrorGame_Save";

	public void SaveGame(GameData data)
	{
		string json = JsonUtility.ToJson(data);
		PlayerPrefs.SetString(SAVE_KEY, json);
		PlayerPrefs.Save();

		Debug.Log("[SaveSystem] 게임 저장 완료");
	}

	public GameData LoadGame()
	{
		if (PlayerPrefs.HasKey(SAVE_KEY))
		{
			string json = PlayerPrefs.GetString(SAVE_KEY);
			GameData data = JsonUtility.FromJson<GameData>(json);

			Debug.Log("[SaveSystem] 게임 로드 완료");
			return data;
		}

		Debug.Log("[SaveSystem] 저장 파일 없음");
		return null;
	}

	public bool HasSaveFile()
	{
		return PlayerPrefs.HasKey(SAVE_KEY);
	}

	public void DeleteSave()
	{
		PlayerPrefs.DeleteKey(SAVE_KEY);
		PlayerPrefs.Save();

		Debug.Log("[SaveSystem] 저장 파일 삭제");
	}
}