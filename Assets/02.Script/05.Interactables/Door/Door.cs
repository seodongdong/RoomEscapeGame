using UnityEngine;

/// <summary>
/// 문 (수동 아이템 선택 방식)
/// 플레이어가 인벤토리에서 열쇠를 직접 선택해야 함
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
	[Header("Door Settings")]
	[SerializeField] private bool isLocked = true;
	[SerializeField] private string requiredKeyId;
	[SerializeField] private string requiredKeyName = "열쇠";

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string lockedDialogue = "잠겨있다...";
	[TextArea(2, 5)]
	[SerializeField] private string noKeyDialogue = "열쇠가 없다.";
	[TextArea(2, 5)]
	[SerializeField] private string wrongKeyDialogue = "이 열쇠가 아닌 것 같다.";
	[TextArea(2, 5)]
	[SerializeField] private string openDialogue = "문이 열렸다!";

	[Header("Animation")]
	[SerializeField] private Animator doorAnimator;

	private InventoryUI _inventoryUI;

	public string InteractionPrompt
	{
		get
		{
			if (!isLocked) return "[F] 문 열기";
			return string.IsNullOrEmpty(requiredKeyId)
				? "[F] 문 열기 (잠김)"
				: $"[F] 문 열기 ({requiredKeyName} 필요)";
		}
	}

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();

		if (!isLocked)
		{
			OpenDoor();
			uiManager?.ShowDialogue(speaker, openDialogue);
			return;
		}

		// 열쇠가 필요 없는 잠긴 문
		if (string.IsNullOrEmpty(requiredKeyId))
		{
			uiManager?.ShowDialogue(speaker, lockedDialogue);
			return;
		}

		// 인벤토리에 열쇠 있는지 확인
		if (!player.Inventory.HasItem(requiredKeyId))
		{
			uiManager?.ShowDialogue(speaker, noKeyDialogue);
			return;
		}

		// 🆕 인벤토리 열고 플레이어가 직접 선택
		_inventoryUI = FindAnyObjectByType<InventoryUI>();
		if (_inventoryUI != null)
		{
			// 인벤토리 열고 선택 모드 시작
			_inventoryUI.OpenForItemSelect(requiredKeyId, (selectedId) =>
			{
				if (selectedId == requiredKeyId)
				{
					// 올바른 열쇠 선택
					isLocked = false;
					player.Inventory.RemoveItem(player.Inventory.GetItem(requiredKeyId));
					OpenDoor();
					uiManager?.ShowDialogue(speaker, openDialogue);

					var audioManager = FindAnyObjectByType<AudioManager>();
					audioManager?.PlaySFX("door_unlock");
				}
				else
				{
					// 잘못된 아이템 선택
					uiManager?.ShowDialogue(speaker, wrongKeyDialogue);
				}
			});
		}
		else
		{
			// 인벤토리 UI 없으면 자동 매칭 (fallback)
			isLocked = false;
			player.Inventory.RemoveItem(player.Inventory.GetItem(requiredKeyId));
			OpenDoor();
			uiManager?.ShowDialogue(speaker, openDialogue);
		}
	}

	private void OpenDoor()
	{
		if (doorAnimator != null)
		{
			doorAnimator.SetTrigger("Open");
		}
		else
		{
			gameObject.SetActive(false);
		}
	}
}