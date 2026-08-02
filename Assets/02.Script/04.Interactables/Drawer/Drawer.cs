using UnityEngine;
using System.Collections;

/// <summary>
/// 서랍 — Door.cs와 완전히 동일한 방식으로 동작합니다.
///
/// [Door.cs와 같은 점]
/// - Animator에 "Open" / "Close" 트리거를 보냅니다. (문 애니메이션과 동일 규약)
/// - doorAnimator가 비어 있으면 openOffset만큼 코루틴으로 밀고/당깁니다.
/// - isLocked + requiredKeyId로 열쇠 잠금을 처리하고, 한 번 열면 이후 자유롭게 여닫힙니다.
/// - IItemUsable 구현 → 인벤토리에서 열쇠를 직접 사용해도 열립니다.
/// - ISaveableObject 구현 → saveId 기준으로 열림/잠금 상태가 저장·복원됩니다.
///
/// [Door.cs와 다른 점]
/// - InteractableBase를 상속합니다. 1스테이지 TV 우선순위 게이트가 자동 적용됩니다.
///   (거실 서랍을 TV 보기 전에 열어버리는 구멍을 구조적으로 막기 위함)
/// - contentsRoot: 서랍 안 내용물(단서 오브젝트)을 담는 부모.
///   열려 있을 때만 활성화되므로, 닫힌 서랍 속 단서를 F키로 집는 버그가 없습니다.
/// - lockAfterOpen(방에 가두기)은 서랍에 필요 없으므로 없습니다.
///
/// [기획서 프롬프트 규칙 — 그대로 구현]
///   열쇠 없이 잠김   → 프롬프트 미표시 + 철컥 SFX
///   열쇠 있고 잠김   → "[F] OO 사용하기"
///   열쇠 사용 후     → "[F] 서랍 열기" / "[F] 서랍 닫기"
///
/// [씬 설정]
/// 1. 서랍 오브젝트에 이 스크립트 + Collider 부착
/// 2. drawerTransform: 실제로 움직이는 서랍 몸통 (비우면 자기 자신)
/// 3. drawerAnimator: 서랍 애니메이터 (Open / Close 트리거 파라미터 필요)
/// 4. contentsRoot: 서랍 안 단서들을 자식으로 넣은 빈 오브젝트
/// 5. saveId: 씬 내에서 유일한 값으로 설정 (컴포넌트 부착 시 자동 생성됨)
/// </summary>
public class Drawer : InteractableBase, IItemUsable, ISaveableObject
{
	[Header("서랍 이름 (프롬프트에 표시)")]
	[SerializeField] private string drawerName = "서랍";

	[Header("잠금 설정")]
	[SerializeField] private bool isLocked = false;
	[SerializeField] private string requiredKeyId = "";
	[SerializeField] private string requiredKeyName = "열쇠";
	[SerializeField] private bool consumeKey = true;

	[Header("열쇠 사용 범위")]
	[SerializeField] private float keyUseDistance = 3f;
	[SerializeField] private float keyUseFacingDot = 0.3f;

	[Header("애니메이션 (문과 동일 규약: Open / Close 트리거)")]
	[Tooltip("실제로 움직이는 서랍 몸통. 비워두면 이 오브젝트 자신을 움직입니다.")]
	[SerializeField] private Transform drawerTransform;
	[Tooltip("서랍 Animator. Open / Close 트리거 파라미터가 있어야 합니다.")]
	[SerializeField] private Animator drawerAnimator;
	[Tooltip("Animator가 없을 때 사용할 슬라이드 오프셋 (로컬 기준).")]
	[SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, 0.35f);
	[SerializeField] private float openDuration = 0.5f;

	[Header("서랍 내용물")]
	[Tooltip("서랍 안 단서들의 부모 오브젝트. 서랍이 열려 있을 때만 활성화됩니다.")]
	[SerializeField] private GameObject contentsRoot;
	[Tooltip("체크하면 서랍을 닫아도 내용물이 계속 활성 상태로 남습니다.")]
	[SerializeField] private bool keepContentsAfterFirstOpen = false;

	[Header("효과음 (AudioManager SFX ID)")]
	[SerializeField] private string openSFX = "drawer_open";
	[SerializeField] private string closeSFX = "drawer_close";
	[SerializeField] private string lockedSFX = "door_locked";
	[SerializeField] private string unlockSFX = "door_unlock";

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string needKeyDialogue = "잠겨있다. 열쇠가 필요할 것 같다.";
	[TextArea(2, 4)][SerializeField] private string unlockDialogue = "열쇠로 서랍을 열었다.";
	[TextArea(2, 4)][SerializeField] private string openDialogue = "";
	[TextArea(2, 4)][SerializeField] private string emptyDialogue = "...아무것도 없다.";
	[TextArea(2, 4)][SerializeField] private string wrongItemDialogue = "이 아이템은 여기에 사용할 수 없다.";
	[TextArea(2, 4)][SerializeField] private string tooFarDialogue = "";

	[Header("저장 ID (씬 내 유일해야 함)")]
	[SerializeField] private string saveId = "drawer_001";

	// ── 런타임 상태 ──────────────────────────────────────────
	private bool _isOpen = false;
	private bool _isMoving = false;
	private bool _keyUsed = false;
	private bool _hasOpenedOnce = false;
	private Transform _target;
	private Vector3 _closedLocalPosition;

	// ── ISaveableObject ───────────────────────────────────────

	public string SaveId => saveId;

	[System.Serializable]
	private class DrawerState
	{
		public bool isLocked;
		public bool isOpen;
		public bool keyUsed;
		public bool hasOpenedOnce;
	}

	public string SaveState()
	{
		return JsonUtility.ToJson(new DrawerState
		{
			isLocked = isLocked,
			isOpen = _isOpen,
			keyUsed = _keyUsed,
			hasOpenedOnce = _hasOpenedOnce
		});
	}

	public void LoadState(string json)
	{
		if (string.IsNullOrEmpty(json)) return;
		var state = JsonUtility.FromJson<DrawerState>(json);

		isLocked = state.isLocked;
		_keyUsed = state.keyUsed;
		_hasOpenedOnce = state.hasOpenedOnce;

		// 복원은 연출 없이 즉시 반영 (Door.cs의 OpenDoorImmediate와 동일한 취지)
		if (state.isOpen) OpenImmediate();
		else CloseImmediate();
	}

	// 에디터 전용 — 컴포넌트 부착 시 오브젝트 이름으로 고유 saveId 생성
	private void Reset()
	{
		saveId = $"drawer_{gameObject.name}";
	}

	// ── 초기화 ────────────────────────────────────────────────

	private void Awake()
	{
		_target = drawerTransform != null ? drawerTransform : transform;
		_closedLocalPosition = _target.localPosition;

		if (contentsRoot != null)
			contentsRoot.SetActive(false);
	}

	// ── InteractableBase ──────────────────────────────────────

	public override string InteractionPrompt
	{
		get
		{
			if (!isLocked)
				return _isOpen ? $"[F] {drawerName} 닫기" : $"[F] {drawerName} 열기";

			// 열쇠가 지정되지 않은 잠긴 서랍 → 프롬프트 미표시 (기획서)
			if (string.IsNullOrEmpty(requiredKeyId)) return "";

			return $"[F] {requiredKeyName} 사용하기";
		}
	}

	public override bool CanInteract(IPlayer player) => true;

	protected override void OnInteract(IPlayer player)
	{
		if (_isMoving) return;

		var ui = GameServices.UI;

		// ── 잠금 해제된 서랍: 자유롭게 여닫기
		if (!isLocked)
		{
			if (_isOpen) Close();
			else Open();
			return;
		}

		// ── 열쇠가 아예 없는 잠금 (퍼즐/영구 잠금)
		if (string.IsNullOrEmpty(requiredKeyId))
		{
			GameServices.Audio?.PlaySFX(lockedSFX);
			return;
		}

		// ── 열쇠 잠금
		if (player != null && player.Inventory.HasItem(requiredKeyId))
			UnlockAndOpen(player);
		else
		{
			GameServices.Audio?.PlaySFX(lockedSFX);
			ui?.ShowDialogue(speaker, needKeyDialogue);
		}
	}

	// ── IItemUsable (인벤토리에서 열쇠 직접 사용) ─────────────

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

		Vector3 toDrawer = (transform.position - player.transform.position).normalized;
		if (Vector3.Dot(player.transform.forward, toDrawer) < keyUseFacingDot)
		{
			if (!string.IsNullOrEmpty(tooFarDialogue))
				GameServices.UI?.ShowDialogue(speaker, tooFarDialogue);
			return false;
		}

		return true;
	}

	public void UseItem(string itemId)
	{
		if (CanUseItem(itemId))
		{
			var player = GameServices.Player;
			if (player != null) UnlockAndOpen(player);
			else { isLocked = false; _keyUsed = true; Open(); }
		}
		else
		{
			GameServices.UI?.ShowDialogue(speaker, wrongItemDialogue);
		}
	}

	// ── 외부 호출 ─────────────────────────────────────────────

	/// <summary>퍼즐 완료 등으로 서랍 잠금을 풀 때 호출합니다.</summary>
	public void Unlock(bool openNow = false)
	{
		isLocked = false;
		_keyUsed = true;
		GameServices.Audio?.PlaySFX(unlockSFX);
		if (openNow && !_isOpen) Open();
	}

	public bool IsOpen => _isOpen;

	// ── 열기 / 닫기 ───────────────────────────────────────────

	private void UnlockAndOpen(IPlayer player)
	{
		isLocked = false;
		_keyUsed = true;

		if (consumeKey && !string.IsNullOrEmpty(requiredKeyId))
		{
			var key = player.Inventory.GetItem(requiredKeyId);
			if (key != null) player.Inventory.RemoveItem(key);
		}

		GameServices.Audio?.PlaySFX(unlockSFX);
		GameServices.UI?.ShowDialogue(speaker, unlockDialogue);
		Open();
	}

	private void Open()
	{
		_isOpen = true;
		_isMoving = true;

		GameServices.Audio?.PlaySFX(openSFX);
		SetContentsActive(true);

		if (drawerAnimator != null)
		{
			drawerAnimator.ResetTrigger("Close");
			drawerAnimator.SetTrigger("Open");
			StartCoroutine(ClearMovingWhenAnimationEnds("Opening"));
		}
		else
		{
			StartCoroutine(SlideDrawer(true));
		}

		// 첫 개방 대사 — 내용물이 없으면 "아무것도 없다"
		if (!_hasOpenedOnce)
		{
			_hasOpenedOnce = true;
			bool hasContents = contentsRoot != null && contentsRoot.transform.childCount > 0;

			if (hasContents && !string.IsNullOrEmpty(openDialogue))
				GameServices.UI?.ShowDialogue(speaker, openDialogue);
			else if (!hasContents && !string.IsNullOrEmpty(emptyDialogue))
				GameServices.UI?.ShowDialogue(speaker, emptyDialogue);
		}
	}

	private void Close()
	{
		_isOpen = false;
		_isMoving = true;

		GameServices.Audio?.PlaySFX(closeSFX);
		SetContentsActive(false);

		if (drawerAnimator != null)
		{
			drawerAnimator.ResetTrigger("Open");
			drawerAnimator.SetTrigger("Close");
			StartCoroutine(ClearMovingWhenAnimationEnds("Closing"));
		}
		else
		{
			StartCoroutine(SlideDrawer(false));
		}
	}

	// ── 복원용 — 연출 없이 즉시 반영 ──────────────────────────

	private void OpenImmediate()
	{
		_isOpen = true;
		_isMoving = false;
		SetContentsActive(true);

		if (drawerAnimator != null)
		{
			drawerAnimator.ResetTrigger("Close");
			drawerAnimator.SetTrigger("Open");
		}
		else if (_target != null)
		{
			_target.localPosition = _closedLocalPosition + openOffset;
		}
	}

	private void CloseImmediate()
	{
		_isOpen = false;
		_isMoving = false;
		SetContentsActive(false);

		if (drawerAnimator != null)
		{
			drawerAnimator.ResetTrigger("Open");
			drawerAnimator.SetTrigger("Close");
		}
		else if (_target != null)
		{
			_target.localPosition = _closedLocalPosition;
		}
	}

	// ── 내부 유틸 ─────────────────────────────────────────────

	private void SetContentsActive(bool active)
	{
		if (contentsRoot == null) return;
		if (!active && keepContentsAfterFirstOpen && _hasOpenedOnce) return;
		contentsRoot.SetActive(active);
	}

	private IEnumerator ClearMovingWhenAnimationEnds(string stateName)
	{
		// 트리거가 반영되어 해당 상태로 진입할 때까지 대기 (최대 0.5초)
		float wait = 0f;
		while (wait < 0.5f &&
			   !drawerAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
		{
			wait += Time.deltaTime;
			yield return null;
		}

		// 재생이 끝나거나 다음 상태로 넘어갈 때까지 대기
		while (true)
		{
			var info = drawerAnimator.GetCurrentAnimatorStateInfo(0);
			if (!info.IsName(stateName)) break;
			if (info.normalizedTime >= 1f) break;
			yield return null;
		}

		_isMoving = false;
	}

	private IEnumerator SlideDrawer(bool opening)
	{
		Vector3 start = _target.localPosition;
		Vector3 end = opening ? _closedLocalPosition + openOffset : _closedLocalPosition;

		float elapsed = 0f;
		while (elapsed < openDuration)
		{
			elapsed += Time.deltaTime;
			_target.localPosition = Vector3.Lerp(start, end,
				Mathf.SmoothStep(0f, 1f, elapsed / openDuration));
			yield return null;
		}

		_target.localPosition = end;
		_isMoving = false;
	}

	// ── 기즈모 ────────────────────────────────────────────────

	private void OnDrawGizmosSelected()
	{
		Transform t = drawerTransform != null ? drawerTransform : transform;
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(t.position, t.position + t.TransformVector(openOffset));
		Gizmos.DrawWireCube(t.position + t.TransformVector(openOffset), t.localScale * 0.9f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, keyUseDistance);
	}
}