using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 일기장 단서
/// - F키 상호작용 → PlayerInventory + InventoryUI_Complete 동시 등록 → DiaryUI 열기
/// - 인벤토리에서 다시 읽기 가능
///
/// [수정]
/// - PlayerInventory.AddItem() 추가 (Door.HasItem() 등 체크용)
/// - DiaryUI 열릴 때 UILayerManager.Push → ESC로 닫기 가능
/// </summary>
public class DiaryClue : MonoBehaviour, IInteractable
{
	[Header("Clue Info")]
	[SerializeField] private string clueId = "diary_page_1";
	[SerializeField] private string clueName = "찢어진 일기장";

	[Header("Diary Pages")]
	[TextArea(5, 10)]
	[SerializeField]
	private List<string> pages = new List<string>
	{
		"1988년 7월 15일\n\n오늘은 친구가 우리집에 놀러왔다.\n맛있는 음식도 같이 먹고,\n인형놀이도 했다.\n\n정말 재미있었다!",
		"1988년 7월 20일\n\n오늘도 친구랑 놀았다.\n엄마가 만들어준 과자가\n정말 맛있었어.\n\n내일도 같이 놀기로 했다.",
		"1988년 7월 25일\n\n...\n\n(페이지가 찢어져있다)"
	};

	[Header("Inventory Data")]
	[SerializeField] private string itemDate = "1988.07.15";
	[TextArea(3, 5)]
	[SerializeField] private string summary = "어린 여자아이가 쓴 일기장. 친구와 함께 놀았다는 내용이 적혀있다.";

	[Header("First Interaction Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string firstDialogue = "찢어진 일기장이다... 누가 그린 걸까?";

	[Header("Settings")]
	[SerializeField] private bool collectOnRead = true;

	// ── 런타임 ───────────────────────────────────────────────
	private bool _hasRead = false;
	private DiaryUI _diaryUI;
	private InventoryUI_Complete _inventoryUI;

	private void Start()
	{
		// 비활성 오브젝트도 탐색 (DiaryUI가 꺼져있을 수 있음)
		_diaryUI = FindAnyObjectByType<DiaryUI>(FindObjectsInactive.Include);
		_inventoryUI = FindAnyObjectByType<InventoryUI_Complete>(FindObjectsInactive.Include);

		if (_diaryUI == null)
			Debug.LogError("[DiaryClue] DiaryUI를 찾을 수 없습니다!");
		if (_inventoryUI == null)
			Debug.LogError("[DiaryClue] InventoryUI_Complete를 찾을 수 없습니다!");
	}

	// ── IInteractable ─────────────────────────────────────────

	public string InteractionPrompt =>
		_hasRead ? $"[F] {clueName} 다시 보기" : $"[F] {clueName} 조사하기";

	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		// Stage1 TV 우선순위 체크
		if (Stage1TVPriorityManager.CheckPriorityBlocked(player)) return;

		// 첫 상호작용: 대사 + 인벤토리 등록
		if (!_hasRead && collectOnRead)
		{
			_hasRead = true;

			// 대사 출력
			var uiManager = FindAnyObjectByType<UIManager>();
			if (!string.IsNullOrEmpty(firstDialogue))
				uiManager?.ShowDialogue(speaker, firstDialogue);

			// ★ PlayerInventory 등록 (HasItem 체크용)
			player.Inventory.AddItem(new ClueItem(clueId, clueName, summary));

			// ★ InventoryUI_Complete 등록 (UI 표시용)
			_inventoryUI?.AddItem(new InventoryItemData
			{
				itemId = clueId,
				title = clueName,
				date = itemDate,
				itemType = ItemType.Document,
				description = summary,
				pages = new List<string>(pages)
			});

			// 단서 추적 등록
			GameManager.Instance?.ClueTracker.RegisterClue(clueId);
		}

		// 일기장 UI 열기 (첫 조사 / 다시 보기 모두)
		if (_diaryUI != null)
		{
			// UILayerManager에 등록 → ESC로 닫기 가능
			UILayerManager.Instance?.Push(_diaryUI, _diaryUI.CloseDiary);
			_diaryUI.OpenDiary(pages);
		}
	}

	// ── Trigger (보조 — Raycast와 병행) ──────────────────────

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<Player>(out var player))
			player.SetCurrentInteractable(this);
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent<Player>(out var player))
			player.SetCurrentInteractable(null);
	}
}