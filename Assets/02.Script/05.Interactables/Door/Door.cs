using UnityEngine;
using System.Collections;

/// <summary>
/// 완전한 문 스크립트
///
/// [기획서 기준 수정]
/// - 열쇠 없이 잠겨있을 때 : 프롬프트 미표시 + 철컥 효과음만
/// - 열쇠 있고 잠겨있을 때 : "열쇠 사용하기" 프롬프트
/// - 열쇠 사용 후           : "열기 / 닫기" 프롬프트
///
/// [기존 기능 유지]
/// - lockAfterOpen : 한 번 열리면 자동으로 닫히고 퍼즐 완료 전까지 잠김
/// - 열쇠 거리 + 방향 체크
/// - IItemUsable : 인벤토리에서 열쇠 아이템 직접 사용 가능
/// </summary>
public class Door : MonoBehaviour, IInteractable, IItemUsable
{
	[Header("Door Settings")]
	[SerializeField] private bool isLocked = true;
	[SerializeField] private string requiredKeyId = "";
	[SerializeField] private string requiredKeyName = "열쇠";
	[SerializeField] private bool consumeKey = true;

	[Header("열쇠 사용 범위")]
	[Tooltip("이 거리 이내에서만 열쇠를 사용할 수 있습니다.")]
	[SerializeField] private float keyUseDistance = 3f;
	[Tooltip("플레이어가 문을 향해 있어야 하는 각도 범위 (0~1 dot값, 클수록 넓음)")]
	[SerializeField] private float keyUseFacingDot = 0.3f;

	[Header("lockAfterOpen")]
	[Tooltip("켜두면 열린 직후 자동으로 닫히고 퍼즐 완료 전까지 다시 열리지 않습니다.")]
	[SerializeField] private bool lockAfterOpen = false;
	[SerializeField] private MonoBehaviour puzzleObject;
	[SerializeField] private float autoCloseDelay = 2f;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string lockedDialogue = "잠겨있다...";
	[TextArea(2, 4)][SerializeField] private string needKeyDialogue = "열쇠가 필요할 것 같다.";
	[TextArea(2, 4)][SerializeField] private string openDialogue = "문이 열렸다!";
	[TextArea(2, 4)][SerializeField] private string wrongItemDialogue = "이 아이템은 여기에 사용할 수 없다.";
	[TextArea(2, 4)][SerializeField] private string puzzleLockedDialogue = "아까 열었는데 다시 잠겨버렸다...";
	[TextArea(2, 4)][SerializeField] private string tooFarDialogue = "";  // 비워두면 대사 없음

	[Header("Open Settings")]
	[SerializeField] private Animator doorAnimator;
	[SerializeField] private Vector3 openOffset = new Vector3(0, 3, 0);
	[SerializeField] private float openDuration = 1f;

	// ── 런타임 상태 ──────────────────────────────────────────
	private IPuzzle _puzzle;
	private bool _isOpen = false;
	private bool _isMoving = false;
	private bool _lockedByPuzzle = false;
	private bool _keyUsed = false;
	private Vector3 _closedPosition;

	private void Awake()
	{
		_closedPosition = transform.position;
		_puzzle = puzzleObject as IPuzzle;
	}

	private void Start()
	{
		if (_puzzle != null)
			_puzzle.OnPuzzleSolved += OnPuzzleSolvedHandler;
	}

	private void OnDestroy()
	{
		if (_puzzle != null)
			_puzzle.OnPuzzleSolved -= OnPuzzleSolvedHandler;
	}

	// ── 퍼즐 완료 콜백 ───────────────────────────────────────

	private void OnPuzzleSolvedHandler()
	{
		if (_lockedByPuzzle)
		{
			_lockedByPuzzle = false;
			isLocked = false;
			Debug.Log("[Door] 퍼즐 완료 → lockAfterOpen 잠금 해제");
			return;
		}

		if (isLocked && string.IsNullOrEmpty(requiredKeyId))
		{
			isLocked = false;
			Debug.Log("[Door] 퍼즐 완료 → 잠금 해제");
		}
	}

	// ── 외부 호출 ─────────────────────────────────────────────

	public void UnlockForFreeAccess()
	{
		if (!_keyUsed) return;
		_lockedByPuzzle = false;
		isLocked = false;
		Debug.Log("[Door] 자유 출입 해제됨");
	}

	// ── IInteractable ─────────────────────────────────────────

	public string InteractionPrompt
	{
		get
		{
			// ★ lockAfterOpen으로 잠긴 상태: 프롬프트 표시 (이미 열쇠는 썼으니 안내)
			if (_lockedByPuzzle) return "[F] 문 (잠김)";

			// ★ 열쇠 사용 후 자유 출입: 열기/닫기
			if (!isLocked) return _isOpen ? "[F] 문 닫기" : "[F] 문 열기";

			// ★ 잠긴 문
			if (string.IsNullOrEmpty(requiredKeyId))
			{
				// 열쇠 없이 잠긴 문 → 기획서: 프롬프트 미표시
				// 빈 문자열 반환 → Player.cs가 ShowInteractionPrompt 호출 안 함
				return "";
			}

			// 열쇠 필요 문: 플레이어가 열쇠를 갖고 있는지에 따라 다른 프롬프트
			return $"[F] {requiredKeyName} 사용하기";
		}
	}

	public bool CanInteract(IPlayer player)
	{
		// 빈 프롬프트(열쇠 없이 잠긴 문)는 F키 눌렀을 때도 동작 허용
		// → Interact() 안에서 철컥 효과음만 냄
		return true;
	}

	public void Interact(IPlayer player)
	{
		if (_isMoving) return;
		var ui = FindAnyObjectByType<UIManager>();

		// lockAfterOpen으로 잠긴 상태
		if (_lockedByPuzzle)
		{
			ui?.ShowDialogue(speaker, puzzleLockedDialogue);
			FindAnyObjectByType<AudioManager>()?.PlaySFX("door_locked");
			return;
		}

		// 잠금 해제 → 여닫기
		if (!isLocked)
		{
			if (_isOpen) CloseDoor();
			else OpenDoor();
			return;
		}

		// 잠긴 문
		if (string.IsNullOrEmpty(requiredKeyId))
		{
			// ★ 기획서: 열쇠 없이 잠겨있을 때 → 대사 없음, 철컥 효과음만
			FindAnyObjectByType<AudioManager>()?.PlaySFX("door_locked");
			Debug.Log("[Door] 잠김 (철컥)");
		}
		else
		{
			// 열쇠 필요 문: 인벤토리에 열쇠 있으면 사용, 없으면 안내 대사
			if (player.Inventory.HasItem(requiredKeyId))
				UnlockAndOpen(player);
			else
				ui?.ShowDialogue(speaker, needKeyDialogue);
		}
	}

	// ── IItemUsable (인벤토리 사용 버튼) ─────────────────────

	public bool CanUseItem(string itemId)
	{
		if (itemId != requiredKeyId || !isLocked) return false;

		var player = FindAnyObjectByType<Player>();
		if (player == null) return false;

		float dist = Vector3.Distance(player.transform.position, transform.position);
		if (dist > keyUseDistance)
		{
			if (!string.IsNullOrEmpty(tooFarDialogue))
				FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, tooFarDialogue);
			return false;
		}

		Vector3 toDoor = (transform.position - player.transform.position).normalized;
		float dot = Vector3.Dot(player.transform.forward, toDoor);
		if (dot < keyUseFacingDot)
		{
			if (!string.IsNullOrEmpty(tooFarDialogue))
				FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, tooFarDialogue);
			return false;
		}

		return true;
	}

	public void UseItem(string itemId)
	{
		var ui = FindAnyObjectByType<UIManager>();

		if (CanUseItem(itemId))
		{
			var player = FindAnyObjectByType<Player>();
			if (player != null) UnlockAndOpen(player);
			else { isLocked = false; OpenDoor(); ui?.ShowDialogue(speaker, openDialogue); }
		}
		else
		{
			ui?.ShowDialogue(speaker, wrongItemDialogue);
		}
	}

	// ── 문 열기 통합 ─────────────────────────────────────────

	private void UnlockAndOpen(IPlayer player)
	{
		isLocked = false;
		_keyUsed = true;

		if (consumeKey && !string.IsNullOrEmpty(requiredKeyId))
		{
			var key = player.Inventory.GetItem(requiredKeyId);
			if (key != null) player.Inventory.RemoveItem(key);
		}

		OpenDoor();
		FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, openDialogue);
		FindAnyObjectByType<AudioManager>()?.PlaySFX("door_unlock");
	}

	private void OpenDoor()
	{
		_isOpen = true;
		_isMoving = true;

		var col = GetComponent<Collider>();
		if (col != null) col.isTrigger = true;

		if (doorAnimator != null) { doorAnimator.SetTrigger("Open"); _isMoving = false; }
		else StartCoroutine(SlideDoor(true));

		if (lockAfterOpen) StartCoroutine(AutoCloseAndLock());
	}

	private void CloseDoor()
	{
		_isOpen = false;
		_isMoving = true;

		if (doorAnimator != null) { doorAnimator.SetTrigger("Close"); _isMoving = false; }
		else StartCoroutine(SlideDoor(false));
	}

	// ── lockAfterOpen 자동 닫힘 ──────────────────────────────

	private IEnumerator AutoCloseAndLock()
	{
		yield return new WaitForSeconds(autoCloseDelay);
		CloseDoor();
		yield return new WaitForSeconds(openDuration);

		if (!_keyUsed) isLocked = true;
		_lockedByPuzzle = true;
		Debug.Log("[Door] 자동 잠김");
	}

	// ── 슬라이드 애니메이션 ───────────────────────────────────

	private IEnumerator SlideDoor(bool opening)
	{
		Vector3 start = opening ? _closedPosition : _closedPosition + openOffset;
		Vector3 end = opening ? _closedPosition + openOffset : _closedPosition;

		float elapsed = 0f;
		while (elapsed < openDuration)
		{
			elapsed += Time.deltaTime;
			transform.position = Vector3.Lerp(
				start, end,
				Mathf.SmoothStep(0, 1, elapsed / openDuration));
			yield return null;
		}
		transform.position = end;
		_isMoving = false;

		if (!opening)
		{
			var col = GetComponent<Collider>();
			if (col != null) col.isTrigger = false;
		}
	}

	// ── 기즈모 ────────────────────────────────────────────────

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position + openOffset, transform.localScale);
		Gizmos.DrawLine(transform.position, transform.position + openOffset);

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, keyUseDistance);
	}
}