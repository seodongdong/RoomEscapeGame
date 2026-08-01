using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 2스테이지: 불빛 순서 기억 퍼즐 (기획서 v2 기준 신규 구현)
///
/// [흐름]
/// 1. 맵 입장 → 방석 위에 색깔별 각목이 랜덤 배치됨
/// 2. 각목이 고정된 순서로 하나씩 깜빡임 → 한 바퀴 후 5초 휴식 → 반복
/// 3. 플레이어가 관을 클릭 → 퍼즐 화면 진입 (PuzzleTrigger → StartPuzzle)
/// 4. 퍼즐 화면의 각목을 기억한 순서대로 클릭
/// 5. 모든 각목 클릭 완료 시점에 일괄 판정
///    정답 → 촛불 점화 + 액자 표정 어두워짐 + 자동으로 퍼즐 화면 나가짐 → 다음 단계
///    오답 → 화면 진동 + 붉은 플래시 (3초 이내) → 현재 단계 입력만 초기화 (순서 유지)
/// 6. 3단계 클리어 → 불빛 완전 정지, 퍼즐 종료
///
/// [리셋 규칙 — 기획서 표 그대로]
///   오답 판정      : 현재 단계만 리셋, 불빛 순서 유지
///   향로 A 클릭    : 현재 단계만 리셋, 불빛 순서 새로 랜덤 생성
///   향로 B / ESC   : 퍼즐 화면 나가기, 현재 단계 리셋 + 순서 새로 랜덤 생성
///   게임 강제 종료 : 전체 리셋 (1단계부터)
///
/// [기존 Stage2_AltarCandyPuzzle과의 관계]
/// 기획서 v2에서 2스테이지 퍼즐이 "사탕 배치"에서 "불빛 순서 기억"으로
/// 바뀌었기 때문에, 기존 스크립트는 재사용할 수 있는 부분이 없어 새로
/// 작성했습니다. Stage2_AltarCandyPuzzle.cs 파일은 지우지 않았으니,
/// 씬에서 관(棺)의 PuzzleTrigger가 가리키는 대상만 이 스크립트로 바꾸면 됩니다.
///
/// [씬 설정]
/// 1. 빈 오브젝트에 이 스크립트 부착, PuzzleTrigger(관)의 Puzzle 슬롯에 연결
/// 2. cushionPoints : 방석 16개의 Transform (각목이 여기 랜덤 배치됨)
/// 3. mapSticks     : role=Map인 Stage2_LightStick 7개
/// 4. puzzleSticks  : role=PuzzleInput인 Stage2_LightStick 7개 (관 앞 일렬 배치)
/// 5. candles       : Stage2_Candle 3개 (왼쪽/가운데/오른쪽 순서로)
/// 6. portrait      : Stage2_PortraitFrame
/// 7. redFlashOverlay : 화면 전체를 덮는 붉은 Image의 CanvasGroup (alpha 0으로 시작)
/// </summary>
public class Stage2_LightSequencePuzzle : CameraPuzzleBase
{
	[System.Serializable]
	public class StageConfig
	{
		[Tooltip("이 단계에서 사용할 각목 수")]
		public int stickCount = 3;
		[Tooltip("각목 하나가 켜져 있는 시간 (초). 작을수록 빠름.")]
		public float blinkOnTime = 0.7f;
		[Tooltip("각목 사이 간격 (초).")]
		public float blinkGap = 0.35f;
	}

	[Header("단계 설정 (기획서: 3개 느림 / 5개 중간 / 7개 빠름)")]
	[SerializeField]
	private List<StageConfig> stageConfigs = new List<StageConfig>()
	{
		new StageConfig { stickCount = 3, blinkOnTime = 0.80f, blinkGap = 0.45f },
		new StageConfig { stickCount = 5, blinkOnTime = 0.55f, blinkGap = 0.30f },
		new StageConfig { stickCount = 7, blinkOnTime = 0.35f, blinkGap = 0.18f },
	};

	[Header("한 바퀴 후 휴식 시간 (초)")]
	[SerializeField] private float cycleRestTime = 5f;

	[Header("각목 색상 팔레트 (최대 단계 각목 수 이상 필요)")]
	[SerializeField]
	private List<Color> stickColors = new List<Color>()
	{
		new Color(0.90f, 0.25f, 0.25f), // 빨강
		new Color(0.95f, 0.65f, 0.20f), // 주황
		new Color(0.90f, 0.85f, 0.30f), // 노랑
		new Color(0.35f, 0.75f, 0.40f), // 초록
		new Color(0.30f, 0.55f, 0.90f), // 파랑
		new Color(0.60f, 0.40f, 0.85f), // 보라
		new Color(0.95f, 0.95f, 0.95f), // 흰색
	};

	[Header("맵 — 방석 위 각목")]
	[Tooltip("방석 16개의 Transform. 각목이 매 단계 여기 랜덤 배치됩니다.")]
	[SerializeField] private List<Transform> cushionPoints = new List<Transform>();
	[Tooltip("role = Map 인 각목들. 최대 단계 각목 수만큼 필요합니다.")]
	[SerializeField] private List<Stage2_LightStick> mapSticks = new List<Stage2_LightStick>();
	[SerializeField] private float stickHeightOffset = 0.05f;

	[Header("퍼즐 화면 — 일렬 각목")]
	[Tooltip("role = PuzzleInput 인 각목들. 왼쪽부터 순서대로 연결하세요.")]
	[SerializeField] private List<Stage2_LightStick> puzzleSticks = new List<Stage2_LightStick>();

	[Header("촛대 (왼쪽 / 가운데 / 오른쪽 순서로 연결)")]
	[SerializeField] private List<Stage2_Candle> candles = new List<Stage2_Candle>();
	[Tooltip("단계별로 점화할 촛불 인덱스. 기획서: 1단계=왼쪽(0), 2단계=오른쪽(2), 3단계=가운데(1)")]
	[SerializeField] private int[] candleOrder = new int[] { 0, 2, 1 };

	[Header("영정사진 액자")]
	[SerializeField] private Stage2_PortraitFrame portrait;

	[Header("오답 연출 (3초 이내)")]
	[Tooltip("화면 전체를 덮는 붉은 Image의 CanvasGroup. alpha 0으로 시작하세요.")]
	[SerializeField] private CanvasGroup redFlashOverlay;
	[SerializeField] private float shakeDuration = 0.5f;
	[SerializeField] private float shakeMagnitude = 0.08f;
	[SerializeField] private float flashDuration = 0.45f;

	[Header("크리처 연동")]
	[SerializeField] private Stage2_ShadowCreature shadowCreature;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string startDialogue = "아까 본 순서대로 눌러보자.";
	[TextArea(2, 4)][SerializeField] private string stageClearDialogue = "...초에 불이 붙었다.";
	[TextArea(2, 4)][SerializeField] private string wrongDialogue = "...틀렸다.";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "...다음 방으로 갈 수 있을 것 같다.";
	[TextArea(2, 4)][SerializeField] private string exitDialogue = "나중에 다시 와야겠다.";

	[Header("효과음")]
	[SerializeField] private string wrongSFX = "puzzle_wrong";
	[SerializeField] private string stageClearSFX = "puzzle_correct";
	[SerializeField] private string solveSFX = "door_unlock";

	// ── 런타임 상태 ──────────────────────────────────────────
	private int _currentStage = 0;                     // 0 ~ 2
	private List<int> _sequence = new List<int>();     // 정답 순서 (colorId)
	private List<int> _input = new List<int>();        // 플레이어 입력
	private bool _isJudging = false;
	private bool _inPuzzleScreen = false;
	private Coroutine _blinkLoop;

	public int CurrentStageIndex => _currentStage;
	public int TotalStageCount => stageConfigs.Count;

	// ── 초기화 ────────────────────────────────────────────────

	protected override void Start()
	{
		base.Start();

		if (redFlashOverlay != null) redFlashOverlay.alpha = 0f;

		HideAllPuzzleSticks();

		if (!isSolved)
			SetupStage(0, regenerateSequence: true);
	}

	// ── 단계 셋업 ─────────────────────────────────────────────

	/// <summary>
	/// 단계에 맞춰 각목 개수/색을 설정하고 방석 위에 랜덤 배치합니다.
	/// regenerateSequence가 true면 깜빡임 순서도 새로 랜덤 생성합니다.
	/// </summary>
	private void SetupStage(int stageIndex, bool regenerateSequence)
	{
		if (stageIndex >= stageConfigs.Count) return;

		_currentStage = stageIndex;
		_input.Clear();

		var config = stageConfigs[stageIndex];
		int n = Mathf.Min(config.stickCount, mapSticks.Count, puzzleSticks.Count, stickColors.Count);

		// ── 맵 각목: 색 지정 + 방석 랜덤 배치
		List<Transform> availableCushions = new List<Transform>(cushionPoints);

		for (int i = 0; i < mapSticks.Count; i++)
		{
			var stick = mapSticks[i];
			if (stick == null) continue;

			bool used = i < n;
			stick.gameObject.SetActive(used);
			if (!used) continue;

			stick.Setup(this, i, stickColors[i]);
			stick.StopBlinking();

			if (availableCushions.Count > 0)
			{
				int pick = Random.Range(0, availableCushions.Count);
				Transform cushion = availableCushions[pick];
				availableCushions.RemoveAt(pick);

				stick.transform.position = cushion.position + Vector3.up * stickHeightOffset;
				stick.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
			}
		}

		// ── 퍼즐 화면 각목: 색만 지정 (위치는 씬에 고정 배치)
		for (int i = 0; i < puzzleSticks.Count; i++)
		{
			var stick = puzzleSticks[i];
			if (stick == null) continue;

			bool used = i < n;
			stick.gameObject.SetActive(used);
			if (!used) continue;

			stick.Setup(this, i, stickColors[i]);
			stick.SetClickEnabled(false);
		}

		// ── 정답 순서 생성
		if (regenerateSequence || _sequence.Count != n)
			GenerateSequence(n);

		RestartBlinkLoop();

		Debug.Log($"[LightSequence] {stageIndex + 1}단계 셋업 — 각목 {n}개, 순서: {SequenceToString()}");
	}

	/// <summary>0..n-1을 무작위로 섞어 정답 순서를 만듭니다 (각목마다 한 번씩).</summary>
	private void GenerateSequence(int n)
	{
		_sequence.Clear();
		for (int i = 0; i < n; i++) _sequence.Add(i);

		// Fisher-Yates
		for (int i = _sequence.Count - 1; i > 0; i--)
		{
			int j = Random.Range(0, i + 1);
			(_sequence[i], _sequence[j]) = (_sequence[j], _sequence[i]);
		}
	}

	private string SequenceToString()
	{
		var sb = new System.Text.StringBuilder();
		foreach (int id in _sequence) sb.Append(id).Append(' ');
		return sb.ToString().TrimEnd();
	}

	// ── 맵 불빛 깜빡임 루프 ──────────────────────────────────

	private void RestartBlinkLoop()
	{
		if (_blinkLoop != null) StopCoroutine(_blinkLoop);
		_blinkLoop = StartCoroutine(BlinkLoop());
	}

	private IEnumerator BlinkLoop()
	{
		// 시작 직후 잠깐 대기 (셋업이 끝난 뒤 보이도록)
		yield return new WaitForSeconds(1f);

		while (!isSolved)
		{
			var config = stageConfigs[_currentStage];

			foreach (int colorId in _sequence)
			{
				if (isSolved) yield break;

				var stick = FindMapStick(colorId);
				stick?.Blink(config.blinkOnTime);

				yield return new WaitForSeconds(config.blinkOnTime + config.blinkGap);
			}

			yield return new WaitForSeconds(cycleRestTime);
		}
	}

	private Stage2_LightStick FindMapStick(int colorId)
	{
		foreach (var s in mapSticks)
			if (s != null && s.gameObject.activeSelf && s.ColorId == colorId) return s;
		return null;
	}

	private void StopAllLights()
	{
		if (_blinkLoop != null) { StopCoroutine(_blinkLoop); _blinkLoop = null; }
		foreach (var s in mapSticks) s?.StopBlinking();
	}

	// ── 퍼즐 화면 진입 / 이탈 ────────────────────────────────

	protected override void OnPuzzleStarted()
	{
		_inPuzzleScreen = true;
		_input.Clear();
		_isJudging = false;

		SetPuzzleSticksClickable(true);
		GameServices.UI?.ShowDialogue(speaker, startDialogue);
	}

	/// <summary>ESC / 향로 B — 현재 단계 리셋 + 순서 새로 랜덤 생성</summary>
	public override void ExitPuzzle()
	{
		SetPuzzleSticksClickable(false);
		_inPuzzleScreen = false;

		if (!isSolved && !_isJudging)
		{
			_input.Clear();
			SetupStage(_currentStage, regenerateSequence: true);
			GameServices.UI?.ShowDialogue(speaker, exitDialogue);
		}

		base.ExitPuzzle();
	}

	/// <summary>향로 A — 현재 단계만 리셋 + 순서 새로 랜덤 생성 (화면은 유지)</summary>
	public void ResetCurrentStageFromIncense()
	{
		if (isSolved || _isJudging) return;

		_input.Clear();
		SetupStage(_currentStage, regenerateSequence: true);
		SetPuzzleSticksClickable(true);

		Debug.Log("[LightSequence] 향로 A — 현재 단계 리셋 (순서 재생성)");
	}

	private void SetPuzzleSticksClickable(bool clickable)
	{
		foreach (var s in puzzleSticks)
			if (s != null) s.SetClickEnabled(clickable);
	}

	private void HideAllPuzzleSticks()
	{
		foreach (var s in puzzleSticks)
			if (s != null) s.SetClickEnabled(false);
	}

	// ── 입력 ─────────────────────────────────────────────────

	/// <summary>Stage2_LightStick(PuzzleInput)이 클릭될 때 호출됩니다.</summary>
	public void OnStickClicked(Stage2_LightStick stick)
	{
		if (isSolved || _isJudging || !_inPuzzleScreen) return;
		if (stick == null) return;

		_input.Add(stick.ColorId);

		// 입력 도중에는 정오답을 알려주지 않습니다 (기획서)
		if (_input.Count >= _sequence.Count)
			StartCoroutine(JudgeRoutine());
	}

	private IEnumerator JudgeRoutine()
	{
		_isJudging = true;
		SetPuzzleSticksClickable(false);

		yield return new WaitForSecondsRealtime(0.35f);

		bool correct = IsSolutionCorrect();

		if (correct)
		{
			yield return StartCoroutine(StageClearRoutine());
		}
		else
		{
			yield return StartCoroutine(WrongAnswerRoutine());
			_isJudging = false;
		}
	}

	// ── 정답 판정 ────────────────────────────────────────────

	protected override bool IsSolutionCorrect()
	{
		if (_sequence.Count == 0) return false;
		if (_input.Count != _sequence.Count) return false;

		for (int i = 0; i < _sequence.Count; i++)
			if (_input[i] != _sequence[i]) return false;

		return true;
	}

	// ── 단계 클리어 ──────────────────────────────────────────

	private IEnumerator StageClearRoutine()
	{
		// 촛불 점화 (기획서: 1단계 왼쪽 / 2단계 오른쪽 / 3단계 가운데)
		LightCandleForStage(_currentStage);

		// 액자 표정 단계별 변화
		portrait?.SetExpressionStage(_currentStage + 1);

		GameServices.Audio?.PlaySFX(stageClearSFX);
		GameServices.UI?.ShowDialogue(speaker, stageClearDialogue);

		yield return new WaitForSecondsRealtime(1.5f);

		int next = _currentStage + 1;

		if (next >= stageConfigs.Count)
		{
			// 3단계 클리어 → 퍼즐 종료
			_isJudging = false;
			SolvePuzzle();
			yield break;
		}

		// 다음 단계 준비 후 퍼즐 화면에서 자동으로 나감
		// (기획서: 다음 단계 불빛을 맵에서 다시 확인하기 위함)
		SetupStage(next, regenerateSequence: true);

		_isJudging = false;
		_inPuzzleScreen = false;
		SetPuzzleSticksClickable(false);

		base.ExitPuzzle();
	}

	private void LightCandleForStage(int stageIndex)
	{
		if (candles == null || candles.Count == 0) return;
		if (candleOrder == null || stageIndex >= candleOrder.Length) return;

		int candleIndex = candleOrder[stageIndex];
		if (candleIndex < 0 || candleIndex >= candles.Count) return;

		candles[candleIndex]?.SetLit(true);
	}

	// ── 오답 연출 ────────────────────────────────────────────

	private IEnumerator WrongAnswerRoutine()
	{
		GameServices.Audio?.PlaySFX(wrongSFX);
		GameServices.UI?.ShowDialogue(speaker, wrongDialogue);

		// 화면 진동 + 붉은 플래시 동시 진행 (합쳐서 3초 이내)
		Coroutine flash = StartCoroutine(RedFlash());
		yield return StartCoroutine(CameraShake());
		yield return flash;

		// 현재 단계만 리셋 — 불빛 순서는 유지 (기획서)
		_input.Clear();
		SetPuzzleSticksClickable(true);

		Debug.Log("[LightSequence] 오답 — 입력만 초기화 (순서 유지)");
	}

	private IEnumerator CameraShake()
	{
		if (_mainCamera == null) yield break;

		Transform cam = _mainCamera.transform;
		Vector3 origin = cam.position;

		float elapsed = 0f;
		while (elapsed < shakeDuration)
		{
			elapsed += Time.unscaledDeltaTime;
			float damper = 1f - Mathf.Clamp01(elapsed / shakeDuration);
			cam.position = origin + (Vector3)(Random.insideUnitCircle * shakeMagnitude * damper);
			yield return null;
		}

		cam.position = origin;
	}

	private IEnumerator RedFlash()
	{
		if (redFlashOverlay == null) yield break;

		float half = flashDuration * 0.5f;

		float t = 0f;
		while (t < half)
		{
			t += Time.unscaledDeltaTime;
			redFlashOverlay.alpha = Mathf.Lerp(0f, 0.6f, t / half);
			yield return null;
		}

		t = 0f;
		while (t < half)
		{
			t += Time.unscaledDeltaTime;
			redFlashOverlay.alpha = Mathf.Lerp(0.6f, 0f, t / half);
			yield return null;
		}

		redFlashOverlay.alpha = 0f;
	}

	// ── 퍼즐 완료 ────────────────────────────────────────────

	protected override void SolvePuzzle()
	{
		// 3단계 클리어 → 불빛 완전 정지
		StopAllLights();
		foreach (var s in mapSticks) if (s != null) s.gameObject.SetActive(false);

		SetPuzzleSticksClickable(false);
		portrait?.SetExpressionStage(3);

		shadowCreature?.MoveToFinalPosition();

		GameServices.Audio?.PlaySFX(solveSFX);
		GameServices.UI?.ShowDialogue(speaker, solveDialogue);

		base.SolvePuzzle();
	}

	// ── 저장 복원 ────────────────────────────────────────────

	protected override void OnLoadStateSolved()
	{
		StopAllLights();
		foreach (var s in mapSticks) if (s != null) s.gameObject.SetActive(false);
		foreach (var s in puzzleSticks) if (s != null) s.gameObject.SetActive(false);

		foreach (var c in candles) c?.SetLit(true, playSFX: false);
		portrait?.SetExpressionStage(3);

		shadowCreature?.MoveToFinalPosition();

		Debug.Log("[LightSequence] 저장 복원 — 퍼즐 완료 상태");
	}

	// ── 기즈모 ────────────────────────────────────────────────

	private void OnDrawGizmosSelected()
	{
		if (cushionPoints == null) return;
		Gizmos.color = Color.yellow;
		foreach (var p in cushionPoints)
			if (p != null) Gizmos.DrawWireSphere(p.position, 0.15f);
	}
}