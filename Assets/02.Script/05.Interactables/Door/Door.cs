using UnityEngine;
using System.Collections;

/// <summary>
/// 완전한 문 스크립트
/// - 일반 잠긴 문 / 열쇠 필요 문 / 열고 닫기 토글
/// - lockAfterOpen: 한 번 열리면 자동으로 닫히고 퍼즐 완료 전까지 잠김
/// - 열쇠 사용 시 거리 + 방향 체크 (문 근처에서 문을 향해야만 사용 가능)
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
	[Tooltip("플레이어가 문을 향해 있어야 하는 각도 범위 (클수록 넓음, 0~1 사이 dot값)")]
	[SerializeField] private float keyUseFacingDot = 0.3f;

	[Header("한 번 열리면 다시 잠기는 옵션")]
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
	private bool _keyUsed = false;  // 열쇠 사용 여부 (자유 출입 판단용)
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
		// lockAfterOpen으로 잠긴 경우
		if (_lockedByPuzzle)
		{
			_lockedByPuzzle = false;
			isLocked = false;
			Debug.Log("[Door] 퍼즐 완료 → lockAfterOpen 잠금 해제");
			return;
		}

		// 열쇠 없이 잠긴 문 (출구 등) — 퍼즐 완료 시 잠금 해제
		if (isLocked && string.IsNullOrEmpty(requiredKeyId))
		{
			isLocked = false;
			Debug.Log("[Door] 퍼즐 완료 → 잠금 해제");
		}
	}

	// ── 외부에서 자유 출입 해제 (ExitTrigger 등에서 호출) ────

	/// <summary>
	/// 열쇠를 이미 사용한 문에서 lockAfterOpen으로 잠긴 상태를 풀어줍니다.
	/// 이후 자유롭게 여닫을 수 있습니다.
	/// </summary>
	public void UnlockForFreeAccess()
	{
		if (!_keyUsed) return; // 열쇠 사용 전에는 효과 없음
		_lockedByPuzzle = false;
		isLocked = false;
		Debug.Log("[Door] 자유 출입 해제됨");
	}

	// ── IInteractable ─────────────────────────────────────────

	public string InteractionPrompt
	{
		get
		{
			if (_lockedByPuzzle) return "[F] 문 (잠김)";
			if (!isLocked) return _isOpen ? "[F] 문 닫기" : "[F] 문 열기";
			if (string.IsNullOrEmpty(requiredKeyId)) return "[F] 문 (잠김)";
			return $"[F] 잠긴 문 ({requiredKeyName} 필요)";
		}
	}

	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		if (_isMoving) return;
		var ui = FindAnyObjectByType<UIManager>();

		if (_lockedByPuzzle) { ui?.ShowDialogue(speaker, puzzleLockedDialogue); return; }
		if (!isLocked) { if (_isOpen) CloseDoor(); else OpenDoor(); return; }
		if (string.IsNullOrEmpty(requiredKeyId)) ui?.ShowDialogue(speaker, lockedDialogue);
		else ui?.ShowDialogue(speaker, needKeyDialogue);
	}

	// ── IItemUsable ───────────────────────────────────────────

	public bool CanUseItem(string itemId)
	{
		if (itemId != requiredKeyId || !isLocked) return false;

		// 거리 + 방향 체크
		var player = FindAnyObjectByType<Player>();
		if (player == null) return false;

		// 거리 체크
		float dist = Vector3.Distance(player.transform.position, transform.position);
		if (dist > keyUseDistance)
		{
			if (!string.IsNullOrEmpty(tooFarDialogue))
				FindAnyObjectByType<UIManager>()?.ShowDialogue(speaker, tooFarDialogue);
			return false;
		}

		// 방향 체크 (플레이어가 문을 향해 있는지)
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
		_keyUsed = true;  // 열쇠 사용 기록

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

		// 열쇠로 연 문이면 → lockAfterOpen이지만 _lockedByPuzzle만 true로 (isLocked는 false 유지)
		// UnlockForFreeAccess() 호출 시 _lockedByPuzzle만 풀면 됨
		if (!_keyUsed)
		{
			isLocked = true;
		}
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
			transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, elapsed / openDuration));
			yield return null;
		}
		transform.position = end;
		_isMoving = false;

		if (!opening) { var col = GetComponent<Collider>(); if (col != null) col.isTrigger = false; }
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position + openOffset, transform.localScale);
		Gizmos.DrawLine(transform.position, transform.position + openOffset);

		// 열쇠 사용 범위 시각화
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, keyUseDistance);
	}
}