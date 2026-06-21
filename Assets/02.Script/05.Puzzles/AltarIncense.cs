using UnityEngine;

/// <summary>
/// 제단 향로 — 단서 + 3D 뷰어 + 퍼즐 나가기
///
/// [세 가지 모드]
/// 1. 퍼즐 밖: F키 → 단서 등록 + ObjectViewer3D로 3D 감상
/// 2. 퍼즐 안: 마우스 클릭 → 퍼즐 종료 (진행값 보존)
///
/// [씬 설정]
/// - 향로 오브젝트에 이 스크립트 + ObjectViewer3D + Collider(IsTrigger: false) 부착
/// - ObjectViewer3D는 IInteractable을 구현하지만 Player가 직접 호출하지 않음
///   (AltarIncense가 IInteractable을 가로채서 대신 호출)
/// - Puzzle 슬롯에 AltarCandyPuzzle 연결
/// </summary>
public class AltarIncense : MonoBehaviour, IInteractable
{
	[Header("연결")]
	[SerializeField] private Stage2_AltarCandyPuzzle puzzle;

	[Header("단서 정보")]
	[SerializeField] private string clueId = "stage2_incense";
	[SerializeField] private string clueName = "향 찌꺼기";
	[SerializeField] private string speaker = "소년";

	[TextArea(2, 4)]
	[SerializeField] private string clueDialogue = "향이 완전히 다 탔다... 얼마나 오래 피운 걸까.";

	[Header("퍼즐 안에서 클릭 시 대사")]
	[TextArea(2, 4)]
	[SerializeField] private string exitDialogue = "일단 나가야겠다. 기억해뒀다가 나중에 다시 풀자.";

	// ── 컴포넌트 참조 ─────────────────────────────────────────
	private ObjectViewer3D _viewer;
	private bool _clueRegistered = false;

	private void Awake()
	{
		// 같은 오브젝트의 ObjectViewer3D를 가져옴
		_viewer = GetComponent<ObjectViewer3D>();

		if (_viewer == null)
			Debug.LogWarning("[AltarIncense] ObjectViewer3D 컴포넌트가 없습니다. 3D 뷰어 없이 대사만 출력됩니다.");
	}

	// ── IInteractable ─────────────────────────────────────────

	public string InteractionPrompt => $"[F] {clueName} 조사하기";

	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		// 단서 최초 1회 등록 (인벤토리 + ClueTracker + InventoryUI)
		if (!_clueRegistered)
		{
			_clueRegistered = true;

			var clueItem = new ClueItem(clueId, clueName, clueDialogue);
			player.Inventory.AddItem(clueItem);
			GameManager.Instance?.ClueTracker.RegisterClue(clueId);

			// InventoryUI에도 등록
			var inventoryData = new InventoryItemData
			{
				itemId = clueId,
				title = clueName,
				description = clueDialogue,
				itemType = ItemType.UsableItem
			};
			FindAnyObjectByType<InventoryUI_Complete>()?.AddItem(inventoryData);
		}

		// ObjectViewer3D가 있으면 3D 뷰어 열기
		// 없으면 대사만 출력
		if (_viewer != null)
			_viewer.Interact(player);
		else
			GameServices.UI?.ShowDialogue(speaker, clueDialogue);

		// 향로는 사라지지 않음 — 이후에도 다시 3D로 볼 수 있음
	}

	// ── 마우스 클릭 (퍼즐 안에서 작동) ──────────────────────

	private void OnMouseDown()
	{
		if (GameManager.Instance == null) return;
		if (GameManager.Instance.CurrentState != GameState.Puzzle) return;
		if (puzzle == null) return;

		GameServices.UI?.ShowDialogue(speaker, exitDialogue);
		puzzle.ExitPuzzlePreserveState();
	}

	private void OnMouseEnter()
	{
		if (GameManager.Instance?.CurrentState == GameState.Puzzle)
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}
}