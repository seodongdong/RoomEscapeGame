using UnityEngine;

/// <summary>
/// 제단 향로 — 퍼즐 나가기 오브젝트
///
/// [두 가지 모드]
/// 1. 퍼즐 밖 (일반 탐색 중): F키 → 단서 대사 출력 (향 찌꺼기 단서)
/// 2. 퍼즐 안 (관 상호작용 후 카메라 이동 상태): 마우스 클릭 → 퍼즐 종료
///    - 진행 상황은 저장됨. 다시 관에 상호작용하면 이어서 풀 수 있음.
///
/// [퍼즐 안에서 클릭이 되는 이유]
/// CameraPuzzleBase가 퍼즐 진입 시 Cursor를 잠금 해제(CursorLockMode.None)하기 때문에
/// 마우스 클릭이 가능합니다. Player 컴포넌트는 비활성화되어 F키는 안 되지만
/// OnMouseDown()은 Collider만 있으면 항상 작동합니다.
///
/// [씬 설정]
/// - 향로 오브젝트에 이 스크립트 + Collider 부착 (IsTrigger: false)
/// - Puzzle 슬롯에 AltarCandyPuzzle 오브젝트 연결
/// - 관 카메라 시점에서 향로가 보이는 위치에 배치 (관 근처 제단 위)
/// </summary>
public class AltarIncense : MonoBehaviour, IInteractable
{
	[Header("연결")]
	[Tooltip("씬의 AltarCandyPuzzle 오브젝트 연결")]
	[SerializeField] private Stage2_AltarCandyPuzzle puzzle;

	[Header("단서 정보 (퍼즐 밖에서 F키 상호작용)")]
	[SerializeField] private string clueId = "stage2_incense";
	[SerializeField] private string clueName = "향 찌꺼기";
	[SerializeField] private string speaker = "소년";

	[TextArea(2, 4)]
	[SerializeField] private string clueDialogue = "향이 완전히 다 탔다... 얼마나 오래 피운 걸까.";

	[Header("퍼즐 안에서 클릭 시 대사")]
	[TextArea(2, 4)]
	[SerializeField] private string exitDialogue = "일단 나가야겠다. 기억해뒀다가 나중에 다시 풀자.";

	// ── IInteractable (F키, 퍼즐 밖에서만 작동) ──────────────

	public string InteractionPrompt => $"[F] {clueName} 조사하기";

	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		// 퍼즐 모드 중에는 Player가 비활성화되어 이 메서드가 호출되지 않습니다.
		// 퍼즐 밖에서 F키를 누르면 단서 대사만 출력합니다.
		var clue = new ClueItem(clueId, clueName, clueDialogue);
		player.Inventory.AddItem(clue);
		GameManager.Instance?.ClueTracker.RegisterClue(clueId);

		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, clueDialogue);
		gameObject.SetActive(false); // 획득 후 사라짐
	}

	// ── 마우스 클릭 (퍼즐 안에서 작동) ──────────────────────

	private void OnMouseDown()
	{
		// 퍼즐 모드(GameState.Puzzle)일 때만 반응
		if (GameManager.Instance == null) return;
		if (GameManager.Instance.StateManager.CurrentState != GameState.Puzzle) return;
		if (puzzle == null) return;

		// 대사 출력 후 퍼즐 종료 (진행값 보존)
		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, exitDialogue);
		puzzle.ExitPuzzlePreserveState();
	}

	private void OnMouseEnter()
	{
		// 퍼즐 모드 중 마우스 오버 시 커서 변경 (선택 사항)
		if (GameManager.Instance?.StateManager.CurrentState == GameState.Puzzle)
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // 기본 커서
	}
}