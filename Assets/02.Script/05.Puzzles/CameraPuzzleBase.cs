using UnityEngine;
using System.Collections;

public abstract class CameraPuzzleBase : MonoBehaviour, IPuzzle, ISaveableObject
{
	[Header("Puzzle Settings")]
	[SerializeField] protected string puzzleId;
	[SerializeField] protected bool isSolved;

	[Header("Camera Settings")]
	[SerializeField] protected Transform puzzleCameraPosition;
	[SerializeField] protected float cameraTransitionDuration = 1f;
	[SerializeField] protected AnimationCurve cameraTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

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

		if (state.isSolved && !isSolved)
		{
			isSolved = true;
			// ★ 연결된 문/오브젝트에 완료 상태 전파
			// OnPuzzleSolved 이벤트는 재발동하지 않음 (중복 방지)
			// 파생 클래스에서 OnLoadStateSolved()를 override해서 추가 처리
			OnLoadStateSolved();
		}
	}

	/// <summary>
	/// 저장 데이터 복원 시 퍼즐이 완료 상태였을 때 호출됩니다.
	/// 파생 클래스에서 override해서 추가 처리를 하세요.
	/// </summary>
	protected virtual void OnLoadStateSolved() { }

	// ★ 에디터 전용: 컴포넌트 처음 부착 시 오브젝트 이름 기반으로 saveId 자동 설정
	private void Reset()
	{
		saveId = $"puzzle_{gameObject.name}";
	}

	// ── 캐싱 ─────────────────────────────────────────────────
	protected Camera _mainCamera;
	protected Transform _originalCameraParent;
	protected Vector3 _originalCameraPosition;
	protected Quaternion _originalCameraRotation;
	protected Player _player;

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
			SetPlayerMeshVisible(false);

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
				Debug.LogWarning($"[Puzzle:{puzzleId}] puzzleCameraPosition 없음");
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
		UILayerManager.Instance?.Pop(this);
		ExitPuzzle();
	}

	// ── 퍼즐 나가기 ──────────────────────────────────────────
	public virtual void ExitPuzzle()
	{
		if (_isExiting) return;
		_isExiting = true;
		UILayerManager.Instance?.Pop(this);
		StartCoroutine(ExitPuzzleCoroutine());
	}

	protected virtual IEnumerator ExitPuzzleCoroutine()
	{
		yield return StartCoroutine(TransitionCamera(false));

		GameManager.Instance?.ChangeState(GameState.Playing);

		if (_player != null)
			SetPlayerMeshVisible(true);

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