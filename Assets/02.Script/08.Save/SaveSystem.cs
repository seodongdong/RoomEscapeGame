using UnityEngine;
using System.Linq;

public class SaveSystem : MonoBehaviour, ISaveSystem
{
	private const string SAVE_KEY_PREFIX = "HorrorGame_Save_";
	private const string CHASE_AUTOSAVE_KEY = "HorrorGame_ChaseAutosave";
	public const int SLOT_COUNT = 4;

	private static string GetKey(int slotIndex) => $"{SAVE_KEY_PREFIX}{slotIndex}";

	public void SaveGame(GameData data) => SaveGame(0, data);
	public GameData LoadGame() => LoadGame(0);
	public bool HasSaveFile() => HasSaveFile(0);
	public void DeleteSave() => DeleteSave(0);

	public void SaveGame(int slotIndex, GameData data)
	{
		if (!IsValidSlot(slotIndex)) return;

		// ★ 씬의 모든 ISaveableObject 상태 수집
		var saveables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
			UnityEngine.FindObjectsSortMode.None).OfType<ISaveableObject>();

		foreach (var s in saveables)
		{
			if (!string.IsNullOrEmpty(s.SaveId))
				data.SetObjectState(s.SaveId, s.SaveState());
		}

		string json = JsonUtility.ToJson(data);
		PlayerPrefs.SetString(GetKey(slotIndex), json);
		PlayerPrefs.Save();
		DeleteChaseAutosave();

		Debug.Log($"[SaveSystem] 슬롯 {slotIndex} 저장 완료 (씬: {data.sceneName}, 오브젝트 {data.savedObjectIds.Count}개)");
	}

	public GameData LoadGame(int slotIndex)
	{
		if (!IsValidSlot(slotIndex)) return null;

		string key = GetKey(slotIndex);
		if (PlayerPrefs.HasKey(key))
		{
			string json = PlayerPrefs.GetString(key);
			GameData data = JsonUtility.FromJson<GameData>(json);

			Debug.Log($"[SaveSystem] 슬롯 {slotIndex} 로드 완료 (씬: {data.sceneName})");
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

	public SaveSlotInfo GetSlotInfo(int slotIndex)
	{
		var info = new SaveSlotInfo { hasSave = false, currentStage = 0, stageName = "", playTimeSeconds = 0f };

		if (!HasSaveFile(slotIndex)) return info;

		var data = LoadGame(slotIndex);
		if (data == null) return info;

		info.hasSave = true;
		info.currentStage = data.currentStage;

		// ★ 수정: 저장 시점에 함께 기록된 표시 이름(savedDisplayName)을 우선 사용.
		// 없으면(이전 버전 데이터 등) 씬 이름 기반 매핑으로 폴백.
		info.stageName = !string.IsNullOrEmpty(data.savedDisplayName)
			? data.savedDisplayName
			: (!string.IsNullOrEmpty(data.sceneName)
				? data.sceneName // 매핑 자체도 없으면 씬 이름을 그대로 표시
				: GetStageNameByNumber(data.currentStage));

		info.playTimeSeconds = data.playTimeSeconds;
		return info;
	}

	public void SaveChaseAutosave(GameData data)
	{
		string json = JsonUtility.ToJson(data);
		PlayerPrefs.SetString(CHASE_AUTOSAVE_KEY, json);
		PlayerPrefs.Save();

		Debug.Log($"[SaveSystem] 추격전 자동 저장 완료 (씬: {data.sceneName})");
	}

	public GameData LoadChaseAutosave()
	{
		if (!PlayerPrefs.HasKey(CHASE_AUTOSAVE_KEY))
		{
			Debug.LogWarning("[SaveSystem] 추격전 자동 저장 데이터가 없습니다.");
			return null;
		}

		string json = PlayerPrefs.GetString(CHASE_AUTOSAVE_KEY);
		return JsonUtility.FromJson<GameData>(json);
	}

	public bool HasChaseAutosave() => PlayerPrefs.HasKey(CHASE_AUTOSAVE_KEY);

	public void DeleteChaseAutosave()
	{
		if (!PlayerPrefs.HasKey(CHASE_AUTOSAVE_KEY)) return;

		PlayerPrefs.DeleteKey(CHASE_AUTOSAVE_KEY);
		PlayerPrefs.Save();

		Debug.Log("[SaveSystem] 추격전 자동 저장 데이터 삭제");
	}

	/// <summary>기존 매핑(StageInfo가 없는 씬을 위한 폴백). 코드 수정이 필요하면 여기만 고치면 됩니다.</summary>
	private string GetStageNameByNumber(int stageNumber)
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