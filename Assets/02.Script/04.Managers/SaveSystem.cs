using UnityEngine;

public class SaveSystem : MonoBehaviour, ISaveSystem
{
	private const string SAVE_KEY_PREFIX = "HorrorGame_Save_";
	public const int SLOT_COUNT = 4;

	private static string GetKey(int slotIndex) => $"{SAVE_KEY_PREFIX}{slotIndex}";

	public void SaveGame(GameData data) => SaveGame(0, data);
	public GameData LoadGame() => LoadGame(0);
	public bool HasSaveFile() => HasSaveFile(0);
	public void DeleteSave() => DeleteSave(0);




	public void SaveGame(int slotIndex, GameData data)
	{
		if (!IsValidSlot(slotIndex)) return;

		string json = JsonUtility.ToJson(data);
		PlayerPrefs.SetString(GetKey(slotIndex), json);
		PlayerPrefs.Save();

		Debug.Log($"[SaveSystem] 슬롯 {slotIndex} 저장 완료");
	}

	public GameData LoadGame(int slotIndex)
	{
		if (!IsValidSlot(slotIndex)) return null;

		string key = GetKey(slotIndex);
		if (PlayerPrefs.HasKey(key))
		{
			string json = PlayerPrefs.GetString(key);
			GameData data = JsonUtility.FromJson<GameData>(json);

			Debug.Log($"[SaveSystem] 슬롯 {slotIndex} 로드 완료");
			return data;
		}

		Debug.Log($"[SaveSystem] 슬롯 {slotIndex} 저장 파일 없음");
		return null;
	}

	public bool HasSaveFile(int slotIndex)
	{
		if (!IsValidSlot(slotIndex)) return false;
		return PlayerPrefs.HasKey(GetKey(slotIndex));
	}

	public void DeleteSave(int slotIndex)
	{
		if (!IsValidSlot(slotIndex)) return;

		PlayerPrefs.DeleteKey(GetKey(slotIndex));
		PlayerPrefs.Save();

		Debug.Log($"[SaveSystem] 슬롯 {slotIndex} 저장 파일 삭제");
	}

	/// <summary>
	/// ★ 수정: stageName, playTimeSeconds를 포함하도록 확장.
	/// </summary>
	public SaveSlotInfo GetSlotInfo(int slotIndex)
	{
		var info = new SaveSlotInfo { hasSave = false, currentStage = 0, stageName = "", playTimeSeconds = 0f };

		if (!HasSaveFile(slotIndex)) return info;

		var data = LoadGame(slotIndex);
		if (data == null) return info;

		info.hasSave = true;
		info.currentStage = data.currentStage;
		info.stageName = GetStageName(data.currentStage); // ★ 추가
		info.playTimeSeconds = data.playTimeSeconds;       // ★ 추가
		return info;
	}

	/// <summary>
	/// ★ 추가: 스테이지 번호 → 기획서상 장소 이름 매핑.
	/// StageManager.GetSceneName()의 씬 이름 매핑과 동일한 기준(기획서 6장 씬 구성)을 따릅니다.
	/// </summary>
	private string GetStageName(int stageNumber)
	{
		switch (stageNumber)
		{
			case 1: return "거실";
			case 2: return "장례식장";
			case 3: return "미로";
			case 4: return "주방";
			case 5: return "지하실";
			default: return $"스테이지 {stageNumber}";
		}
	}

	private bool IsValidSlot(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= SLOT_COUNT)
		{
			Debug.LogError($"[SaveSystem] 잘못된 슬롯 인덱스: {slotIndex} (유효 범위: 0~{SLOT_COUNT - 1})");
			return false;
		}
		return true;
	}
}