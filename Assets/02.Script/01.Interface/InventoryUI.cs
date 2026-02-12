using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

/// <summary>
/// 인벤토리 UI
/// - 열쇠/도구 중심 슬롯형
/// - 수동 선택 방식
/// </summary>
public class InventoryUI : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private GameObject inventoryPanel;
	[SerializeField] private Transform slotContainer;       // 슬롯들의 부모
	[SerializeField] private GameObject slotPrefab;         // 슬롯 프리팹
	[SerializeField] private TextMeshProUGUI selectedItemName;
	[SerializeField] private TextMeshProUGUI selectedItemDesc;
	[SerializeField] private Image selectedItemIcon;
	[SerializeField] private Button useButton;
	[SerializeField] private Button closeButton;

	[Header("Select Mode UI")]
	[SerializeField] private GameObject selectModePanel;    // "아이템을 선택하세요" 패널
	[SerializeField] private TextMeshProUGUI selectModeText;

	private IPlayer _player;
	private string _selectedItemId;
	private bool _isSelectMode = false;
	private Action<string> _onItemSelected;
	private string _highlightItemId;

	public bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;

	private void Start()
	{
		_player = FindAnyObjectByType<Player>();

		if (closeButton != null)
			closeButton.onClick.AddListener(CloseInventory);

		if (useButton != null)
			useButton.onClick.AddListener(UseSelectedItem);

		inventoryPanel?.SetActive(false);
		selectModePanel?.SetActive(false);
	}

	private void Update()
	{
		// I키로 인벤토리 토글
		if (Input.GetKeyDown(KeyCode.I))
		{
			if (IsOpen && !_isSelectMode)
				CloseInventory();
			else if (!IsOpen)
				OpenInventory();
		}

		// ESC로 닫기 (선택 모드 아닐 때만)
		if (Input.GetKeyDown(KeyCode.Escape) && IsOpen && !_isSelectMode)
		{
			CloseInventory();
		}
	}

	public void OpenInventory()
	{
		inventoryPanel?.SetActive(true);
		GameManager.Instance?.StateManager.ChangeState(GameState.Paused);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		RefreshSlots();
	}

	public void CloseInventory()
	{
		inventoryPanel?.SetActive(false);
		selectModePanel?.SetActive(false);
		_isSelectMode = false;
		_onItemSelected = null;

		GameManager.Instance?.StateManager.ChangeState(GameState.Playing);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	/// <summary>
	/// 아이템 선택 모드로 인벤토리 열기 (문/장치 상호작용 시)
	/// </summary>
	public void OpenForItemSelect(string highlightItemId, Action<string> onSelected)
	{
		_isSelectMode = true;
		_highlightItemId = highlightItemId;
		_onItemSelected = onSelected;

		OpenInventory();

		// 선택 모드 안내 UI 표시
		if (selectModePanel != null)
		{
			selectModePanel.SetActive(true);
			if (selectModeText != null)
				selectModeText.text = "사용할 아이템을 선택하세요";
		}

		// 사용 버튼 텍스트 변경
		if (useButton != null)
		{
			var btnText = useButton.GetComponentInChildren<TextMeshProUGUI>();
			if (btnText != null)
				btnText.text = "선택";
		}
	}

	private void RefreshSlots()
	{
		if (_player == null || slotContainer == null) return;

		// 기존 슬롯 삭제
		foreach (Transform child in slotContainer)
		{
			Destroy(child.gameObject);
		}

		// 인벤토리 아이템 목록 가져오기
		var items = _player.Inventory.GetAllItems();

		foreach (var item in items)
		{
			CreateSlot(item);
		}

		// 빈 슬롯 표시
		if (items.Count == 0)
		{
			if (selectedItemName != null)
				selectedItemName.text = "인벤토리가 비어있습니다";
		}
	}

	private void CreateSlot(IItem item)
	{
		if (slotPrefab == null) return;

		GameObject slotObj = Instantiate(slotPrefab, slotContainer);
		var slot = slotObj.GetComponent<InventorySlot>();

		if (slot != null)
		{
			bool isHighlighted = _isSelectMode && item.ItemId == _highlightItemId;
			slot.Setup(item, isHighlighted, OnSlotClicked);
		}
	}

	private void OnSlotClicked(string itemId)
	{
		_selectedItemId = itemId;

		// 선택된 아이템 정보 표시
		var item = _player.Inventory.GetItem(itemId);
		if (item != null)
		{
			if (selectedItemName != null) selectedItemName.text = item.ItemName;
			if (selectedItemDesc != null) selectedItemDesc.text = item.Description;
			if (selectedItemIcon != null && item.Icon != null)
				selectedItemIcon.sprite = item.Icon;
		}

		// 선택 모드에서 바로 콜백
		if (_isSelectMode && _onItemSelected != null)
		{
			_onItemSelected.Invoke(itemId);
			CloseInventory();
		}
	}

	private void UseSelectedItem()
	{
		if (string.IsNullOrEmpty(_selectedItemId)) return;

		if (_isSelectMode && _onItemSelected != null)
		{
			_onItemSelected.Invoke(_selectedItemId);
			CloseInventory();
		}
	}
}