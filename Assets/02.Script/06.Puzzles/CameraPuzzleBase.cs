using UnityEngine;
using System.Collections;

/// <summary>
/// 카메라 전환 기반 퍼즐의 베이스 클래스.
/// 퍼즐 진입 시 카메라를 지정된 위치로 이동시키고,
/// 완료/나가기 시 원래 위치로 복귀합니다.
///
/// [수정 사항]
/// puzzleUI?.SetActive() → if (puzzleUI != null) puzzleUI.SetActive()
/// Unity의 가짜 null 오브젝트는 ?. 연산자로 안전하게 처리되지 않아
/// UnassignedReferenceException이 발생합니다.
/// puzzleUI를 Inspector에서 비워둬도 에러가 나지 않습니다.
/// </summary>
public abstract class CameraPuzzleBase : MonoBehaviour, IPuzzle
{
	[Header("Puzzle Settings")]
	[SerializeField] protected string puzzleId;
	[SerializeField] protected bool isSolved;

	[Header("Camera Settings")]
	[SerializeField] protected Transform puzzleCameraPosition;
	[SerializeField] protected float cameraTransitionDuration = 1f;
	[SerializeField] protected AnimationCurve cameraTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[Header("UI (선택 — 월드 스페이스 퍼즐은 비워두세요)")]
	[SerializeField] protected GameObject puzzleUI;

	protected Camera _mainCamera;
	protected Transform _originalCameraParent;
	protected Vector3 _originalCameraPosition;
	protected Quaternion _originalCameraRotation;
	protected Player _player;

	public string PuzzleId => puzzleId;
	public bool IsSolved => isSolved;
	public event System.Action OnPuzzleSolved;

	protected virtual void Awake()
	{
		_mainCamera = Camera.main;
		_player = FindAnyObjectByType<Player>();
	}

	// ── 퍼즐 시작 ────────────────────────────────────────────

	public virtual void StartPuzzle()
	{
		if (isSolved) return;

		GameManager.Instance.StateManager.ChangeState(GameState.Puzzle);

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.HideInteractionPrompt();

		if (_player != null)
		{
			_player.enabled = false;
			foreach (var mesh in _player.GetComponentsInChildren<MeshRenderer>())
				mesh.enabled = false;
			foreach (var mesh in _player.GetComponentsInChildren<SkinnedMeshRenderer>())
				mesh.enabled = false;
		}

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		_originalCameraParent = _mainCamera.transform.parent;
		_originalCameraPosition = _mainCamera.transform.position;
		_originalCameraRotation = _mainCamera.transform.rotation;
		_mainCamera.transform.SetParent(null);

		StartCoroutine(TransitionCamera(true));
	}

	// ── 카메라 전환 ──────────────────────────────────────────

	protected virtual IEnumerator TransitionCamera(bool toPuzzle)
	{
		Vector3 startPos = _mainCamera.transform.position;
		Quaternion startRot = _mainCamera.transform.rotation;
		Vector3 endPos;
		Quaternion endRot;

		if (toPuzzle)
		{
			endPos = puzzleCameraPosition.position;
			endRot = puzzleCameraPosition.rotation;
		}
		else
		{
			endPos = _originalCameraPosition;
			endRot = _originalCameraRotation;
		}

		float elapsed = 0f;
		while (elapsed < cameraTransitionDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = cameraTransitionCurve.Evaluate(elapsed / cameraTransitionDuration);
			_mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
			_mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
			yield return null;
		}

		_mainCamera.transform.position = endPos;
		_mainCamera.transform.rotation = endRot;

		if (toPuzzle)
		{
			// ★ 수정: ?. 대신 명시적 null 체크 (Unity 가짜 null 대응)
			if (puzzleUI != null) puzzleUI.SetActive(true);
			OnPuzzleStarted();
		}
		else
		{
			_mainCamera.transform.SetParent(_originalCameraParent);
			OnPuzzleExited();
		}
	}

	protected virtual void OnPuzzleStarted()
	{
		Debug.Log($"[Puzzle] 시작: {puzzleId}");
	}

	protected virtual void OnPuzzleExited()
	{
		Debug.Log($"[Puzzle] 종료: {puzzleId}");
	}

	// ── 정답 체크 ────────────────────────────────────────────

	public virtual void CheckSolution()
	{
		if (IsSolutionCorrect())
			SolvePuzzle();
	}

	protected abstract bool IsSolutionCorrect();

	protected virtual void SolvePuzzle()
	{
		isSolved = true;
		// ★ 수정: 명시적 null 체크
		if (puzzleUI != null) puzzleUI.SetActive(false);
		OnPuzzleSolved?.Invoke();
		Debug.Log($"[Puzzle] 해결: {puzzleId}");
		ExitPuzzle();
	}

	// ── 퍼즐 나가기 ──────────────────────────────────────────

	public virtual void ExitPuzzle()
	{
		// ★ 수정: 명시적 null 체크
		if (puzzleUI != null) puzzleUI.SetActive(false);
		StartCoroutine(ExitPuzzleCoroutine());
	}

	protected virtual IEnumerator ExitPuzzleCoroutine()
	{
		yield return StartCoroutine(TransitionCamera(false));

		GameManager.Instance.StateManager.ChangeState(GameState.Playing);

		if (_player != null)
		{
			_player.enabled = true;
			foreach (var mesh in _player.GetComponentsInChildren<MeshRenderer>())
				mesh.enabled = true;
			foreach (var mesh in _player.GetComponentsInChildren<SkinnedMeshRenderer>())
				mesh.enabled = true;
		}

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}