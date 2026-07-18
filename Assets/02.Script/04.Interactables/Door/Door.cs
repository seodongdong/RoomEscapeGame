using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour, IInteractable, IItemUsable, ISaveableObject
{
	[Header("Door Settings")]
	[SerializeField] private bool isLocked = true;
	[SerializeField] private string requiredKeyId = "";
	[SerializeField] private string requiredKeyName = "열쇠";
	[SerializeField] private bool consumeKey = true;

	[Header("열쇠 사용 범위")]
	[SerializeField] private float keyUseDistance = 3f;
	[SerializeField] private float keyUseFacingDot = 0.3f;

	[Header("lockAfterOpen")]
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
	[TextArea(2, 4)][SerializeField] private string tooFarDialogue = "";

	[Header("Open Settings")]
	[SerializeField] private Animator doorAnimator;
	[SerializeField] private Vector3 openOffset = new Vector3(0, 3, 0);
	[SerializeField] private float openDuration = 1f;

	[Header("저장 ID (씬 내 유일해야 함)")]
	[SerializeField] private string saveId = "door_001";

	// ── 런타임 상태 ──────────────────────────────────────────
	private IPuzzle _puzzle;
	private bool _isOpen = false;
	private bool _isMoving = false;
	private bool _lockedByPuzzle = false;
	private bool _keyUsed = false;
	private Vector3 _closedPosition;

	// ── ISaveableObject ───────────────────────────────────────
	public string SaveId => saveId;

	[System.Serializable]
	private class DoorState
	{
		public bool isLocked;
		public bool isOpen;
		public bool keyUsed;
		public bool lockedByPuzzle;
	}

	public string SaveState()
	{
		return JsonUtility.ToJson(new DoorState
		{
			isLocked = isLocked,
			isOpen = _isOpen,
			keyUsed = _keyUsed,
			lockedByPuzzle = _lockedByPuzzle
		});
	}

	public void LoadState(string json)
	{
		if (string.IsNullOrEmpty(json)) return;
		var state = JsonUtility.FromJson<DoorState>(json);

		isLocked = state.isLocked;
		_keyUsed = state.keyUsed;
		_lockedByPuzzle = state.lockedByPuzzle;

		// ★ 열린 상태였으면 즉시 열기 (연출 없이)
		if (state.isOpen && !_isOpen)
			OpenDoorImmediate();
		else if (!state.isOpen && _isOpen)
			CloseDoorImmediate();
	}

	// ★ 에디터 전용: 컴포넌트 처음 부착 시 자동으로 고유 saveId 설정
	private void Reset()
	{
		saveId = $"door_{gameObject.name}";
	}

	// ── 초기화 ────────────────────────────────────────────────
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
	public void UnlockFreeAccess()
	{
		isLocked = false;
		_keyUsed = true;
		_lockedByPuzzle = false;
		lockAfterOpen = false;

		StopAllCoroutines();
		OpenDoor();

		GameServices.Audio?.PlaySFX("door_unlock");
		Debug.Log($"[Door] {gameObject.name} 자유 접근 해제");
	}

	// ── IInteractable ─────────────────────────────────────────
	public string InteractionPrompt
	{
		get
		{
			if (_lockedByPuzzle) return "[F] 문 (잠김)";
			if (!isLocked) return _isOpen ? "[F] 문 닫기" : "[F] 문 열기";
			if (string.IsNullOrEmpty(requiredKeyId)) return "";
			return $"[F] {requiredKeyName} 사용하기";
		}
	}

	public bool CanInteract(IPlayer player) => true;

	public void Interact(IPlayer player)
	{
		if (_isMoving) return;
		var ui = GameServices.UI;

		if (_lockedByPuzzle)
		{
			ui?.ShowDialogue(speaker, puzzleLockedDialogue);
			GameServices.Audio?.PlaySFX("door_locked");
			return;
		}

		if (!isLocked)
		{
			if (_isOpen) CloseDoor();
			else OpenDoor();
			return;
		}

		if (string.IsNullOrEmpty(requiredKeyId))
		{
			GameServices.Audio?.PlaySFX("door_locked");
		}
		else
		{
			if (player.Inventory.HasItem(requiredKeyId))
				UnlockAndOpen(player);
			else
				ui?.ShowDialogue(speaker, needKeyDialogue);
		}
	}

	// ── IItemUsable ───────────────────────────────────────────
	public bool CanUseItem(string itemId)
	{
		if (itemId != requiredKeyId || !isLocked) return false;

		var player = GameServices.Player;
		if (player == null) return false;

		float dist = Vector3.Distance(player.transform.position, transform.position);
		if (dist > keyUseDistance)
		{
			if (!string.IsNullOrEmpty(tooFarDialogue))
				GameServices.UI?.ShowDialogue(speaker, tooFarDialogue);
			return false;
		}

		Vector3 toDoor = (transform.position - player.transform.position).normalized;
		float dot = Vector3.Dot(player.transform.forward, toDoor);
		if (dot < keyUseFacingDot)
		{
			if (!string.IsNullOrEmpty(tooFarDialogue))
				GameServices.UI?.ShowDialogue(speaker, tooFarDialogue);
			return false;
		}

		return true;
	}

	public void UseItem(string itemId)
	{
		var ui = GameServices.UI;
		if (CanUseItem(itemId))
		{
			var player = GameServices.Player;
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
		GameServices.UI?.ShowDialogue(speaker, openDialogue);
		GameServices.Audio?.PlaySFX("door_unlock");
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

	// ★ 복원용 — 애니메이션 없이 즉시 열기/닫기
	private void OpenDoorImmediate()
	{
		_isOpen = true;
		_isMoving = false;

		var col = GetComponent<Collider>();
		if (col != null) col.isTrigger = true;

		if (doorAnimator != null)
			doorAnimator.SetTrigger("Open");
		else
			transform.position = _closedPosition + openOffset;
	}

	private void CloseDoorImmediate()
	{
		_isOpen = false;
		_isMoving = false;

		var col = GetComponent<Collider>();
		if (col != null) col.isTrigger = false;

		if (doorAnimator != null)
			doorAnimator.SetTrigger("Close");
		else
			transform.position = _closedPosition;
	}

	// ── AutoCloseAndLock ──────────────────────────────────────
	private IEnumerator AutoCloseAndLock()
	{
		yield return new WaitForSeconds(autoCloseDelay);
		if (!lockAfterOpen) yield break; // ★ UnlockFreeAccess 호출됐으면 중단

		CloseDoor();
		yield return new WaitForSeconds(openDuration);
		if (!lockAfterOpen) yield break; // ★ 닫히는 동안 해제됐으면 중단

		if (!_keyUsed) isLocked = true;
		_lockedByPuzzle = true;
		Debug.Log("[Door] 자동 잠김");
	}

	// ── SlideDoor ─────────────────────────────────────────────
	private IEnumerator SlideDoor(bool opening)
	{
		Vector3 start = opening ? _closedPosition : _closedPosition + openOffset;
		Vector3 end = opening ? _closedPosition + openOffset : _closedPosition;

		float elapsed = 0f;
		while (elapsed < openDuration)
		{
			elapsed += Time.deltaTime;
			transform.position = Vector3.Lerp(start, end,
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

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position + openOffset, transform.localScale);
		Gizmos.DrawLine(transform.position, transform.position + openOffset);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, keyUseDistance);
	}
}