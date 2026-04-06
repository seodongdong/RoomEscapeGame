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
	Chase,
	Dialogue,
	Viewer,     // 🆕 문서 뷰어 열람 중
	GameOver,
	Ending
}

public enum EndingType
{
	GameOver,
	Normal,
	True
}

// 🆕 단서 타입 구분
public enum ClueType
{
	Document,   // 문서류 (일기, 편지, 신문) → 맵에 남음
	Physical,   // 물리 아이템 (열쇠, 도구) → 인벤토리
	Environment // 환경 단서 (숫자, 날짜) → 맵에 남음
}