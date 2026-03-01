using UnityEngine;
using System.Collections;

/// <summary>
/// 완전한 문 스크립트
/// - 일반 잠긴 문 (requiredKeyId 비움)
/// - 열쇠 필요 문 (requiredKeyId 설정) - 인벤토리에서 "사용하기"만 가능
/// - 열고 닫기 토글
/// - 애니메이터 또는 슬라이드 이동 지원
/// </summary>
public class Door : MonoBehaviour, IInteractable, IItemUsable
{
	[Header("Door Settings")]
	[SerializeField] private bool isLocked = true;
	[SerializeField] private string requiredKeyId;         // 비워두면 열쇠 없는 잠긴 문
	[SerializeField] private string requiredKeyName = "열쇠";
	[SerializeField] private bool consumeKey = true;       // 열쇠 사용 후 인벤토리에서 제거

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string lockedDialogue = "잠겨있다...";
	[TextArea(2, 5)]
	[SerializeField] private string needKeyDialogue = "열쇠가 필요할 것 같다.";
	[TextArea(2, 5)]
	[SerializeField] private string openDialogue = "문이 열렸다!";
	[TextArea(2, 5)]
	[SerializeField] private string wrongItemDialogue = "이 아이템은 여기에 사용할 수 없다.";

	[Header("Open Settings")]
	[SerializeField] private Animator doorAnimator;
	[SerializeField] private Vector3 openOffset = new Vector3(0, 3, 0); // 애니메이터 없을 때 이동
	[SerializeField] private float openDuration = 1f;

	private bool _isOpen = false;
	private bool _isMoving = false; // 이동 중 중복 클릭 방지
	private Vector3 _closedPosition;

	private void Awake()
	{
		_closedPosition = transform.position;
	}

	// ========== IInteractable ==========
	public string InteractionPrompt
	{
		get
		{
			if (!isLocked) return _isOpen ? "[F] 문 닫기" : "[F] 문 열기";

			// 열쇠 필요 여부 표시
			if (string.IsNullOrEmpty(requiredKeyId))
				return "[F] 문 (잠김)";
			else
				return $"[F] 잠긴 문 ({requiredKeyName} 필요)";
		}
	}

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();

		// 이동 중이면 무시
		if (_isMoving) return;

		// 잠금 해제된 문 → 열고 닫기 토글
		if (!isLocked)
		{
			if (_isOpen)
				CloseDoor();
			else
				OpenDoor();
			return;
		}

		// 잠긴 문 → 대사만 출력
		if (string.IsNullOrEmpty(requiredKeyId))
		{
			// 열쇠 없는 일반 잠긴 문
			uiManager?.ShowDialogue(speaker, lockedDialogue);
		}
		else
		{
			// 열쇠 필요한 문 → 인벤토리 사용 안내
			uiManager?.ShowDialogue(speaker, needKeyDialogue);
		}
	}

	// ========== IItemUsable (인벤토리에서 아이템 사용) ==========
	public bool CanUseItem(string itemId)
	{
		return itemId == requiredKeyId && isLocked;
	}

	public void UseItem(string itemId)
	{
		var uiManager = FindAnyObjectByType<UIManager>();

		if (CanUseItem(itemId))
		{
			// 올바른 아이템 → 문 열림
			var player = FindAnyObjectByType<Player>();
			if (player != null)
			{
				UnlockAndOpen(player);
			}
			else
			{
				// Player 없으면 그냥 잠금 해제만
				isLocked = false;
				OpenDoor();
				uiManager?.ShowDialogue(speaker, openDialogue);
			}

			Debug.Log($"[Door] {itemId} 사용 → 문 열림!");
		}
		else
		{
			// 잘못된 아이템
			uiManager?.ShowDialogue(speaker, wrongItemDialogue);
			Debug.Log($"[Door] {itemId}는 사용할 수 없음");
		}
	}

	// ========== 문 열기 통합 메서드 ==========
	private void UnlockAndOpen(IPlayer player)
	{
		isLocked = false;

		// 열쇠 소비
		if (consumeKey && !string.IsNullOrEmpty(requiredKeyId))
		{
			var key = player.Inventory.GetItem(requiredKeyId);
			if (key != null)
			{
				player.Inventory.RemoveItem(key);
				Debug.Log($"[Door] {requiredKeyName} 소비됨");
			}
		}

		OpenDoor();

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, openDialogue);

		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("door_unlock");
	}

	// ========== 문 열기/닫기 ==========
	private void OpenDoor()
	{
		_isOpen = true;
		_isMoving = true;

		// 콜라이더를 Trigger로 변경 (Raycast는 되지만 통과 가능)
		var col = GetComponent<Collider>();
		if (col != null) col.isTrigger = true;

		if (doorAnimator != null)
		{
			doorAnimator.SetTrigger("Open");
			// 애니메이터 사용 시 이동 완료 시점을 알 수 없으므로 즉시 해제
			_isMoving = false;
		}
		else
		{
			StartCoroutine(SlideDoor(true));
		}

		Debug.Log("[Door] 문이 열렸습니다.");
	}

	private void CloseDoor()
	{
		_isOpen = false;
		_isMoving = true;

		if (doorAnimator != null)
		{
			doorAnimator.SetTrigger("Close");
			_isMoving = false;
		}
		else
		{
			StartCoroutine(SlideDoor(false));
		}

		Debug.Log("[Door] 문이 닫혔습니다.");
	}

	private IEnumerator SlideDoor(bool opening)
	{
		Vector3 startPos = opening ? _closedPosition : _closedPosition + openOffset;
		Vector3 endPos = opening ? _closedPosition + openOffset : _closedPosition;

		float elapsed = 0f;
		while (elapsed < openDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / openDuration;
			transform.position = Vector3.Lerp(startPos, endPos, t);
			yield return null;
		}

		transform.position = endPos;
		_isMoving = false;

		// 닫힐 때 콜라이더 물리 충돌 복구
		if (!opening)
		{
			var col = GetComponent<Collider>();
			if (col != null) col.isTrigger = false;
		}
	}

	// ========== Gizmos ==========
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position + openOffset, transform.localScale);
		Gizmos.DrawLine(transform.position, transform.position + openOffset);
	}
}