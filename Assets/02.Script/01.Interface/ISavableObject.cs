/// <summary>
/// 씬 내 오브젝트 상태를 저장/복원하는 인터페이스.
///
/// [구현 대상]
/// - Door.cs (잠금 상태, 열림 여부)
/// - PuzzleSolveDoor.cs (퍼즐 해결 후 열림 여부)
/// - CameraPuzzleBase 파생 클래스들 (퍼즐 완료 여부)
///
/// [사용법]
/// SaveId: 씬 내에서 유일한 문자열 ID (Inspector에서 설정)
/// SaveState(): 현재 상태를 JSON 문자열로 반환
/// LoadState(json): JSON 문자열에서 상태 복원
///
/// SaveSystem이 씬의 모든 ISaveableObject를 찾아서
/// SaveGame 시 상태를 수집하고, SaveLoader가 복원합니다.
/// </summary>
public interface ISaveableObject
{
	string SaveId { get; }
	string SaveState();
	void LoadState(string json);
}