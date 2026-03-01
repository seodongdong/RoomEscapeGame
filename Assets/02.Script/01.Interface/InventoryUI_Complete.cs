using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 완전한 인벤토리 시스템
/// - 탭: Documents / Items
/// - 문서: 읽기 버튼 → DiaryUI
/// - 아이템: 사용하기 / 보기(3D 뷰어)
/// - Painscreek 스타일 2단 레이아웃
/// </summary>
public class InventoryUI_Complete : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private GameObject inventoryPanel;

	[Header("Tabs")]
	[SerializeField] private Button documentsTab;
	[SerializeField] private Button itemsTab;
	[SerializeField] private GameObject documentsTabHighlight;  // 선택된 탭 표시
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
	[SerializeField] private Button readButton;      // 문서: 읽기
	[SerializeField] private Button useButton;       // 아이템: 사용하기
	[SerializeField] private Button viewButton;      // 아이템: 3D 보기
	[SerializeField] private Button closeButton;     // 인벤토리 닫기

	[Header("3D Item Viewer")]
	[SerializeField] private GameObject itemViewerPanel;
	[SerializeField] private Transform itemViewerPoint;     // 아이템 표시 위치
	[SerializeField] private Camera itemViewerCamera;
	[SerializeField] private Button exitViewerButton;       // 뷰어 나가기
	[SerializeField] private float rotationSpeed = 100f;

	private Player _player;
	private DiaryUI _diaryUI;
	private List<InventoryItemData> _allItems = new List<InventoryItemData>();
	private InventoryItemData _selectedItem;
	private string _currentTab = "Documents";  // Documents or Items

	private GameObject _currentViewerObject;
	private bool _isDragging = false;
	private Vector3 _lastMousePosition;

	private void Awake()
	{
		if (inventoryPanel != null)
			inventoryPanel.SetActive(false);

		if (detailPanel != null)
			detailPanel.SetActive(false);

		if (itemViewerPanel != null)
			itemViewerPanel.SetActive(false);

		// 버튼 이벤트
		documentsTab?.onClick.AddListener(() => SwitchTab("Documents"));
		itemsTab?.onClick.AddListener(() => SwitchTab("Items"));

		closeButton?.onClick.AddListener(CloseInventory);
		readButton?.onClick.AddListener(ReadDocument);
		useButton?.onClick.AddListener(UseItem);
		viewButton?.onClick.AddListener(View3DItem);
		exitViewerButton?.onClick.AddListener(ExitViewer);
	}

	private void Start()
	{
		_player = FindAnyObjectByType<Player>();
		_diaryUI = FindAnyObjectByType<DiaryUI>();
	}

	/// <summary>
	/// 인벤토리 열기
	/// </summary>
	public void OpenInventory()
	{
		inventoryPanel?.SetActive(true);

		if (_player != null)
			_player.enabled = false;

		GameManager.Instance?.StateManager.ChangeState(GameState.Puzzle);

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		RefreshInventory();

		Debug.Log("[InventoryUI] 인벤토리 열림");
	}

	/// <summary>
	/// 인벤토리 닫기
	/// </summary>
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

		Debug.Log("[InventoryUI] 인벤토리 닫힘");
	}

	/// <summary>
	/// 탭 전환
	/// </summary>
	private void SwitchTab(string tab)
	{
		_currentTab = tab;

		// 탭 하이라이트 업데이트
		if (documentsTabHighlight != null)
			documentsTabHighlight.SetActive(tab == "Documents");

		if (itemsTabHighlight != null)
			itemsTabHighlight.SetActive(tab == "Items");

		RefreshInventory();

		// 상세 패널 닫기
		if (detailPanel != null)
			detailPanel.SetActive(false);

		Debug.Log($"[InventoryUI] 탭 전환: {tab}");
	}

	/// <summary>
	/// 아이템 추가
	/// </summary>
	public void AddItem(InventoryItemData item)
	{
		_allItems.Add(item);

		// 날짜순 정렬
		_allItems = _allItems.OrderBy(i => i.date).ToList();

		Debug.Log($"[InventoryUI] 아이템 추가: {item.title} ({item.itemType})");
	}

	/// <summary>
	/// 인벤토리 목록 새로고침
	/// </summary>
	private void RefreshInventory()
	{
		// 기존 버튼 제거
		foreach (Transform child in itemListContent)
		{
			Destroy(child.gameObject);
		}

		// 현재 탭에 맞는 아이템만 표시
		var filteredItems = _allItems.Where(i =>
			(_currentTab == "Documents" && i.itemType == ItemType.Document) ||
			(_currentTab == "Items" && i.itemType == ItemType.UsableItem)
		).ToList();

		// 아이템 버튼 생성
		foreach (var item in filteredItems)
		{
			GameObject btn = Instantiate(itemButtonPrefab, itemListContent);

			var tmpText = btn.GetComponentInChildren<TextMeshProUGUI>();
			if (tmpText != null)
			{
				tmpText.text = $"{item.date} - {item.title}";
			}

			var button = btn.GetComponent<Button>();
			if (button != null)
			{
				var itemCopy = item;
				button.onClick.AddListener(() => SelectItem(itemCopy));
			}
		}
	}

	/// <summary>
	/// 아이템 선택
	/// </summary>
	private void SelectItem(InventoryItemData item)
	{
		_selectedItem = item;

		if (detailPanel != null)
			detailPanel.SetActive(true);

		if (detailTitle != null)
			detailTitle.text = item.title;

		if (detailDate != null)
			detailDate.text = item.date;

		// 내용 표시
		if (detailContent != null)
		{
			if (item.pages != null && item.pages.Count > 0)
			{
				detailContent.text = string.Join("\n\n───────────\n\n", item.pages);
			}
			else
			{
				detailContent.text = item.description;
			}
		}

		// 버튼 표시/숨김
		UpdateActionButtons(item.itemType);

		Debug.Log($"[InventoryUI] 아이템 선택: {item.title}");
	}

	/// <summary>
	/// 액션 버튼 업데이트
	/// </summary>
	private void UpdateActionButtons(ItemType itemType)
	{
		if (itemType == ItemType.Document)
		{
			// 문서: 읽기만 표시
			readButton?.gameObject.SetActive(true);
			useButton?.gameObject.SetActive(false);
			viewButton?.gameObject.SetActive(false);
		}
		else if (itemType == ItemType.UsableItem)
		{
			// 아이템: 사용하기 + 보기
			readButton?.gameObject.SetActive(false);
			useButton?.gameObject.SetActive(true);
			viewButton?.gameObject.SetActive(true);
		}
	}

	/// <summary>
	/// 문서 읽기 (일기장 열기)
	/// </summary>
	private void ReadDocument()
	{
		if (_selectedItem == null || _selectedItem.pages == null) return;

		CloseInventory();

		if (_diaryUI != null)
		{
			_diaryUI.OpenDiary(_selectedItem.pages);
		}
	}

	/// <summary>
	/// 아이템 사용하기
	/// </summary>
	private void UseItem()
	{
		if (_selectedItem == null) return;

		Debug.Log($"[InventoryUI] 아이템 사용: {_selectedItem.title}");

		// 인벤토리 닫기
		CloseInventory();

		// 플레이어가 바라보는 오브젝트에 아이템 사용
		if (_player != null)
		{
			// Raycast로 상호작용 가능한 오브젝트 찾기
			RaycastHit hit;
			if (Physics.Raycast(_player.Transform.position, _player.Transform.forward, out hit, 3f))
			{
				var usable = hit.collider.GetComponent<IItemUsable>();
				if (usable != null)
				{
					usable.UseItem(_selectedItem.itemId);
				}
				else
				{
					Debug.Log("[InventoryUI] 여기엔 사용할 수 없습니다.");
				}
			}
		}
	}

	/// <summary>
	/// 아이템 3D 보기
	/// </summary>
	private void View3DItem()
	{
		if (_selectedItem == null || _selectedItem.itemPrefab == null) return;

		itemViewerPanel?.SetActive(true);

		// 기존 오브젝트 제거
		if (_currentViewerObject != null)
		{
			Destroy(_currentViewerObject);
		}

		// 새 오브젝트 생성
		_currentViewerObject = Instantiate(_selectedItem.itemPrefab, itemViewerPoint.position, Quaternion.identity);
		_currentViewerObject.transform.SetParent(itemViewerPoint);
		_currentViewerObject.transform.localPosition = Vector3.zero;

		// 레이어 설정 (카메라에만 보이도록)
		SetLayerRecursively(_currentViewerObject, LayerMask.NameToLayer("UI"));

		Debug.Log($"[InventoryUI] 3D 뷰어 열림: {_selectedItem.title}");
	}

	/// <summary>
	/// 3D 뷰어 나가기
	/// </summary>
	private void ExitViewer()
	{
		itemViewerPanel?.SetActive(false);

		if (_currentViewerObject != null)
		{
			Destroy(_currentViewerObject);
			_currentViewerObject = null;
		}

		Debug.Log("[InventoryUI] 3D 뷰어 닫힘");
	}

	/// <summary>
	/// 레이어 재귀 설정
	/// </summary>
	private void SetLayerRecursively(GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, layer);
		}
	}

	private void Update()
	{
		// I키로 인벤토리 토글
		if (Input.GetKeyDown(KeyCode.I))
		{
			if (inventoryPanel != null && inventoryPanel.activeSelf)
			{
				CloseInventory();
			}
			else
			{
				OpenInventory();
			}
		}

		// 3D 뷰어 회전
		if (itemViewerPanel != null && itemViewerPanel.activeSelf && _currentViewerObject != null)
		{
			HandleItemRotation();
		}
	}

	/// <summary>
	/// 마우스 드래그로 아이템 회전
	/// </summary>
	private void HandleItemRotation()
	{
		if (Input.GetMouseButtonDown(0))
		{
			_isDragging = true;
			_lastMousePosition = Input.mousePosition;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			_isDragging = false;
		}

		if (_isDragging)
		{
			Vector3 delta = Input.mousePosition - _lastMousePosition;

			_currentViewerObject.transform.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime, Space.World);
			_currentViewerObject.transform.Rotate(Vector3.right, delta.y * rotationSpeed * Time.deltaTime, Space.World);

			_lastMousePosition = Input.mousePosition;
		}
	}
}

/// <summary>
/// 아이템 타입
/// </summary>
public enum ItemType
{
	Document,    // 문서 (일기장, 편지 등)
	UsableItem   // 사용 가능한 아이템 (열쇠, 도구 등)
}

/// <summary>
/// 인벤토리 아이템 데이터
/// </summary>
[System.Serializable]
public class InventoryItemData
{
	public string itemId;               // "key_bedroom" (사용 시 확인용)
	public string title;                // "침실 열쇠"
	public string date;                 // "2023.07.15"
	public ItemType itemType;           // Document or UsableItem
	public string description;          // 설명
	public List<string> pages;          // 문서 페이지 (Document만)
	public GameObject itemPrefab;       // 3D 모델 (UsableItem만)
}

/// <summary>
/// 아이템 사용 가능한 오브젝트 인터페이스
/// </summary>
public interface IItemUsable
{
	void UseItem(string itemId);
	bool CanUseItem(string itemId);
}