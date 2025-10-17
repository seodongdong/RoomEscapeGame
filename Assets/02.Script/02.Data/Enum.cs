using UnityEngine;

// 게임 진행 상태, 엔딩 타입 등을 정의하는 열거형

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Puzzle,
    Chase,
    GameOver,
    Ending
}

public enum EndingType
{
    GameOver,
    Normal,
    True
}
