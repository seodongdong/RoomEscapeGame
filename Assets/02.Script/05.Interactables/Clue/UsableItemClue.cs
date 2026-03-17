using UnityEngine;

/// <summary>
/// 사용 가능한 아이템 (열쇠, 도구 등)
/// - F키 획득 → InventoryUI_Complete(UI) + Player.Inventory(PlayerInventory) 동시 등록
/// - 인벤토리에서 "사용하기" / "보기(3D)"
/// - ⭐ OnTriggerEnter/Exit 제거 → Player.cs Raycast 방식으로 통일
/// </summary>
public class UsableItemClue : MonoBehaviour, IInteractable
{
	[Header("Item Info")]
	[SerializeField] private string itemId = "key_bedroom";
	[SerializeField] private string itemName = "침실 열쇠";

	[Header("Inventory Data")]
	[SerializeField] private string itemDate = "2023.07.16";
	[TextArea(3, 5)]
	[SerializeField] private string description = "낡은 침실 열쇠. 녹슬어 있지만 아직 사용할 수 있을 것 같다.";
	[SerializeField] private GameObject itemPrefab;

	[Header("First Interaction Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string firstDialogue = "열쇠를 발견했다. 어디에 쓰는 열쇠일까?";

	private bool _hasCollected = false;
	private InventoryUI_Complete _inventoryUI;
	private Player _playerRef;

	private void Start()
	{
		_inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();
		_playerRef = FindAnyObjectByType<Player>();

		if (_inventoryUI == null)
			Debug.LogError("[UsableItemClue] InventoryUI_Complete를 찾을 수 없습니다!");
		if (_playerRef == null)
			Debug.LogError("[UsableItemClue] Player를 찾을 수 없습니다!");
	}

	// ── IInteractable ──────────────────────────────
	public string InteractionPrompt => _hasCollected ? "" : $"[F] {itemName} 획득";

	public bool CanInteract(IPlayer player)
	{
		return !_hasCollected;
	}

	public void Interact(IPlayer player)
	{
		// 1스테이지 TV 우선순위 체크
		if (Stage1TVPriorityManager.CheckPriorityBlocked(player)) return;

		if (_hasCollected) return;

		_hasCollected = true;

		// 대사 출력
		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(firstDialogue))
			uiManager?.ShowDialogue(speaker, firstDialogue);

		// InventoryUI_Complete (UI 표시용) 등록
		_inventoryUI?.AddItem(new InventoryItemData
		{
			itemId = itemId,
			title = itemName,
			date = itemDate,
			itemType = ItemType.UsableItem,
			description = description,
			pages = null,
			itemPrefab = itemPrefab
		});

		// PlayerInventory (퍼즐 HasItem 체크용) 등록
		player?.Inventory.AddItem(new ClueItem(itemId, itemName, description));

		// 단서 추적 등록
		GameManager.Instance?.ClueTracker.RegisterClue(itemId);

		// 오브젝트 비활성화
		gameObject.SetActive(false);

		Debug.Log($"[UsableItemClue] {itemName} 획득 완료");
	}
}