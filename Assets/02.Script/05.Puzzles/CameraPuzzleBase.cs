using UnityEngine;
using System.Collections;

/// <summary>
/// 카메라 전환 기반 퍼즐 베이스 클래스
///
/// [수정]
/// - puzzleUI 완전 제거 → 패널 없이 3D 월드에서 직접 퍼즐
/// - 퍼즐 진입 시 UILayerManager.Push → ESC로 나가기 가능
/// - null 안전 체크, 중복 호출 방지 플래그 유지
/// </summary>
public abstract class CameraPuzzleBase : MonoBehaviour, IPuzzle, ISaveableObject
{
	[Header("Puzzle Settings")]
	[SerializeField] protected string puzzleId;
	[SerializeField] protected bool isSolved;

	[Header("Camera Settings")]
	[Tooltip("퍼즐 카메라 위치. 비워두면 현재 위치에서 시작.")]
	[SerializeField] protected Transform puzzleCameraPosition;
	[SerializeField] protected float cameraTransitionDuration = 1f;
	[SerializeField]
	protected AnimationCurve cameraTransitionCurve
		= AnimationCurve.EaseInOut(0, 0, 1, 1);

	[Header("저장 ID (씬 내 유일해야 함)")]
	[SerializeField] private string saveId = "puzzle_001";

	public string SaveId => saveId;

	[System.Serializable]
	private class PuzzleState { public bool isSolved; }

	public virtual string SaveState()
		=> JsonUtility.ToJson(new PuzzleState { isSolved = isSolved });

	public virtual void LoadState(string json)
	{
		if (string.IsNullOrEmpty(json)) return;
		var state = JsonUtility.FromJson<PuzzleState>(json);
		if (state.isSolved) isSolved = true;
	}

	// ── 캐싱 ─────────────────────────────────────────────────
	protected Camera _mainCamera;
	protected Transform _originalCameraParent;
	protected Vector3 _originalCameraPosition;
	protected Quaternion _originalCameraRotation;
	protected Player _player;

	// ── 상태 플래그 ───────────────────────────────────────────
	private bool _isTransitioning = false;
	private bool _isExiting = false;

	// ── IPuzzle ───────────────────────────────────────────────
	public string PuzzleId => puzzleId;
	public bool IsSolved => isSolved;
	public event System.Action OnPuzzleSolved;

	// ── 초기화 ────────────────────────────────────────────────
	protected virtual void Awake()
	{
		_mainCamera = Camera.main;
		_player = GameServices.Player;
	}

	protected virtual void Start()
	{
		if (_mainCamera == null) _mainCamera = Camera.main;
		if (_player == null) _player = GameServices.Player;
	}

	// ── 퍼즐 시작 ────────────────────────────────────────────
	public virtual void StartPuzzle()
	{
		if (isSolved) return;
		if (_isTransitioning) return;

		if (_mainCamera == null)
		{
			_mainCamera = Camera.main;
			if (_mainCamera == null)
			{
				Debug.LogError($"[Puzzle:{puzzleId}] Camera.main을 찾을 수 없습니다!");
				return;
			}
		}

		_isExiting = false;
		_isTransitioning = true;

		GameManager.Instance?.ChangeState(GameState.Puzzle);
		GameServices.UI?.HideInteractionPrompt();

		if (_player != null)
		{
			// ★ 수정: _player.enabled = false 제거
			// Player.cs의 Update()가 멈추면 퍼즐 완료 후에도
			// 어떤 키 입력도 받지 못하는 버그가 있었습니다.
			// 이동/상호작용 차단은 Player.cs 내부에서 GameState.Puzzle
			// 체크로 이미 처리되므로, 컴포넌트를 끄지 않습니다.
			SetPlayerMeshVisible(false);
		}

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		UILayerManager.Instance?.Push(this, ExitPuzzle);

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
			if (puzzleCameraPosition != null)
			{
				endPos = puzzleCameraPosition.position;
				endRot = puzzleCameraPosition.rotation;
			}
			else
			{
				endPos = startPos;
				endRot = startRot;
				Debug.LogWarning($"[Puzzle:{puzzleId}] puzzleCameraPosition 없음 — 현재 위치에서 시작");
			}
		}
		else
		{
			endPos = _originalCameraPosition;
			endRot = _originalCameraRotation;
		}

		bool needsTransition = Vector3.Distance(startPos, endPos) > 0.01f
							|| Quaternion.Angle(startRot, endRot) > 0.1f;

		if (needsTransition && cameraTransitionDuration > 0f)
		{
			float elapsed = 0f;
			while (elapsed < cameraTransitionDuration)
			{
				elapsed += Time.unscaledDeltaTime;
				float t = cameraTransitionCurve.Evaluate(
					Mathf.Clamp01(elapsed / cameraTransitionDuration));
				_mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
				_mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
				yield return null;
			}
		}

		_mainCamera.transform.position = endPos;
		_mainCamera.transform.rotation = endRot;
		_isTransitioning = false;

		if (toPuzzle)
			OnPuzzleStarted();
		else
		{
			_mainCamera.transform.SetParent(_originalCameraParent);
			OnPuzzleExited();
		}
	}

	protected virtual void OnPuzzleStarted() { }
	protected virtual void OnPuzzleExited() { }

	// ── 정답 체크 ────────────────────────────────────────────
	public virtual void CheckSolution()
	{
		if (IsSolutionCorrect()) SolvePuzzle();
	}

	protected abstract bool IsSolutionCorrect();

	protected virtual void SolvePuzzle()
	{
		isSolved = true;
		OnPuzzleSolved?.Invoke();
		Debug.Log($"[Puzzle:{puzzleId}] 해결!");

		// 해결됐으므로 UILayerManager 스택에서 제거
		UILayerManager.Instance?.Pop(this);

		ExitPuzzle();
	}

	// ── 퍼즐 나가기 ──────────────────────────────────────────
	public virtual void ExitPuzzle()
	{
		if (_isExiting) return;
		_isExiting = true;

		// ESC로 나갈 때 UILayerManager 스택 정리
		// (SolvePuzzle에서 이미 Pop한 경우 Pop 내부에서 중복 처리됨)
		UILayerManager.Instance?.Pop(this);

		StartCoroutine(ExitPuzzleCoroutine());
	}

	protected virtual IEnumerator ExitPuzzleCoroutine()
	{
		yield return StartCoroutine(TransitionCamera(false));

		GameManager.Instance?.ChangeState(GameState.Playing);

		if (_player != null)
		{
			// ★ 수정: _player.enabled = true 제거 (StartPuzzle과 동일한 이유)
			SetPlayerMeshVisible(true);
		}

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		_isExiting = false;
	}

	// ── 헬퍼 ─────────────────────────────────────────────────
	private void SetPlayerMeshVisible(bool visible)
	{
		if (_player == null) return;
		foreach (var m in _player.GetComponentsInChildren<MeshRenderer>())
			m.enabled = visible;
		foreach (var m in _player.GetComponentsInChildren<SkinnedMeshRenderer>())
			m.enabled = visible;
	}

	protected void InvokeOnPuzzleSolved() => OnPuzzleSolved?.Invoke();
}