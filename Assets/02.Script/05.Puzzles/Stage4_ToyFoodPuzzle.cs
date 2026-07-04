using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 4스테이지 요리 퍼즐 — 드래그앤드롭 + 마우스 미니게임
///
/// [흐름]
/// 1. 퍼즐 진입 → 재료 4개 조리대에 놓여있음
/// 2. 플레이어가 재료를 드래그 → 도마(CuttingBoard) 위에 올려놓음
/// 3. 도마에 재료가 올려지면 해당 재료의 미니게임 시작
/// 4. 미니게임 완료 → 재료가 접시로 자동 이동
///    (나중에 애니메이션으로 교체 시 LerpToPlate() 코루틴 → Animator.SetTrigger)
/// 5. 4개 재료 모두 완료 → 섞기→뭉치기→굽기→담기 자동 진행
/// 6. 전체 완료 → 완성 접시 들기 연출 → 퍼즐 종료
///
/// [ESC 동작]
/// - 재료 손질 중 ESC: 현재 재료만 도마에서 원위치, 나머지 진행 상태 유지
/// - 후속 단계(섞기 등) 중 ESC: 후속 단계 인덱스 초기화, 재료 손질 상태 유지
/// - 다시 진입 시 중단 지점부터 이어서 진행
///
/// [제한시간 실패]
/// - 재료 손질 중 실패: 그 재료만 원위치, 퍼즐 화면에서 나가짐
/// - 후속 단계 중 실패: 후속 단계 인덱스 0으로 초기화, 퍼즐 화면에서 나가짐
///
/// [씬 세팅]
/// 1. 재료 오브젝트: PuzzleDraggableItem 부착 + itemId 설정
///    예: 당근 → itemId="carrot"
/// 2. 도마: PuzzleDropZone 부착, requiredItemId 비워두기
/// 3. 접시 위치: 빈 Transform (PlateTransform)
/// 4. 완성 접시 오브젝트: 평소 비활성
/// 5. Stage4_CookingMinigame 오브젝트 연결
/// </summary>
public class Stage4_ToyFoodPuzzle : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	// ── 재료 데이터 ───────────────────────────────────────────

	[System.Serializable]
	public class IngredientData
	{
		[Tooltip("PuzzleDraggableItem의 itemId와 일치해야 함")]
		public string itemId = "carrot";

		[Tooltip("화면에 표시될 재료 이름")]
		public string displayName = "당근";

		[Tooltip("도마에 올렸을 때 실행할 미니게임 타입")]
		public Stage4_CookingMinigame.InputType minigameType
			= Stage4_CookingMinigame.InputType.UpDown;

		[Tooltip("미니게임 안내 텍스트")]
		[TextArea(1, 2)]
		public string instruction = "위아래로 드래그하세요! (다지기)";

		[Tooltip("완료에 필요한 횟수")]
		public int requiredCount = 3;

		[Tooltip("제한시간(초). 0이면 무제한.")]
		public float timeLimit = 5f;

		[Tooltip("씬의 재료 오브젝트 (PuzzleDraggableItem 부착)")]
		public PuzzleDraggableItem draggableItem;

		[Tooltip("손질 완료 연출용 Animator (나중에 애니 추가 시 연결)")]
		public Animator ingredientAnimator;

		[Tooltip("완료 시 Animator에 쏠 Trigger 이름")]
		public string animTrigger = "PlaceToPlate";

		[HideInInspector]
		public bool isProcessed = false;
	}

	[System.Serializable]
	public class PostProcessStep
	{
		public string displayName = "섞기";
		public Stage4_CookingMinigame.InputType minigameType
			= Stage4_CookingMinigame.InputType.Circle;
		[TextArea(1, 2)]
		public string instruction = "원을 그리듯 돌리세요! (섞기)";
		public int requiredCount = 3;

		[Tooltip("제한시간(초). 0이면 무제한.")]
		public float timeLimit = 7f;
	}

	// ── Inspector 슬롯 ────────────────────────────────────────

	[Header("재료 데이터")]
	[SerializeField]
	private List<IngredientData> ingredientDataList = new List<IngredientData>()
	{
		new IngredientData { itemId="carrot",  displayName="당근",    minigameType=Stage4_CookingMinigame.InputType.UpDown, instruction="위아래로 드래그하세요! (당근 다지기)",    requiredCount=3, timeLimit=5f },
		new IngredientData { itemId="pork",    displayName="돼지고기", minigameType=Stage4_CookingMinigame.InputType.UpDown, instruction="위아래로 드래그하세요! (돼지고기 다지기)", requiredCount=3, timeLimit=5f },
		new IngredientData { itemId="egg",     displayName="계란",    minigameType=Stage4_CookingMinigame.InputType.Shake,  instruction="좌우로 빠르게 흔드세요! (계란 풀기)",    requiredCount=4, timeLimit=6f },
		new IngredientData { itemId="onion",   displayName="양파",    minigameType=Stage4_CookingMinigame.InputType.UpDown, instruction="위아래로 드래그하세요! (양파 다지기)",    requiredCount=3, timeLimit=5f },
	};

	[Header("후속 단계 (재료 4개 완료 후 자동 진행)")]
	[SerializeField]
	private List<PostProcessStep> postProcessSteps = new List<PostProcessStep>()
	{
		new PostProcessStep { displayName="섞기",        minigameType=Stage4_CookingMinigame.InputType.Circle, instruction="원을 그리듯 돌리세요! (섞기)",      requiredCount=3, timeLimit=7f },
		new PostProcessStep { displayName="뭉치기",      minigameType=Stage4_CookingMinigame.InputType.Circle, instruction="원을 그리듯 돌리세요! (뭉치기)",    requiredCount=2, timeLimit=6f },
		new PostProcessStep { displayName="굽기",        minigameType=Stage4_CookingMinigame.InputType.UpDown, instruction="위아래로 드래그하세요! (굽기)",     requiredCount=4, timeLimit=7f },
		new PostProcessStep { displayName="접시에 담기", minigameType=Stage4_CookingMinigame.InputType.Click,  instruction="클릭하세요! (접시에 담기)",         requiredCount=1, timeLimit=3f },
	};

	[Header("도마 DropZone")]
	[Tooltip("PuzzleDropZone이 붙은 도마. requiredItemId는 비워두세요.")]
	[SerializeField] private PuzzleDropZone cuttingBoard;

	[Header("접시 위치")]
	[Tooltip("손질 완료된 재료들이 모이는 위치 Transform")]
	[SerializeField] private Transform plateTransform;
	[Tooltip("접시에 재료가 쌓이는 Y 오프셋 간격")]
	[SerializeField] private float plateStackOffset = 0.05f;
	[Tooltip("재료가 접시로 이동하는 시간")]
	[SerializeField] private float lerpDuration = 0.5f;

	[Header("조리대 Y 좌표")]
	[SerializeField] private float tableSurfaceY = 0.8f;

	[Header("미니게임 컴포넌트")]
	[SerializeField] private Stage4_CookingMinigame minigame;

	[Header("완성 접시")]
	[SerializeField] private GameObject completedDishObject;
	[Tooltip("완성 접시가 카메라 기준으로 붙을 로컬 위치")]
	[SerializeField] private Vector3 dishHoldLocalPosition = new Vector3(0.3f, -0.25f, 0.5f);
	[SerializeField] private float dishAttachDuration = 0.5f;
	[SerializeField] private float holdBeforeExitDuration = 0.8f;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string solveDialogue = "맛있어 보인다... 가져다줘야겠다.";
	[TextArea(2, 4)][SerializeField] private string exitDialogue = "나중에 다시 만들자.";
	[TextArea(2, 4)][SerializeField] private string resumeDialogue = "계속 요리하자.";
	[TextArea(2, 4)][SerializeField] private string failDialogue = "...다시 해보자.";

	// ── 런타임 상태 ───────────────────────────────────────────

	private IngredientData _currentIngredient = null;
	private int _processedCount = 0;
	private int _postProcessIndex = 0;
	private bool _isPostProcessing = false;
	private int _platedCount = 0;
	public bool IsHoldingDish { get; private set; } = false;

	// ── 초기화 ────────────────────────────────────────────────

	protected override void Awake()
	{
		base.Awake();

		if (cuttingBoard != null)
			cuttingBoard.Initialize(this);

		if (completedDishObject != null)
			completedDishObject.SetActive(false);

		if (minigame != null)
			minigame.gameObject.SetActive(false);
	}

	// ── 퍼즐 진입 ────────────────────────────────────────────

	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();

		// 손질 안 된 재료만 드래그 활성화
		foreach (var data in ingredientDataList)
			if (data.draggableItem != null && !data.isProcessed)
				data.draggableItem.EnableDragging(Camera.main, tableSurfaceY);

		// 재진입 대사
		if (_processedCount > 0 || _isPostProcessing)
			GameServices.UI?.ShowDialogue(speaker, resumeDialogue);

		// 후속 단계 중이었다면 이어서
		if (_isPostProcessing)
			StartPostProcessStep(_postProcessIndex);
	}

	// ── IDropZonePuzzle — 도마에 재료가 올려졌을 때 ──────────

	public void OnItemPlacedOnZone(PuzzleDropZone zone)
	{
		if (_isPostProcessing) return;

		PuzzleDraggableItem placed = zone.GetPlacedItem();
		if (placed == null) return;

		IngredientData found = ingredientDataList.Find(d => d.itemId == placed.itemId);
		if (found == null)
		{
			Debug.LogWarning($"[FoodPuzzle] 알 수 없는 재료: {placed.itemId}");
			return;
		}

		if (found.isProcessed)
		{
			zone.RemoveItem();
			placed.ResetToHomePosition();
			GameServices.UI?.ShowDialogue(speaker, "이미 손질한 재료야.");
			return;
		}

		_currentIngredient = found;
		placed.DisableDragging();

		minigame.OnMinigameComplete -= OnIngredientMinigameComplete;
		minigame.OnMinigameComplete += OnIngredientMinigameComplete;
		minigame.OnMinigameFailed -= OnMinigameFailed;
		minigame.OnMinigameFailed += OnMinigameFailed;
		minigame.StartMinigame(found.minigameType, found.instruction, found.requiredCount, found.timeLimit);

		Debug.Log($"[FoodPuzzle] {found.displayName} 손질 시작");
	}

	// ── 재료 미니게임 완료 ────────────────────────────────────

	private void OnIngredientMinigameComplete()
	{
		minigame.OnMinigameComplete -= OnIngredientMinigameComplete;
		minigame.OnMinigameFailed -= OnMinigameFailed;

		if (_currentIngredient == null) return;

		var data = _currentIngredient;
		data.isProcessed = true;
		_processedCount++;
		_currentIngredient = null;

		cuttingBoard?.RemoveItem();

		// 재료 → 접시 이동
		// ★ 나중에 애니메이션 교체 시:
		//   data.ingredientAnimator?.SetTrigger(data.animTrigger);
		//   _platedCount++는 애니메이션 이벤트에서 호출
		if (data.draggableItem != null)
			StartCoroutine(LerpToPlate(data.draggableItem.gameObject, data.displayName));

		Debug.Log($"[FoodPuzzle] {data.displayName} 손질 완료 ({_processedCount}/{ingredientDataList.Count})");

		if (_processedCount >= ingredientDataList.Count)
			StartCoroutine(WaitAndStartPostProcess());
	}

	// ── 재료 → 접시 이동 (코드 기반, 나중에 애니로 교체) ────

	private IEnumerator LerpToPlate(GameObject obj, string displayName)
	{
		if (plateTransform == null) { _platedCount++; yield break; }

		Vector3 start = obj.transform.position;
		Vector3 end = plateTransform.position + Vector3.up * (plateStackOffset * _platedCount);

		float elapsed = 0f;
		while (elapsed < lerpDuration)
		{
			elapsed += Time.deltaTime;
			obj.transform.position = Vector3.Lerp(start, end,
				Mathf.SmoothStep(0f, 1f, elapsed / lerpDuration));
			yield return null;
		}

		obj.transform.position = end;
		_platedCount++;

		Debug.Log($"[FoodPuzzle] {displayName} 접시로 이동");
	}

	// ── 후속 단계 시작 ────────────────────────────────────────

	private IEnumerator WaitAndStartPostProcess()
	{
		yield return new WaitForSeconds(lerpDuration + 0.3f);

		_isPostProcessing = true;
		_postProcessIndex = 0;

		foreach (var data in ingredientDataList)
			data.draggableItem?.DisableDragging();

		StartPostProcessStep(0);
	}

	private void StartPostProcessStep(int index)
	{
		if (index >= postProcessSteps.Count)
		{
			StartCoroutine(SolveSequence());
			return;
		}

		var step = postProcessSteps[index];

		minigame.OnMinigameComplete -= OnPostProcessStepComplete;
		minigame.OnMinigameComplete += OnPostProcessStepComplete;
		minigame.OnMinigameFailed -= OnMinigameFailed;
		minigame.OnMinigameFailed += OnMinigameFailed;
		minigame.StartMinigame(step.minigameType, step.instruction, step.requiredCount, step.timeLimit);

		Debug.Log($"[FoodPuzzle] 후속 단계: {step.displayName}");
	}

	private void OnPostProcessStepComplete()
	{
		minigame.OnMinigameComplete -= OnPostProcessStepComplete;
		minigame.OnMinigameFailed -= OnMinigameFailed;

		_postProcessIndex++;
		Debug.Log($"[FoodPuzzle] 후속 단계 완료 ({_postProcessIndex}/{postProcessSteps.Count})");
		StartPostProcessStep(_postProcessIndex);
	}

	// ── 미니게임 실패 (시간 초과) ─────────────────────────────

	private void OnMinigameFailed()
	{
		minigame.OnMinigameComplete -= OnIngredientMinigameComplete;
		minigame.OnMinigameComplete -= OnPostProcessStepComplete;
		minigame.OnMinigameFailed -= OnMinigameFailed;

		Debug.Log("[FoodPuzzle] 시간 초과 — 전체 초기화 후 퍼즐 종료");

		GameServices.UI?.ShowDialogue(speaker, failDialogue);

		// ★ 전체 초기화
		FullReset();

		base.ExitPuzzle();
	}

	/// <summary>
	/// 실패 시 전체 초기화.
	/// 모든 재료를 원래 위치로 되돌리고, 진행 상태를 전부 리셋합니다.
	/// 다시 F키로 진입하면 처음부터 다시 시작합니다.
	/// </summary>
	private void FullReset()
	{
		// 도마 비우기
		cuttingBoard?.RemoveItem();

		// 모든 재료 원위치 + isProcessed 초기화
		foreach (var data in ingredientDataList)
		{
			data.isProcessed = false;
			if (data.draggableItem != null)
			{
				data.draggableItem.DisableDragging();
				data.draggableItem.ResetToHomePosition();
			}
		}

		// 접시 위 재료들 위치도 원위치됨 (ResetToHomePosition이 처리)

		// 진행 상태 전부 초기화
		_currentIngredient = null;
		_processedCount = 0;
		_platedCount = 0;
		_postProcessIndex = 0;
		_isPostProcessing = false;

		Debug.Log("[FoodPuzzle] 전체 초기화 완료 — 다시 처음부터 시작 가능");
	}

	// ── ESC로 나갈 때 (진행 상태 유지) ──────────────────────

	public override void ExitPuzzle()
	{
		minigame?.StopMinigame();
		minigame.OnMinigameComplete -= OnIngredientMinigameComplete;
		minigame.OnMinigameComplete -= OnPostProcessStepComplete;
		minigame.OnMinigameFailed -= OnMinigameFailed;

		// 도마에 올라와있던 재료 원위치 (그 재료 손질은 취소)
		if (_currentIngredient != null && !_currentIngredient.isProcessed)
		{
			cuttingBoard?.RemoveItem();
			_currentIngredient.draggableItem?.ResetToHomePosition();
			_currentIngredient.draggableItem?.EnableDragging(Camera.main, tableSurfaceY);
			_currentIngredient = null;
		}

		if (!IsSolved)
			GameServices.UI?.ShowDialogue(speaker, exitDialogue);

		base.ExitPuzzle();
	}

	// ── 정답 판정 ────────────────────────────────────────────

	protected override bool IsSolutionCorrect() => false;

	// ── 완성 시퀀스 ──────────────────────────────────────────

	private IEnumerator SolveSequence()
	{
		isSolved = true;
		InvokeOnPuzzleSolved();
		UILayerManager.Instance?.Pop(this);

		GameServices.UI?.ShowDialogue(speaker, solveDialogue);
		yield return new WaitForSeconds(0.5f);

		if (completedDishObject == null)
		{
			base.ExitPuzzle();
			yield break;
		}

		// ★ 순서 변경: 카메라를 먼저 원위치로 돌려놓고
		// base.ExitPuzzle() 대신 직접 카메라 복원 코루틴을 기다림
		yield return StartCoroutine(ExitPuzzleCoroutine());

		// ★ 카메라 원위치 완료 후 접시를 플레이어 메인카메라 앞에 붙이기
		completedDishObject.SetActive(true);

		Camera cam = Camera.main;
		Vector3 startPos = completedDishObject.transform.position;
		Quaternion startRot = completedDishObject.transform.rotation;

		float elapsed = 0f;
		while (elapsed < dishAttachDuration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / dishAttachDuration);
			Vector3 targetPos = cam.transform.TransformPoint(dishHoldLocalPosition);
			completedDishObject.transform.position = Vector3.Lerp(startPos, targetPos, t);
			completedDishObject.transform.rotation =
				Quaternion.Slerp(startRot, cam.transform.rotation, t);
			yield return null;
		}

		// 카메라 자식으로 고정
		completedDishObject.transform.SetParent(cam.transform);
		completedDishObject.transform.localPosition = dishHoldLocalPosition;
		completedDishObject.transform.localRotation = Quaternion.identity;
		IsHoldingDish = true;

		yield return new WaitForSeconds(holdBeforeExitDuration);
	}

	// ── 아귀 테이블에 접시 내려놓기 ──────────────────────────

	public void PlaceDishOnTable(Transform tableTop)
	{
		if (completedDishObject == null) return;
		completedDishObject.transform.SetParent(null);
		completedDishObject.transform.position = tableTop.position + Vector3.up * 0.05f;
		completedDishObject.transform.rotation = Quaternion.identity;
		IsHoldingDish = false;
		Debug.Log("[FoodPuzzle] 접시를 테이블에 내려놓음");
	}
}