using UnityEngine;

/// <summary>
/// 1스테이지: 인형의 집 퍼즐용 "배치 단서" 수집 컴포넌트.
///
/// [기획서 — 배치 / 퍼즐용 단서]
/// - 상호작용 시 맵 내에서 사라지고, 지정된 퍼즐 근처 위치로 자동 이동
/// - 인벤토리에 등록되지 않음
/// - "어딘가 사용할 수 있을 것 같다" 류의 대사로 퍼즐 사용 암시
/// - 퍼즐 근처에서 단서 형태 그대로(3D 오브젝트) 생성됨
/// - 퍼즐 진입 시 인형의 집 프랍 앞 바닥에 방향 무관하게 널브러져 있음
///
/// [동작]
/// F키 → 이 오브젝트 비활성화 → spawnPoint 주변에 랜덤 위치/회전으로
/// 드래그 가능한 3D 오브젝트 생성 → 퍼즐에 자동 등록.
///
/// [씬 설정]
/// 1. 거실에 놓인 장난감 프랍(장난감 의자/장롱/인형 등)에 이 스크립트 + Collider 부착
/// 2. spawnPrefab: PuzzleDraggableItem이 붙은 프리팹.
///    (비워두면 이 오브젝트 자신을 spawnPoint로 옮기고, 붙어 있는
///     PuzzleDraggableItem을 그대로 사용합니다.)
/// 3. spawnPoint: 인형의 집 프랍 앞 바닥에 배치한 빈 오브젝트
/// 4. puzzle: 씬의 Stage1_DollHousePuzzle 연결
/// 5. itemId: 대응하는 PuzzleDropZone의 requiredItemId와 반드시 동일하게!
/// </summary>
public class Stage1_DollHousePickupClue : InteractableBase, ISaveRestorable
{
	[Header("단서 식별")]
	[Tooltip("대응하는 PuzzleDropZone의 requiredItemId와 반드시 같아야 합니다.")]
	[SerializeField] private string itemId = "toy_chair";
	[SerializeField] private string itemName = "장난감 의자";

	[Header("퍼즐 연결")]
	[SerializeField] private Stage1_DollHousePuzzle puzzle;

	[Header("생성 위치")]
	[Tooltip("인형의 집 프랍 앞 바닥. 이 지점 주변에 널브러지듯 생성됩니다.")]
	[SerializeField] private Transform spawnPoint;
	[Tooltip("생성 위치 랜덤 반경 (m). 여러 단서가 겹치지 않게 흩어 놓습니다.")]
	[SerializeField] private float scatterRadius = 0.35f;
	[Tooltip("체크하면 Y축 회전을 랜덤하게 줍니다. (기획서: 방향 무관하게 널브러져 있음)")]
	[SerializeField] private bool randomizeRotation = true;

	[Header("생성 오브젝트")]
	[Tooltip("PuzzleDraggableItem이 붙은 프리팹. 비우면 이 오브젝트 자신을 옮깁니다.")]
	[SerializeField] private GameObject spawnPrefab;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string pickupDialogue = "어딘가 사용할 수 있을 것 같다.";

	[Header("효과음")]
	[SerializeField] private string pickupSFX = "item_pickup";

	private bool _collected = false;

	// ── ISaveRestorable ───────────────────────────────────────

	public string RestoreItemId => itemId;

	public void ApplyAlreadyCollected()
	{
		if (_collected) return;
		_collected = true;
		SpawnDraggable(silent: true);
	}

	// ── InteractableBase ──────────────────────────────────────

	public override string InteractionPrompt => _collected ? "" : $"[F] {itemName}";

	public override bool CanInteract(IPlayer player) => !_collected;

	protected override void OnInteract(IPlayer player)
	{
		if (_collected) return;
		_collected = true;

		GameServices.Audio?.PlaySFX(pickupSFX);
		GameServices.UI?.ShowDialogue(speaker, pickupDialogue);

		// 인벤토리에는 등록하지 않고 단서 집계만 (기획서: 배치/퍼즐용 단서)
		ClueRegistrar.RegisterClueOnly(itemId);

		SpawnDraggable(silent: false);

		Debug.Log($"[DollHousePickup] {itemName} 수집 → 인형의 집 앞으로 이동");
	}

	// ── 내부 ──────────────────────────────────────────────────

	private void SpawnDraggable(bool silent)
	{
		Vector3 pos = GetScatterPosition();
		Quaternion rot = randomizeRotation
			? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
			: transform.rotation;

		PuzzleDraggableItem draggable;

		if (spawnPrefab != null)
		{
			GameObject spawned = Instantiate(spawnPrefab, pos, rot);
			spawned.name = $"PuzzleItem_{itemId}";
			draggable = spawned.GetComponent<PuzzleDraggableItem>();

			// 원본 프랍은 맵에서 사라짐
			gameObject.SetActive(false);
		}
		else
		{
			// 프리팹이 없으면 자기 자신을 옮겨 사용
			draggable = GetComponent<PuzzleDraggableItem>();
			transform.SetPositionAndRotation(pos, rot);
		}

		if (draggable == null)
		{
			Debug.LogError($"[DollHousePickup] {itemName}: PuzzleDraggableItem을 찾을 수 없습니다. " +
						   "spawnPrefab에 PuzzleDraggableItem이 붙어 있는지 확인하세요.");
			return;
		}

		draggable.itemId = itemId;
		draggable.DisableDragging(); // 퍼즐에 들어가야 드래그 가능

		if (puzzle != null) puzzle.RegisterPickedItem(draggable);
		else Debug.LogWarning($"[DollHousePickup] {itemName}: puzzle 슬롯이 비어 있습니다.");
	}

	private Vector3 GetScatterPosition()
	{
		Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;
		if (scatterRadius <= 0f) return basePos;

		Vector2 offset = Random.insideUnitCircle * scatterRadius;
		return basePos + new Vector3(offset.x, 0f, offset.y);
	}

	private void OnDrawGizmosSelected()
	{
		if (spawnPoint == null) return;
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(spawnPoint.position, scatterRadius);
		Gizmos.DrawLine(transform.position, spawnPoint.position);
	}
}