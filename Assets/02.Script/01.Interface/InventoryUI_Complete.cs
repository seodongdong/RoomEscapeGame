using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 완전한 인벤토리 시스템
/// - 탭: Documents / Items
/// - 문서: 읽기 버튼 → DiaryUI (닫으면 인벤토리로 복귀)
/// - 아이템: 보기(3D 뷰어) → ItemViewer3D 사용
/// - 열릴 때 상호작용 프롬프트 숨김 / 닫힐 때 복원
///
/// [버그 수정]
/// 1. 읽기 버튼 → 다이어리 닫으면 인벤토리로 돌아오도록 수정
/// 2. 3D 보기 → ObjectViewer3D 방식 참고한 ItemViewer3D 사용
/// 3. 인벤토리 열릴 때 InteractionPrompt 숨김 / 닫힐 때 복원
/// </summary>
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
	[SerializeField] private Button readButton;     // 문서: 읽기
	[SerializeField] private Button viewButton;     // 아이템: 3D 보기
	[SerializeField] private Button closeButton;    // 인벤토리 닫기
	[SerializeField] private Button useButton;     // 아이템 사용

	[Header("3D Item Viewer")]
	[SerializeField] private ItemViewer3D itemViewer3D; // 별도 컴포넌트로 분리

	private Player _player;
	private DiaryUI _diaryUI;
	private UIManager _uiManager;

	private List<InventoryItemData> _allItems = new List<InventoryItemData>();
	private InventoryItemData _selectedItem;
	private string _currentTab = "Documents";

	private void Awake()
	{
		if (inventoryPanel != null)
			inventoryPanel.SetActive(false);

		if (detailPanel != null)
			detailPanel.SetActive(false);

		documentsTab?.onClick.AddListener(() => SwitchTab("Documents"));
		itemsTab?.onClick.AddListener(() => SwitchTab("Items"));

		closeButton?.onClick.AddListener(CloseInventory);
		readButton?.onClick.AddListener(ReadDocument);
		viewButton?.onClick.AddListener(View3DItem);
		useButton?.onClick.AddListener(() =>
		{
			if (_selectedItem != null)
			{
				var usableObjects = FindObjectsOfType<MonoBehaviour>().OfType<IItemUsable>();
				foreach (var obj in usableObjects)
				{
					if (obj.CanUseItem(_selectedItem.itemId))
					{
						obj.UseItem(_selectedItem.itemId);
						CloseInventory();
						return;
					}
				}
				Debug.LogWarning($"[InventoryUI] {_selectedItem.title}을(를) 사용할 수 있는 대상이 없습니다.");
			}
		});
	}

	private void Start()
	{
		_player = FindAnyObjectByType<Player>();
		_diaryUI = FindAnyObjectByType<DiaryUI>();
		_uiManager = FindAnyObjectByType<UIManager>();
	}

	// ─────────────────────────────────────────────
	// 열기 / 닫기
	// ─────────────────────────────────────────────

	public void OpenInventory()
	{
		inventoryPanel?.SetActive(true);

		if (_player != null)
			_player.enabled = false;

		GameManager.Instance?.StateManager.ChangeState(GameState.Puzzle);

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		// 상호작용 프롬프트 숨기기
		_uiManager?.HideInteractionPrompt();

		RefreshInventory();

		Debug.Log("[InventoryUI] 인벤토리 열림");
	}

	public void CloseInventory()
	{
		inventoryPanel?.SetActive(false);

		if (detailPanel != null)
			detailPanel.SetActive(false);

		if (_player != null)
			_player.enabled = true;

		GameManager.Instance?.StateManager.ChangeState(GameState.Playing);

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		// 상호작용 프롬프트 복원은 Player의 Raycast가 자연스럽게 처리함
		// (플레이어가 활성화되면 매 프레임 Raycast → 필요시 ShowInteractionPrompt)

		Debug.Log("[InventoryUI] 인벤토리 닫힘");
	}

	// ─────────────────────────────────────────────
	// 탭 / 아이템 관리
	// ─────────────────────────────────────────────

	private void SwitchTab(string tab)
	{
		_currentTab = tab;

		if (documentsTabHighlight != null)
			documentsTabHighlight.SetActive(tab == "Documents");

		if (itemsTabHighlight != null)
			itemsTabHighlight.SetActive(tab == "Items");

		RefreshInventory();

		if (detailPanel != null)
			detailPanel.SetActive(false);
	}

	public void AddItem(InventoryItemData item)
	{
		_allItems.Add(item);
		_allItems = _allItems.OrderBy(i => i.date).ToList();

		Debug.Log($"[InventoryUI] 아이템 추가: {item.title} ({item.itemType})");
	}

	public void RefreshInventory()
	{
		foreach (Transform child in itemListContent)
			Destroy(child.gameObject);

		var filteredItems = _allItems.Where(i =>
			(_currentTab == "Documents" && i.itemType == ItemType.Document) ||
			(_currentTab == "Items" && i.itemType == ItemType.UsableItem)
		).ToList();

		foreach (var item in filteredItems)
		{
			GameObject btn = Instantiate(itemButtonPrefab, itemListContent);

			var tmpText = btn.GetComponentInChildren<TextMeshProUGUI>();
			if (tmpText != null)
				tmpText.text = $"{item.date} - {item.title}";

			var button = btn.GetComponent<Button>();
			if (button != null)
			{
				var itemCopy = item;
				button.onClick.AddListener(() => SelectItem(itemCopy));
			}
		}
	}

	private void SelectItem(InventoryItemData item)
	{
		_selectedItem = item;

		if (detailPanel != null)
			detailPanel.SetActive(true);

		if (detailTitle != null)
			detailTitle.text = item.title;

		if (detailDate != null)
			detailDate.text = item.date;

		if (detailContent != null)
		{
			if (item.pages != null && item.pages.Count > 0)
				detailContent.text = string.Join("\n\n───────────\n\n", item.pages);
			else
				detailContent.text = item.description;
		}

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
		else if (itemType == ItemType.UsableItem)
		{
			readButton?.gameObject.SetActive(false);

			// ✅ 기존: itemPrefab 있을 때만 보기 버튼 활성화
			// ✅ 변경: UsableItem이면 무조건 보기 버튼 활성화
			//         (itemPrefab null이면 View3DItem 안에서 경고만 뜸)
			viewButton?.gameObject.SetActive(true);
			useButton?.gameObject.SetActive(true);
		}
	}

	// ─────────────────────────────────────────────
	// 액션: 읽기 / 3D 보기
	// ─────────────────────────────────────────────

	/// <summary>
	/// 문서 읽기
	/// [버그 수정] CloseInventory 후 OpenDiary(returnToInventory: true)
	/// → 다이어리 닫기 버튼 누르면 인벤토리가 다시 열림
	/// </summary>
	private void ReadDocument()
	{
		if (_selectedItem == null || _selectedItem.pages == null || _selectedItem.pages.Count == 0)
		{
			Debug.LogWarning("[InventoryUI] 읽을 페이지가 없습니다.");
			return;
		}

		// 인벤토리 패널 숨기기 (CloseInventory와 달리 상태/커서는 DiaryUI가 이어받음)
		inventoryPanel?.SetActive(false);
		if (detailPanel != null)
			detailPanel.SetActive(false);

		if (_diaryUI != null)
		{
			// returnToInventory = true → 다이어리 닫으면 인벤토리 다시 열림
			_diaryUI.OpenDiary(_selectedItem.pages, returnToInventory: true);
		}
		else
		{
			Debug.LogError("[InventoryUI] DiaryUI를 찾을 수 없습니다!");
		}
	}



	/// <summary>
	/// 아이템 3D 보기
	/// [수정] ItemViewer3D 컴포넌트를 통해 처리
	/// </summary>
	private void View3DItem()
	{
		if (_selectedItem == null || _selectedItem.itemPrefab == null)
		{
			Debug.LogWarning("[InventoryUI] 3D 모델이 없습니다.");
			return;
		}

		if (itemViewer3D == null)
		{
			Debug.LogError("[InventoryUI] ItemViewer3D가 연결되지 않았습니다!");
			return;
		}

		// 인벤토리 패널 숨기기 (뷰어가 닫히면 인벤토리로 복귀)
		inventoryPanel?.SetActive(false);
		if (detailPanel != null)
			detailPanel.SetActive(false);

		itemViewer3D.OpenViewer(_selectedItem.itemPrefab, _selectedItem.title, this);
	}

	/// <summary>
	/// 아이템 제거 후 UI 갱신
	/// 퍼즐 배치 등 아이템 소비 시 호출
	/// </summary>
	public void RemoveItem(string itemId)
	{
		int removed = _allItems.RemoveAll(i => i.itemId == itemId);

		if (removed == 0)
		{
			Debug.LogWarning($"[InventoryUI] 제거할 아이템 없음: {itemId}");
			return;
		}

		// 선택된 아이템이 제거된 경우 상세 패널 닫기
		if (_selectedItem != null && _selectedItem.itemId == itemId)
		{
			_selectedItem = null;
			if (detailPanel != null)
				detailPanel.SetActive(false);
		}

		RefreshInventory();
		Debug.Log($"[InventoryUI] 아이템 제거 완료: {itemId}");
	}


	// ─────────────────────────────────────────────
	// I키 토글
	// ─────────────────────────────────────────────

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			if (inventoryPanel != null && inventoryPanel.activeSelf)
				CloseInventory();
			else
				OpenInventory();
		}
	}
}

/// <summary>아이템 타입</summary>
public enum ItemType
{
	Document,   // 문서 (일기장, 편지 등)
	UsableItem  // 사용 가능한 아이템 (열쇠, 도구 등)
}

/// <summary>인벤토리 아이템 데이터</summary>
[System.Serializable]
public class InventoryItemData
{
	public string itemId;
	public string title;
	public string date;
	public ItemType itemType;
	public string description;
	public List<string> pages;       // 문서 페이지 (Document만)
	public GameObject itemPrefab;    // 3D 모델 (UsableItem만)
}

/// <summary>아이템 사용 가능한 오브젝트 인터페이스</summary>
public interface IItemUsable
{
	void UseItem(string itemId);
	bool CanUseItem(string itemId);
}