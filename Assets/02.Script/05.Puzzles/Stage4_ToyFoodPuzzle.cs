using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stage4_ToyFoodPuzzle : CameraPuzzleBase, PuzzleDropZone.IDropZonePuzzle
{
	[System.Serializable]
	public class IngredientData
	{
		public string itemId = "carrot";
		public string displayName = "당근";
		public Stage4_CookingMinigame.InputType minigameType
			= Stage4_CookingMinigame.InputType.UpDown;
		[TextArea(1, 2)]
		public string instruction = "위아래로 드래그하세요! (다지기)";
		public int requiredCount = 3;
		public float timeLimit = 5f;
		public PuzzleDraggableItem draggableItem;
		public Animator ingredientAnimator;
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
		public float timeLimit = 7f;
	}

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
	[SerializeField] private PuzzleDropZone cuttingBoard;

	[Header("접시 위치")]
	[SerializeField] private Transform plateTransform;
	[SerializeField] private float plateStackOffset = 0.05f;
	[SerializeField] private float lerpDuration = 0.5f;

	[Header("조리대 Y 좌표")]
	[SerializeField] private float tableSurfaceY = 0.8f;

	[Header("미니게임 컴포넌트")]
	[SerializeField] private Stage4_CookingMinigame minigame;

	[Header("완성 접시")]
	[SerializeField] private GameObject completedDishObject;
	[SerializeField] private Vector3 dishHoldLocalPosition = new Vector3(0.3f, -0.25f, 0.5f);
	[SerializeField] private float dishAttachDuration = 0.5f;
	[SerializeField] private float holdBeforeExitDuration = 0.8f;

	[Header("접시 테이블 위치 (저장 복원용)")]
	[Tooltip("GhoulTable의 tableTop Transform. LoadState에서 접시 위치 복원에 사용.")]
	[SerializeField] private Transform dishTableTop;

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
	private bool _dishPlacedOnTable = false; // ★ 추가: 아귀 테이블에 내려놓은 상태
	public bool IsHoldingDish { get; private set; } = false;

	// 완성 접시 원래 회전값 보존용
	private Quaternion _dishOriginalRotation;

	// ── ISaveableObject override ──────────────────────────────

	[System.Serializable]
	private class FoodPuzzleState
	{
		public bool isSolved;
		public bool isHoldingDish;
		public bool dishPlacedOnTable;
		public int processedCount;
		public int postProcessIndex;
		public bool isPostProcessing;
	}

	public override string SaveState()
	{
		return JsonUtility.ToJson(new FoodPuzzleState
		{
			isSolved = isSolved,
			isHoldingDish = IsHoldingDish,
			dishPlacedOnTable = _dishPlacedOnTable,
			processedCount = _processedCount,
			postProcessIndex = _postProcessIndex,
			isPostProcessing = _isPostProcessing
		});
	}

	public override void LoadState(string json)
	{
		if (string.IsNullOrEmpty(json)) return;
		var state = JsonUtility.FromJson<FoodPuzzleState>(json);

		if (state.isSolved)
		{
			isSolved = true;
			_processedCount = state.processedCount;
			_postProcessIndex = state.postProcessIndex;
			_isPostProcessing = state.isPostProcessing;
		}

		if (completedDishObject == null) return;

		if (state.isHoldingDish)
		{
			// 접시를 들고 있던 상태 복원
			completedDishObject.SetActive(true);
			completedDishObject.transform.rotation = _dishOriginalRotation;

			Camera cam = Camera.main;
			if (cam != null)
			{
				completedDishObject.transform.SetParent(cam.transform);
				completedDishObject.transform.localPosition = dishHoldLocalPosition;
				completedDishObject.transform.rotation = _dishOriginalRotation;
			}
			IsHoldingDish = true;
			Debug.Log("[FoodPuzzle] 접시 들기 상태 복원");
		}
		else if (state.dishPlacedOnTable)
		{
			// 테이블에 내려놓은 상태 복원
			completedDishObject.SetActive(true);
			completedDishObject.transform.SetParent(null);
			completedDishObject.transform.rotation = _dishOriginalRotation;

			if (dishTableTop != null)
				completedDishObject.transform.position = dishTableTop.position + Vector3.up * 0.05f;

			_dishPlacedOnTable = true;
			IsHoldingDish = false;
			Debug.Log("[FoodPuzzle] 접시 테이블 위 상태 복원");
		}
	}

	// ── 외부 참조용 ───────────────────────────────────────────

	public GameObject GetCompletedDishObject() => completedDishObject;

	// ── 초기화 ────────────────────────────────────────────────

	protected override void Awake()
	{
		base.Awake();

		if (cuttingBoard != null)
			cuttingBoard.Initialize(this);

		if (completedDishObject != null)
		{
			_dishOriginalRotation = completedDishObject.transform.rotation;
			completedDishObject.SetActive(false);
		}
	}

	// ── 퍼즐 진입 ────────────────────────────────────────────

	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();

		foreach (var data in ingredientDataList)
			if (data.draggableItem != null && !data.isProcessed)
				data.draggableItem.EnableDragging(Camera.main, tableSurfaceY);

		if (_processedCount > 0 || _isPostProcessing)
			GameServices.UI?.ShowDialogue(speaker, resumeDialogue);

		if (_isPostProcessing)
			StartPostProcessStep(_postProcessIndex);
	}

	// ── IDropZonePuzzle ───────────────────────────────────────

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

		if (data.draggableItem != null)
			StartCoroutine(LerpToPlate(data.draggableItem.gameObject, data.displayName));

		Debug.Log($"[FoodPuzzle] {data.displayName} 손질 완료 ({_processedCount}/{ingredientDataList.Count})");

		if (_processedCount >= ingredientDataList.Count)
			StartCoroutine(WaitAndStartPostProcess());
	}

	// ── 재료 → 접시 이동 ─────────────────────────────────────

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

	// ── 후속 단계 ─────────────────────────────────────────────

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

	// ── 실패 ─────────────────────────────────────────────────

	private void OnMinigameFailed()
	{
		minigame.OnMinigameComplete -= OnIngredientMinigameComplete;
		minigame.OnMinigameComplete -= OnPostProcessStepComplete;
		minigame.OnMinigameFailed -= OnMinigameFailed;

		Debug.Log("[FoodPuzzle] 시간 초과 — 전체 초기화 후 퍼즐 종료");
		GameServices.UI?.ShowDialogue(speaker, failDialogue);
		FullReset();
		base.ExitPuzzle();
	}

	private void FullReset()
	{
		cuttingBoard?.RemoveItem();

		foreach (var data in ingredientDataList)
		{
			data.isProcessed = false;
			if (data.draggableItem != null)
			{
				data.draggableItem.DisableDragging();
				data.draggableItem.ResetToHomePosition();
			}
		}

		_currentIngredient = null;
		_processedCount = 0;
		_platedCount = 0;
		_postProcessIndex = 0;
		_isPostProcessing = false;
		_dishPlacedOnTable = false;

		Debug.Log("[FoodPuzzle] 전체 초기화 완료");
	}

	// ── ESC ───────────────────────────────────────────────────

	public override void ExitPuzzle()
	{
		minigame?.StopMinigame();
		minigame.OnMinigameComplete -= OnIngredientMinigameComplete;
		minigame.OnMinigameComplete -= OnPostProcessStepComplete;
		minigame.OnMinigameFailed -= OnMinigameFailed;

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

		// 카메라 먼저 원위치
		yield return StartCoroutine(ExitPuzzleCoroutine());

		// 카메라 복원 완료 후 접시 등장
		completedDishObject.SetActive(true);
		completedDishObject.transform.rotation = _dishOriginalRotation;

		Camera cam = Camera.main;
		Vector3 startPos = completedDishObject.transform.position;

		float elapsed = 0f;
		while (elapsed < dishAttachDuration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / dishAttachDuration);
			Vector3 targetPos = cam.transform.TransformPoint(dishHoldLocalPosition);
			completedDishObject.transform.position = Vector3.Lerp(startPos, targetPos, t);
			yield return null;
		}

		// 카메라 자식으로 고정
		completedDishObject.transform.SetParent(cam.transform);
		completedDishObject.transform.localPosition = dishHoldLocalPosition;
		completedDishObject.transform.rotation = _dishOriginalRotation;
		IsHoldingDish = true;

		yield return new WaitForSeconds(holdBeforeExitDuration);
	}

	// ── 아귀 테이블에 접시 내려놓기 ──────────────────────────

	public void PlaceDishOnTable(Transform tableTop)
	{
		if (completedDishObject == null) return;
		completedDishObject.transform.SetParent(null);
		completedDishObject.transform.position = tableTop.position + Vector3.up * 0.05f;
		completedDishObject.transform.rotation = _dishOriginalRotation;
		IsHoldingDish = false;
		_dishPlacedOnTable = true; // ★ 테이블에 놓인 상태 기록
		Debug.Log("[FoodPuzzle] 접시를 테이블에 내려놓음");
	}
}