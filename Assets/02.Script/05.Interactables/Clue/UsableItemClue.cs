using UnityEngine;

/// <summary>
/// 사용 가능한 아이템 (열쇠, 도구 등)
/// - F키 획득 → 인벤토리 등록
/// - 인벤토리에서 "사용하기" / "보기(3D)"
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

	private void Start()
	{
		_inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();

		if (_inventoryUI == null)
		{
			Debug.LogError("[UsableItemClue] InventoryUI_Complete를 찾을 수 없습니다!");
		}
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

		// 인벤토리에 추가
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

		GameManager.Instance?.ClueTracker.RegisterClue(itemId);

		_hasCollected = true;

		// 오브젝트 비활성화
		gameObject.SetActive(false);

		Debug.Log($"[UsableItemClue] {itemName} 획득!");
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