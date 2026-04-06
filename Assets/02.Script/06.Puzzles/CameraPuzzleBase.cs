using UnityEngine;
using System.Collections;

public abstract class CameraPuzzleBase : MonoBehaviour, IPuzzle
{
	[Header("Puzzle Settings")]
	[SerializeField] protected string puzzleId;
	[SerializeField] protected bool isSolved;

	[Header("Camera Settings")]
	[SerializeField] protected Transform puzzleCameraPosition;
	[SerializeField] protected float cameraTransitionDuration = 1f;
	[SerializeField] protected AnimationCurve cameraTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[Header("UI")]
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

	public virtual void StartPuzzle()
	{
		if (isSolved) return;

		GameManager.Instance.StateManager.ChangeState(GameState.Puzzle);

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.HideInteractionPrompt();

		if (_player != null)
		{
			_player.enabled = false;

			// ⭐ 플레이어 메쉬 숨기기
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
			puzzleUI?.SetActive(true);
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
		Debug.Log($"퍼즐 시작: {puzzleId}");
	}

	protected virtual void OnPuzzleExited()
	{
		Debug.Log($"퍼즐 종료: {puzzleId}");
	}

	public virtual void CheckSolution()
	{
		if (IsSolutionCorrect())
			SolvePuzzle();
	}

	protected abstract bool IsSolutionCorrect();

	protected virtual void SolvePuzzle()
	{
		isSolved = true;
		puzzleUI?.SetActive(false);
		OnPuzzleSolved?.Invoke();
		Debug.Log($"퍼즐 해결: {puzzleId}");
		ExitPuzzle();
	}

	public virtual void ExitPuzzle()
	{
		puzzleUI?.SetActive(false);
		StartCoroutine(ExitPuzzleCoroutine());
	}

	protected virtual IEnumerator ExitPuzzleCoroutine()
	{
		yield return StartCoroutine(TransitionCamera(false));

		GameManager.Instance.StateManager.ChangeState(GameState.Playing);

		if (_player != null)
		{
			_player.enabled = true;

			// ⭐ 플레이어 메쉬 다시 보이기
			foreach (var mesh in _player.GetComponentsInChildren<MeshRenderer>())
				mesh.enabled = true;
			foreach (var mesh in _player.GetComponentsInChildren<SkinnedMeshRenderer>())
				mesh.enabled = true;
		}

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}