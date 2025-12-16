using UnityEngine;

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
	[SerializeField] private string noKeyDialogue = "열쇠가 필요할 것 같다.";
	[TextArea(2, 5)]
	[SerializeField] private string noGirlDialogue = "혼자서는 열 수 없을 것 같다.";
	[TextArea(2, 5)]
	[SerializeField] private string openDialogue = "문이 열렸다!";

	public string InteractionPrompt
	{
		get
		{
			if (isLocked)
			{
				if (requiresGirl)
				{
					return "[F] 문 열기 (소녀가 필요합니다)";
				}
				else if (!string.IsNullOrEmpty(requiredKeyId))
				{
					return "[F] 문 열기 (열쇠가 필요합니다)";
				}
				else
				{
					return "[F] 문 열기 (잠겨있음)";
				}
			}
			return "[F] 문 열기";
		}
	}

	public bool CanInteract(IPlayer player)
	{
		// ⭐ 항상 상호작용 가능 (대사를 출력하기 위해)
		return true;
	}

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();

		// 문이 이미 열려있으면
		if (!isLocked)
		{
			Debug.Log("문이 열려있습니다. (스테이지 전환 또는 문 열림 애니메이션)");

			if (!string.IsNullOrEmpty(openDialogue))
			{
				uiManager?.ShowDialogue(speaker, openDialogue);
			}

			// 여기에 스테이지 전환 로직 추가 가능
			// GameManager.Instance.StageManager.CompleteStage();

			return;
		}

		// ⭐ 문이 잠겨있는 경우 - 조건 체크

		// 1. 열쇠 필요 여부 체크
		bool needsKey = !string.IsNullOrEmpty(requiredKeyId);
		bool hasKey = needsKey ? player.Inventory.HasItem(requiredKeyId) : true;

		// 2. 소녀 필요 여부 체크 (나중에 Girl 오브젝트 체크로 변경 가능)
		bool hasGirl = !requiresGirl; // requiresGirl이 false면 소녀 필요없음 = true

		// ⭐ 조건별 대사 출력 및 처리

		// 케이스 1: 열쇠도 필요하고 소녀도 필요한 경우
		if (needsKey && requiresGirl)
		{
			if (!hasKey && !hasGirl)
			{
				// 둘 다 없음
				Debug.Log("열쇠와 소녀가 모두 필요합니다.");
				uiManager?.ShowDialogue(speaker, "열쇠도 필요하고... 혼자서는 안 될 것 같다.");
			}
			else if (!hasKey)
			{
				// 열쇠만 없음
				Debug.Log("열쇠가 필요합니다.");
				uiManager?.ShowDialogue(speaker, noKeyDialogue);
			}
			else if (!hasGirl)
			{
				// 소녀만 없음
				Debug.Log("소녀가 필요합니다.");
				uiManager?.ShowDialogue(speaker, noGirlDialogue);
			}
			else
			{
				// 둘 다 있음 - 문 열림
				OpenDoor(uiManager);
			}
		}
		// 케이스 2: 열쇠만 필요한 경우
		else if (needsKey)
		{
			if (!hasKey)
			{
				Debug.Log("열쇠가 필요합니다.");
				uiManager?.ShowDialogue(speaker, noKeyDialogue);
			}
			else
			{
				// 열쇠 있음 - 문 열림
				OpenDoor(uiManager);
			}
		}
		// 케이스 3: 소녀만 필요한 경우
		else if (requiresGirl)
		{
			if (!hasGirl)
			{
				Debug.Log("소녀가 필요합니다.");
				uiManager?.ShowDialogue(speaker, noGirlDialogue);
			}
			else
			{
				// 소녀 있음 - 문 열림
				OpenDoor(uiManager);
			}
		}
		// 케이스 4: 아무 조건도 없는데 잠겨있는 경우 (퍼즐 등 외부에서 열림)
		else
		{
			Debug.Log("이 문은 다른 방법으로 열어야 합니다.");
			uiManager?.ShowDialogue(speaker, lockedDialogue);
		}
	}

	// ⭐ 문 열기 처리를 별도 메서드로 분리
	private void OpenDoor(IUIManager uiManager)
	{
		isLocked = false;
		Debug.Log("문이 열렸습니다!");

		if (!string.IsNullOrEmpty(openDialogue))
		{
			uiManager?.ShowDialogue(speaker, openDialogue);
		}

		// 문 열림 효과음
		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("door_open");
	}

	// ⭐ 외부에서 문을 잠그거나 열 수 있는 메서드
	public void SetLocked(bool locked)
	{
		isLocked = locked;
		Debug.Log($"문이 {(locked ? "잠겼습니다" : "열렸습니다")}.");
	}
}