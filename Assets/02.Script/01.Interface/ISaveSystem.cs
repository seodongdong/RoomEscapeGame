using UnityEngine;

// 세이브/로드 시스템 인터페이스

public interface ISaveSystem
{
    void SaveGame(GameData data);
    GameData LoadGame();
    bool HasSaveFile();
}
