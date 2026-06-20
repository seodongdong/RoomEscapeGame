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
}

/// <summary>
/// 슬롯 목록 UI 표시용 요약 정보.
///
/// [추가]
/// - stageName: 스테이지 번호 대신 보여줄 장소 이름 (예: "거실", "지하실")
/// - playTimeSeconds: 저장 시점까지의 누적 플레이 시간(초)
/// </summary>
[System.Serializable]
public struct SaveSlotInfo
{
	public bool hasSave;
	public int currentStage;
	public string stageName;       // ★ 추가
	public float playTimeSeconds;  // ★ 추가
}