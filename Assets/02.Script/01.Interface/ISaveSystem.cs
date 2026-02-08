using UnityEngine;

/// <summary>
/// 저장 시스템 인터페이스
/// 게임 진행 상황 저장/로드
/// </summary>
public interface ISaveSystem
{
	void SaveGame(GameData data);
	GameData LoadGame();
	bool HasSaveFile();
}