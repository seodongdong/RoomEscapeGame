using UnityEngine;

public interface ISaveSystem
{
	void SaveGame(GameData data);
	GameData LoadGame();
	bool HasSaveFile();

	void SaveGame(int slotIndex, GameData data);
	GameData LoadGame(int slotIndex);
	bool HasSaveFile(int slotIndex);
	void DeleteSave(int slotIndex);

	SaveSlotInfo GetSlotInfo(int slotIndex);

	// ★ 추가: 5스테이지 추격전 자동 저장 (일반 슬롯과 분리)
	void SaveChaseAutosave(GameData data);
	GameData LoadChaseAutosave();
	bool HasChaseAutosave();
	void DeleteChaseAutosave();
}

[System.Serializable]
public struct SaveSlotInfo
{
	public bool hasSave;
	public int currentStage;
	public string stageName;
	public float playTimeSeconds;
}