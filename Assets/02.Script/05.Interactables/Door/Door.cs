using UnityEngine;
using System.Collections;

/// <summary>
/// 문 스크립트
/// - requiredKeyId 없음: 그냥 잠긴 문 (lockedDialogue 출력)
/// - requiredKeyId 있음: 열쇠 보유 시 자동으로 열림
/// </summary>
public class Door : MonoBehaviour, IInteractable
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
	[SerializeField] private string noKeyDialogue = "열쇠가 없다.";
	[TextArea(2, 5)]
	[SerializeField] private string openDialogue = "문이 열렸다!";

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

	public string InteractionPrompt
	{
		get
		{
			if (!isLocked) return _isOpen ? "[F] 문 닫기" : "[F] 문 열기";
			return string.IsNullOrEmpty(requiredKeyId)
				? "[F] 문 (잠김)"
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

		// 열쇠 없는 그냥 잠긴 문
		if (string.IsNullOrEmpty(requiredKeyId))
		{
			uiManager?.ShowDialogue(speaker, lockedDialogue);
			return;
		}

		// 열쇠 필요한 문 → 인벤토리 확인
		if (!player.Inventory.HasItem(requiredKeyId))
		{
			uiManager?.ShowDialogue(speaker, noKeyDialogue);
			return;
		}

		// 열쇠 보유 → 바로 열기
		isLocked = false;

		if (consumeKey)
		{
			var key = player.Inventory.GetItem(requiredKeyId);
			if (key != null) player.Inventory.RemoveItem(key);
		}

		OpenDoor();
		uiManager?.ShowDialogue(speaker, openDialogue);

		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("door_unlock");
	}

	private void OpenDoor()
	{
		_isOpen = true;
		_isMoving = true;

		// 콜라이더를 Trigger로 변경 (Raycast는 되지만 통과 가능)
		var col = GetComponent<Collider>();
		if (col != null) col.isTrigger = true;

		if (doorAnimator != null)
			doorAnimator.SetTrigger("Open");
		else
			StartCoroutine(SlideDoor(true));
	}

	private void CloseDoor()
	{
		_isOpen = false;
		_isMoving = true;

		if (doorAnimator != null)
			doorAnimator.SetTrigger("Close");
		else
			StartCoroutine(SlideDoor(false));
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

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position + openOffset, transform.localScale);
		Gizmos.DrawLine(transform.position, transform.position + openOffset);
	}
}