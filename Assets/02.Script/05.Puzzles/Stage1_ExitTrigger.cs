using UnityEngine;

/// <summary>
/// 1스테이지: 출구 통과 트리거
///
/// [기존 Stage2_ExitTrigger와 동일한 패턴]
/// 퍼즐(인형의 집)이 해결된 상태에서 플레이어가 이 트리거를 지나가면
/// 2스테이지로 씬을 전환합니다. 문을 여는 동작(PuzzleSolveDoor)과
/// 씬을 넘기는 동작(이 스크립트)을 분리해서, "문은 열렸는데 씬은 안 넘어감"
/// 같은 상태를 명확하게 구분할 수 있게 했습니다.
///
/// [씬 설정]
/// 1. 출구 문 바로 안쪽(복도 방향)에 빈 GameObject 배치
/// 2. BoxCollider 추가, Is Trigger 체크
/// 3. 이 스크립트 부착
/// 4. puzzleObject 슬롯에 DollHousePuzzle(Stage1_DollHousePuzzle) 연결
/// 5. Player 프리팹의 Tag가 "Player"로 되어 있는지 확인
/// </summary>
public class Stage1_ExitTrigger : MonoBehaviour
{
	[Header("퍼즐 연결 (완료 여부 확인용)")]
	[Tooltip("IPuzzle을 구현한 컴포넌트. Stage1_DollHousePuzzle을 연결하세요.")]
	[SerializeField] private MonoBehaviour puzzleObject;

	[Header("다음 스테이지")]
	[Tooltip("StageManager.LoadStage()에 넘길 스테이지 번호. 1스테이지 다음이므로 2.")]
	[SerializeField] private int nextStageNumber = 2;

	[Header("대사 (선택)")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string exitDialogue = "";

	private IPuzzle _puzzle;
	private bool _hasTriggered = false;

	private void Awake()
	{
		_puzzle = puzzleObject as IPuzzle;

		if (_puzzle == null && puzzleObject != null)
			Debug.LogError($"[Stage1ExitTrigger] {puzzleObject.name}은 IPuzzle을 구현하지 않습니다!");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_hasTriggered) return;
		if (!other.CompareTag("Player")) return;

		bool puzzleSolved = _puzzle == null || _puzzle.IsSolved;
		if (!puzzleSolved) return;

		_hasTriggered = true;

		if (!string.IsNullOrEmpty(exitDialogue))
			GameServices.UI?.ShowDialogue(speaker, exitDialogue);

		Debug.Log($"[Stage1ExitTrigger] 출구 통과 → 스테이지 {nextStageNumber}로 전환");

		if (GameManager.Instance != null)
			GameManager.Instance.StageManager.LoadStage(nextStageNumber);
		else
			Debug.LogError("[Stage1ExitTrigger] GameManager.Instance가 없어 씬 전환을 할 수 없습니다.");
	}
}