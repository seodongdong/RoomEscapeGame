using UnityEngine;

/// <summary>
/// 아이템 사용 가능한 문
/// - 인벤토리에서 "사용하기" 클릭 → 문 열림
/// - IItemUsable 인터페이스 구현
/// </summary>
public class DoorWithItem : MonoBehaviour, IInteractable, IItemUsable
{
	[Header("Door Settings")]
	[SerializeField] private bool isLocked = true;
	[SerializeField] private string requiredItemId = "key_bedroom";  // 필요한 아이템 ID

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string lockedDialogue = "문이 잠겨있다... 열쇠가 필요할 것 같다.";
	[TextArea(2, 5)]
	[SerializeField] private string unlockedDialogue = "문이 열렸다!";
	[TextArea(2, 5)]
	[SerializeField] private string wrongItemDialogue = "이 아이템은 여기에 사용할 수 없다.";

	private Animator _animator;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	// ========== IInteractable ==========
	public string InteractionPrompt
	{
		get
		{
			if (isLocked)
				return "[F] 잠긴 문 (아이템 필요)";
			else
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

		if (isLocked)
		{
			// 잠김 → 대사 출력
			uiManager?.ShowDialogue(speaker, lockedDialogue);
		}
		else
		{
			// 열림 → 문 열기
			OpenDoor();
		}
	}

	// ========== IItemUsable ==========
	public bool CanUseItem(string itemId)
	{
		return itemId == requiredItemId && isLocked;
	}

	public void UseItem(string itemId)
	{
		var uiManager = FindAnyObjectByType<UIManager>();

		if (CanUseItem(itemId))
		{
			// 올바른 아이템 → 문 열림
			isLocked = false;
			uiManager?.ShowDialogue(speaker, unlockedDialogue);

			Debug.Log($"[Door] {itemId} 사용 → 문 열림!");
		}
		else
		{
			// 잘못된 아이템
			uiManager?.ShowDialogue(speaker, wrongItemDialogue);
			Debug.Log($"[Door] {itemId}는 사용할 수 없음");
		}
	}

	private void OpenDoor()
	{
		if (_animator != null)
		{
			_animator.SetTrigger("Open");
		}

		Debug.Log("[Door] 문이 열렸습니다!");
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