using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InventoryUI_Complete : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private GameObject inventoryPanel;

	[Header("Tabs")]
	[SerializeField] private Button documentsTab;
	[SerializeField] private Button itemsTab;
	[SerializeField] private GameObject documentsTabHighlight;
	[SerializeField] private GameObject itemsTabHighlight;

	[Header("Left Panel - Item List")]
	[SerializeField] private Transform itemListContent;
	[SerializeField] private GameObject itemButtonPrefab;

	[Header("Right Panel - Detail View")]
	[SerializeField] private GameObject detailPanel;
	[SerializeField] private TextMeshProUGUI detailTitle;
	[SerializeField] private TextMeshProUGUI detailDate;
	[SerializeField] private TextMeshProUGUI detailContent;

	[Header("Action Buttons")]
	[SerializeField] private Button readButton;
	[SerializeField] private Button viewButton;
	[SerializeField] private Button closeButton;
	[SerializeField] private Button useButton;

	[Header("3D Item Viewer")]
	[SerializeField] private ItemViewer3D itemViewer3D;

	private Player _player;
	private DiaryUI _diaryUI;
	private UIManager _uiManager;

	private List<InventoryItemData> _allItems = new List<InventoryItemData>();
	private InventoryItemData _selectedItem;
	private string _currentTab = "Documents";

	public bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;

	/// <summary>★ 추가: 전체 아이템 목록 반환 (SaveSlotUI에서 저장 시 사용)</summary>
	public List<InventoryItemData> GetAllItems() => new List<InventoryItemData>(_allItems);

	private void Awake()
	{
		if (inventoryPanel != null) inventoryPanel.SetActive(false);
		if (detailPanel != null) detailPanel.SetActive(false);

		documentsTab?.onClick.AddListener(() => SwitchTab("Documents"));
		itemsTab?.onClick.AddListener(() => SwitchTab("Items"));
		closeButton?.onClick.AddListener(CloseInventory);
		readButton?.onClick.AddListener(ReadDocument);
		viewButton?.onClick.AddListener(View3DItem);
		useButton?.onClick.AddListener(OnUseButtonClicked);
	}

	private void Start()
	{
		_player = GameServices.Player;
		_diaryUI = FindAnyObjectByType<DiaryUI>(FindObjectsInactive.Include);
		_uiManager = GameServices.UI;
	}

	// ── 열기/닫기 ─────────────────────────────────────────────

	public void OpenInventory()
	{
		inventoryPanel?.SetActive(true);
		GameManager.Instance?.ChangeState(GameState.Puzzle);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		_uiManager?.HideInteractionPrompt();
		UILayerManager.Instance?.Push(this, CloseInventory);
		RefreshInventory();
		Debug.Log("[InventoryUI] 열림");
	}

	public void CloseInventory()
	{
		inventoryPanel?.SetActive(false);
		if (detailPanel != null) detailPanel.SetActive(false);
		GameManager.Instance?.ChangeState(GameState.Playing);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		UILayerManager.Instance?.Pop(this);
		Debug.Log("[InventoryUI] 닫힘");
	}

	// ── 탭 전환 ───────────────────────────────────────────────

	private void SwitchTab(string tab)
	{
		_currentTab = tab;
		if (documentsTabHighlight != null) documentsTabHighlight.SetActive(tab == "Documents");
		if (itemsTabHighlight != null) itemsTabHighlight.SetActive(tab == "Items");
		RefreshInventory();
		if (detailPanel != null) detailPanel.SetActive(false);
	}

	// ── 아이템 등록/제거 ──────────────────────────────────────

	public void AddItem(InventoryItemData item)
	{
		// ★ 중복 등록 방지
		if (_allItems.Any(i => i.itemId == item.itemId)) return;

		_allItems.Add(item);
		_allItems = _allItems.OrderBy(i => i.date).ToList();
		Debug.Log($"[InventoryUI] 추가: {item.title} ({item.itemType})");
	}

	public void RemoveItem(string itemId)
	{
		int removed = _allItems.RemoveAll(i => i.itemId == itemId);
		if (removed == 0) { Debug.LogWarning($"[InventoryUI] 제거할 아이템 없음: {itemId}"); return; }
		if (_selectedItem != null && _selectedItem.itemId == itemId)
		{
			_selectedItem = null;
			if (detailPanel != null) detailPanel.SetActive(false);
		}
		RefreshInventory();
	}

	// ── 목록 갱신 ─────────────────────────────────────────────

	public void RefreshInventory()
	{
		foreach (Transform child in itemListContent) Destroy(child.gameObject);

		var filtered = _allItems.Where(i =>
			(_currentTab == "Documents" && i.itemType == ItemType.Document) ||
			(_currentTab == "Items" && i.itemType == ItemType.UsableItem)
		).ToList();

		foreach (var item in filtered)
		{
			GameObject btn = Instantiate(itemButtonPrefab, itemListContent);
			var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
			if (tmp != null) tmp.text = $"{item.date} - {item.title}";
			var b = btn.GetComponent<Button>();
			if (b != null) { var ic = item; b.onClick.AddListener(() => SelectItem(ic)); }
		}
	}

	private void SelectItem(InventoryItemData item)
	{
		_selectedItem = item;
		if (detailPanel != null) detailPanel.SetActive(true);
		if (detailTitle != null) detailTitle.text = item.title;
		if (detailDate != null) detailDate.text = item.date;
		if (detailContent != null)
			detailContent.text = (item.pages != null && item.pages.Count > 0)
				? string.Join("\n\n─────\n\n", item.pages)
				: item.description;
		UpdateActionButtons(item.itemType);
	}

	private void UpdateActionButtons(ItemType itemType)
	{
		if (itemType == ItemType.Document)
		{
			readButton?.gameObject.SetActive(true);
			viewButton?.gameObject.SetActive(false);
			useButton?.gameObject.SetActive(false);
		}
		else
		{
			readButton?.gameObject.SetActive(false);
			viewButton?.gameObject.SetActive(true);
			useButton?.gameObject.SetActive(true);
		}
	}

	// ── 버튼 동작 ─────────────────────────────────────────────

	private void ReadDocument()
	{
		if (_selectedItem == null || _selectedItem.pages == null || _selectedItem.pages.Count == 0)
		{
			Debug.LogWarning("[InventoryUI] 읽을 페이지 없음");
			return;
		}
		if (_diaryUI == null) { Debug.LogError("[InventoryUI] DiaryUI 없음"); return; }

		inventoryPanel?.SetActive(false);
		if (detailPanel != null) detailPanel.SetActive(false);

		UILayerManager.Instance?.Push(_diaryUI, () =>
		{
			_diaryUI.CloseDiary();
			inventoryPanel?.SetActive(true);
			RefreshInventory();
		});
		_diaryUI.OpenDiary(_selectedItem.pages, returnToInventory: true);
	}

	private void View3DItem()
	{
		if (_selectedItem == null || itemViewer3D == null) return;
		inventoryPanel?.SetActive(false);
		if (detailPanel != null) detailPanel.SetActive(false);
		itemViewer3D.OpenViewer(_selectedItem.itemPrefab, _selectedItem.title, this);
	}

	private void OnUseButtonClicked()
	{
		if (_selectedItem == null) return;
		var usableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IItemUsable>();
		foreach (var obj in usableObjects)
		{
			if (obj.CanUseItem(_selectedItem.itemId))
			{
				string usedId = _selectedItem.itemId;
				obj.UseItem(usedId);
				RemoveItem(usedId);
				var player = GameServices.Player;
				var itm = player?.Inventory.GetItem(usedId);
				if (itm != null) player.Inventory.RemoveItem(itm);
				CloseInventory();
				return;
			}
		}
		_uiManager?.ShowDialogue("", "여기에 사용할 수 없다.");
	}
}

public enum ItemType { Document, UsableItem }

[System.Serializable]
public class InventoryItemData
{
	public string itemId;
	public string title;
	public string date;
	public ItemType itemType;
	public string description;
	public List<string> pages;
	public GameObject itemPrefab;
}

public interface IItemUsable
{
	void UseItem(string itemId);
	bool CanUseItem(string itemId);
}