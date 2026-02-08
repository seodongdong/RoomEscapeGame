using UnityEngine;

/// <summary>
/// 일반 문 (열쇠 필요)
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
	[Header("Door Settings")]
	[SerializeField] private bool isLocked = true;
	[SerializeField] private string requiredKeyId;
	[SerializeField] private bool requiresGirl;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string lockedDialogue = "잠겨있다...";
	[TextArea(2, 5)]
	[SerializeField] private string noKeyDialogue = "열쇠가 필요하다.";
	[TextArea(2, 5)]
	[SerializeField] private string openDialogue = "문이 열렸다!";

	[Header("Animation")]
	[SerializeField] private Animator doorAnimator;

	public string InteractionPrompt
	{
		get
		{
			if (isLocked)
			{
				if (requiresGirl)
					return "[F] 문 열기 (소녀가 필요합니다)";
				if (!string.IsNullOrEmpty(requiredKeyId))
					return "[F] 문 열기 (열쇠가 필요합니다)";
				return "[F] 문 열기 (잠김)";
			}
			return "[F] 문 열기";
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

		// 열쇠 체크
		bool hasKey = string.IsNullOrEmpty(requiredKeyId) ||
					  player.Inventory.HasItem(requiredKeyId);

		// 소녀 체크
		bool hasGirl = !requiresGirl;

		if (!hasKey)
		{
			uiManager?.ShowDialogue(speaker, noKeyDialogue);
			return;
		}

		if (!hasGirl)
		{
			uiManager?.ShowDialogue(speaker, "소녀와 함께 있어야 열 수 있다.");
			return;
		}

		// 문 열기
		isLocked = false;
		OpenDoor();
		uiManager?.ShowDialogue(speaker, openDialogue);

		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("door_unlock");
	}

	private void OpenDoor()
	{
		if (doorAnimator != null)
		{
			doorAnimator.SetTrigger("Open");
		}
		else
		{
			// 애니메이션 없으면 그냥 비활성화
			gameObject.SetActive(false);
		}
	}
}