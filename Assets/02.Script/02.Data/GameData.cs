using UnityEngine;
using System.Collections.Generic;

// 게임 데이터 저장을 위한 클래스

[System.Serializable]
public class GameData
{
    public int currentStage;
    public Vector3 playerPosition;
    public List<string> collectedClues;     // 수집한 단서 목록
    public int health;
    public bool[] solvedPuzzles;            // 퍼즐 해결 상태 배열

    // 생성자
    public GameData()
    {
        collectedClues = new List<string>();
        solvedPuzzles = new bool[5];        // 예시로 5개의 퍼즐 상태 저장
    }
}
