using UnityEngine;
using System.Collections;

/// <summary>
/// 재료별 마우스 입력 미니게임 시스템.
///
/// [지원하는 입력 타입]
/// - UpDown  : 위아래 드래그 (다지기, 굽기)
/// - Circle  : 원형으로 돌리기 (섞기, 뭉치기)
/// - Shake   : 좌우 빠르게 흔들기 (계란 풀기)
/// - Click   : 클릭 (접시에 담기)
///
/// [제한시간]
/// timeLimit 초 안에 requiredCount를 채우지 못하면
/// OnMinigameFailed 이벤트 발생 → Stage4_ToyFoodPuzzle이 처리.
/// timeLimit = 0이면 제한시간 없음.
///
/// [애니메이션]
/// Trigger 이름: "Chop"(UpDown), "Knead"(Circle), "Crack"(Shake), "Place"(Click)
/// </summary>
public class Stage4_CookingMinigame : MonoBehaviour
{
	public enum InputType
	{
		UpDown,
		Circle,
		Shake,
		Click,
	}

	[Header("진행 표시 UI (선택)")]
	[SerializeField] private UnityEngine.UI.Slider progressSlider;
	[SerializeField] private TMPro.TextMeshProUGUI instructionText;

	[Header("제한시간 UI (선택)")]
	[Tooltip("남은 시간을 표시할 Slider. 없어도 동작.")]
	[SerializeField] private UnityEngine.UI.Slider timerSlider;
	[SerializeField] private TMPro.TextMeshProUGUI timerText;

	[Header("입력 판정 설정")]
	[SerializeField] private int requiredCount = 3;
	[SerializeField] private float timeLimit = 5f;
	[SerializeField] private float minDragDistance = 60f;
	[SerializeField] private float minShakeSpeed = 300f;
	[SerializeField] private float circleThresholdDegrees = 270f;

	// ── 이벤트 ───────────────────────────────────────────────
	public event System.Action OnMinigameComplete;
	public event System.Action OnMinigameFailed;

	// ── 런타임 ───────────────────────────────────────────────
	private InputType _currentType;
	private bool _isActive = false;
	private int _count = 0;
	private float _elapsed = 0f;
	private float _currentTimeLimit = 0f;
	private Animator _animator;

	// UpDown 판정용
	private Vector2 _dragStart;
	private bool _draggingUp = false;
	private bool _halfDone = false;

	// Shake 판정용
	private float _lastMouseX;
	private int _shakeDirection = 0;

	// Circle 판정용
	private Vector2 _circlePrev;
	private float _totalAngle = 0f;

	// ── 초기화 ───────────────────────────────────────────────

	private void Awake()
	{
		_animator = GetComponentInChildren<Animator>();
		gameObject.SetActive(false);
	}

	/// <summary>
	/// 미니게임 시작.
	/// </summary>
	/// <param name="limit">제한시간(초). 0이면 무제한.</param>
	public void StartMinigame(InputType type, string instruction, int count = 3, float limit = 5f)
	{
		_currentType = type;
		_isActive = true;
		_count = 0;
		_elapsed = 0f;
		_totalAngle = 0f;
		_halfDone = false;
		_shakeDirection = 0;
		requiredCount = count;
		_currentTimeLimit = limit;

		gameObject.SetActive(true);

		if (instructionText != null)
			instructionText.text = instruction;

		if (progressSlider != null)
		{
			progressSlider.minValue = 0;
			progressSlider.maxValue = requiredCount;
			progressSlider.value = 0;
		}

		if (timerSlider != null)
		{
			timerSlider.minValue = 0;
			timerSlider.maxValue = limit > 0 ? limit : 1;
			timerSlider.value = limit > 0 ? limit : 1;
			timerSlider.gameObject.SetActive(limit > 0);
		}

		if (timerText != null)
			timerText.gameObject.SetActive(limit > 0);

		_lastMouseX = Input.mousePosition.x;
		_circlePrev = Input.mousePosition;
		_dragStart = Input.mousePosition;

		Debug.Log($"[CookingMinigame] 시작: {type} / {count}회 / 제한시간: {(limit > 0 ? limit + "초" : "없음")}");
	}

	public void StopMinigame()
	{
		_isActive = false;
		gameObject.SetActive(false);
	}

	// ── 매 프레임 ─────────────────────────────────────────────

	private void Update()
	{
		if (!_isActive) return;

		// 제한시간 체크
		if (_currentTimeLimit > 0f)
		{
			_elapsed += Time.deltaTime;
			float remaining = Mathf.Max(0f, _currentTimeLimit - _elapsed);

			if (timerSlider != null)
				timerSlider.value = remaining;

			if (timerText != null)
				timerText.text = remaining.ToString("F1");

			if (_elapsed >= _currentTimeLimit)
			{
				_isActive = false;
				StartCoroutine(FailDelay());
				return;
			}
		}

		// 입력 처리
		switch (_currentType)
		{
			case InputType.UpDown: HandleUpDown(); break;
			case InputType.Circle: HandleCircle(); break;
			case InputType.Shake: HandleShake(); break;
			case InputType.Click: HandleClick(); break;
		}
	}

	// ── UpDown ────────────────────────────────────────────────

	private void HandleUpDown()
	{
		if (Input.GetMouseButtonDown(0))
		{
			_dragStart = Input.mousePosition;
			_halfDone = false;
		}

		if (!Input.GetMouseButton(0)) return;

		Vector2 delta = (Vector2)Input.mousePosition - _dragStart;

		if (!_halfDone && delta.y < -minDragDistance)
		{
			_halfDone = true;
			_draggingUp = false;
		}
		else if (_halfDone && !_draggingUp && delta.y > minDragDistance * 0.5f)
		{
			_dragStart = Input.mousePosition;
			_halfDone = false;
			RegisterProgress("Chop");
		}
		else if (!_halfDone && delta.y > minDragDistance)
		{
			_halfDone = true;
			_draggingUp = true;
		}
		else if (_halfDone && _draggingUp && delta.y < -minDragDistance * 0.5f)
		{
			_dragStart = Input.mousePosition;
			_halfDone = false;
			RegisterProgress("Chop");
		}
	}

	// ── Circle ────────────────────────────────────────────────

	private void HandleCircle()
	{
		if (!Input.GetMouseButton(0))
		{
			_circlePrev = Input.mousePosition;
			return;
		}

		Vector2 current = Input.mousePosition;
		Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
		Vector2 fromCenter = _circlePrev - center;
		Vector2 toCenter = current - center;

		if (fromCenter.magnitude < 20f || toCenter.magnitude < 20f)
		{
			_circlePrev = current;
			return;
		}

		float angle = Vector2.SignedAngle(fromCenter, toCenter);
		_totalAngle += Mathf.Abs(angle);

		if (_totalAngle >= circleThresholdDegrees)
		{
			_totalAngle = 0f;
			RegisterProgress("Knead");
		}

		_circlePrev = current;
	}

	// ── Shake ─────────────────────────────────────────────────

	private void HandleShake()
	{
		float currentX = Input.mousePosition.x;
		float deltaX = currentX - _lastMouseX;
		float shakeSpeed = Mathf.Abs(deltaX) / Time.deltaTime;

		if (shakeSpeed > minShakeSpeed)
		{
			int newDir = deltaX > 0 ? 1 : -1;
			if (newDir != _shakeDirection && _shakeDirection != 0)
				RegisterProgress("Crack");
			_shakeDirection = newDir;
		}

		_lastMouseX = currentX;
	}

	// ── Click ─────────────────────────────────────────────────

	private void HandleClick()
	{
		if (Input.GetMouseButtonDown(0))
			RegisterProgress("Place");
	}

	// ── 진행도 등록 ───────────────────────────────────────────

	private void RegisterProgress(string animTrigger)
	{
		_count++;
		_animator?.SetTrigger(animTrigger);

		if (progressSlider != null)
			progressSlider.value = _count;

		Debug.Log($"[CookingMinigame] 진행 {_count}/{requiredCount}");

		if (_count >= requiredCount)
		{
			_isActive = false;
			StartCoroutine(CompleteDelay());
		}
	}

	// ── 완료 / 실패 딜레이 ────────────────────────────────────

	private IEnumerator CompleteDelay()
	{
		yield return new WaitForSeconds(0.3f);
		gameObject.SetActive(false);
		OnMinigameComplete?.Invoke();
	}

	private IEnumerator FailDelay()
	{
		// 실패 연출 잠깐 (UI가 있으면 0 표시 확인용)
		yield return new WaitForSeconds(0.2f);
		gameObject.SetActive(false);
		Debug.Log("[CookingMinigame] 시간 초과 — 실패");
		OnMinigameFailed?.Invoke();
	}
}