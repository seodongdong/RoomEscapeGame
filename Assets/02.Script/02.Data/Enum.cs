using UnityEngine;

/// <summary>
/// 게임 상태
/// </summary>
public enum GameState
{
	MainMenu,
	Playing,
	Paused,
	Puzzle,
	Chase,      // 5스테이지 추격전
	Dialogue,
	GameOver,
	Ending
}

/// <summary>
/// 엔딩 타입
/// 기획서: 게임오버 / 노말(캠코더X) / 진엔딩(캠코더O)
/// </summary>
public enum EndingType
{
	GameOver,   // 범인에게 잡힘 OR 시간 초과
	Normal,     // 소녀 구출 성공 BUT 캠코더 미수집
	True        // 소녀 구출 성공 + 캠코더 수집
}