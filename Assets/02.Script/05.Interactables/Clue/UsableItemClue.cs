using UnityEngine;

/// <summary>
/// 사용 가능한 아이템 (열쇠, 도구 등)
/// - F키 획득 → InventoryUI_Complete(UI) + Player.Inventory(PlayerInventory) 동시 등록
/// - 인벤토리에서 "사용하기" / "보기(3D)"
/// 
/// [버그 수정] 기존 코드는 InventoryUI_Complete._allItems에만 추가하고
///            Player.Inventory(PlayerInventory)에 추가하지 않아서
///            Stage1_DollHousePuzzle.TryPlaceItemToSlot()의 HasItem() 체크가 항상 false였음
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
	[SerializeField] private GameObject itemPrefab;  // 3D 뷰어용 프리팹

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

	public string InteractionPrompt => $"[F] {itemName} 획득";

	public bool CanInteract(IPlayer player)
	{
		return !_hasCollected;
	}

	public void Interact(IPlayer player)
	{
		if (_hasCollected) return;

		// 대사 출력
		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(firstDialogue))
		{
			uiManager?.ShowDialogue(speaker, firstDialogue);
		}

		// ✅ [수정] 1) InventoryUI_Complete (UI 표시용) 에 추가
		if (_inventoryUI != null)
		{
			InventoryItemData itemData = new InventoryItemData
			{
				itemId = itemId,
				title = itemName,
				date = itemDate,
				itemType = ItemType.UsableItem,
				description = description,
				pages = null,
				itemPrefab = itemPrefab
			};

			_inventoryUI.AddItem(itemData);
		}

		// ✅ [수정] 2) Player.Inventory (PlayerInventory - 퍼즐 HasItem 체크용) 에도 추가
		//    기존 코드에서 이 부분이 빠져있어서 DollHousePuzzle의 HasItem()이 false 반환했음
		if (player != null)
		{
			ClueItem clue = new ClueItem(itemId, itemName, description);
			player.Inventory.AddItem(clue);
			Debug.Log($"[UsableItemClue] PlayerInventory에 추가됨: {itemName} (id={itemId})");
		}

		// 단서 추적 등록
		GameManager.Instance?.ClueTracker.RegisterClue(itemId);

		_hasCollected = true;
		gameObject.SetActive(false);

		Debug.Log($"[UsableItemClue] {itemName} 획득! (InventoryUI + PlayerInventory 모두 등록)");
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<Player>(out var player))
		{
			player.SetCurrentInteractable(this);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent<Player>(out var player))
		{
			player.SetCurrentInteractable(null);
		}
	}
}