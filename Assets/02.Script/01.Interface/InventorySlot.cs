using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 인벤토리 슬롯
/// </summary>
public class InventorySlot : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private Image iconImage;
	[SerializeField] private TextMeshProUGUI itemNameText;
	[SerializeField] private GameObject highlightFrame;     // 선택 모드에서 강조
	[SerializeField] private Button slotButton;

	private string _itemId;
	private Action<string> _onClicked;

	private void Awake()
	{
		if (slotButton != null)
			slotButton.onClick.AddListener(OnClick);
	}

	public void Setup(IItem item, bool isHighlighted, Action<string> onClicked)
	{
		_itemId = item.ItemId;
		_onClicked = onClicked;

		if (iconImage != null && item.Icon != null)
			iconImage.sprite = item.Icon;

		if (itemNameText != null)
			itemNameText.text = item.ItemName;

		// 선택 모드에서 해당 열쇠 강조
		if (highlightFrame != null)
			highlightFrame.SetActive(isHighlighted);
	}

	private void OnClick()
	{
		_onClicked?.Invoke(_itemId);
	}
}