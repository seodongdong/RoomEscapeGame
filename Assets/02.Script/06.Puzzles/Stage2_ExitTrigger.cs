using UnityEngine;

/// <summary>
/// 2스테이지: 퇴장 트리거
///
/// [문 세 개 구조]
/// 입구 (Door.cs)          : 열쇠로 열고 영구 잠금. 이 트리거와 무관.
/// 퍼즐 문 (PuzzleSolveDoor): 퍼즐 완료 시 자동으로 열림. 이 트리거와 무관.
/// 출구 (PuzzleSolveDoor)  : 퍼즐 완료 시 잠금 해제, 플레이어가 직접 열어야 함.
///                           이 트리거 통과 시 자유 출입으로 전환.
///
/// [흐름]
/// 퍼즐 완료 → 출구 잠금 해제 → 플레이어가 F키로 출구 열기
/// → 통과 시 첫 퇴장: 크리처 웃음 + 대사 + 자유 출입 전환
/// → 이후: 자유 출입
/// </summary>
public class Stage2_ExitTrigger : MonoBehaviour
{
	[Header("퍼즐 연결 (완료 여부 확인용)")]
	[SerializeField] private MonoBehaviour puzzleObject;

	[Header("크리처")]
	[SerializeField] private Stage2_ShadowCreature shadowCreature;

	[Header("출구 문 연결")]
	[Tooltip("출구에 붙은 PuzzleSolveDoor. 첫 퇴장 후 자유 출입으로 전환.")]
	[SerializeField] private PuzzleSolveDoor exitDoor;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string exitDialogue = "...뒤에서 시선이 느껴진다.";

	private IPuzzle _puzzle;
	private bool _hasExitedOnce = false;

	private void Awake()
	{
		_puzzle = puzzleObject as IPuzzle;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player")) return;

		bool puzzleSolved = _puzzle == null || _puzzle.IsSolved;
		if (!puzzleSolved) return;

		if (!_hasExitedOnce)
		{
			_hasExitedOnce = true;
			shadowCreature?.TriggerExitSmile();
			FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, exitDialogue);
			exitDoor?.UnlockFreeAccess();
			Debug.Log("[ExitTrigger] 첫 퇴장 연출");
		}
	}
}