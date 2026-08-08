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
/// [v2 변경사항]
/// 1. scatterRadius(원형 반경) → scatterRange(축별 ±범위)
///    Y축을 포함한 세 축을 따로 조절할 수 있어, 조각이 인형의 집 모델
///    안으로 파고드는 문제를 잡을 수 있습니다.
/// 2. spawnOffset 추가 — spawnPoint를 옮기지 않고 앞/위로 밀어낼 수 있습니다.
/// 3. PuzzleDraggableItem / Collider가 없으면 런타임에 자동 추가 + 경고.
///    세팅이 덜 돼도 프랍이 사라진 채 멈추는 상황을 막습니다.
///
/// [씬 설정]
/// 1. 거실 프랍(장난감 의자/장롱/인형 등)에 이 스크립트 + Collider 부착
/// 2. itemId를 대응하는 PuzzleDropZone.requiredItemId와 똑같이 맞추기
/// 3. spawnPrefab: PuzzleDraggableItem + Collider가 붙은 프리팹
///    (인형의 집처럼 세워진 퍼즐이면 Drag Mode를 VerticalFacingCamera로)
/// 4. spawnPoint: 인형의 집 프랍 앞 바닥에 배치한 빈 오브젝트
/// 5. puzzle: 씬의 Stage1_DollHousePuzzle
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
	[Tooltip("인형의 집 프랍 앞 바닥. 이 지점을 기준으로 생성됩니다.")]
	[SerializeField] private Transform spawnPoint;

	[Tooltip("spawnPoint 기준 고정 오프셋. 조각이 모델 안으로 파고들면 " +
			 "여기서 앞(-Z)이나 위(+Y)로 빼주세요.")]
	[SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0f, -0.4f);

	[Tooltip("축별 랜덤 범위(±). Y를 0으로 두면 바닥에 딱 붙습니다.")]
	[SerializeField] private Vector3 scatterRange = new Vector3(0.35f, 0f, 0.2f);

	[Tooltip("체크하면 spawnPoint가 바라보는 방향 기준으로 계산합니다. " +
			 "해제하면 월드 축 기준입니다.")]
	[SerializeField] private bool useSpawnPointAxes = true;

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
		SpawnDraggable();
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

		SpawnDraggable();

		Debug.Log($"[DollHousePickup] {itemName} 수집 → 인형의 집 앞으로 이동");
	}

	// ── 내부 ──────────────────────────────────────────────────

	private void SpawnDraggable()
	{
		Vector3 pos = GetScatterPosition();
		Quaternion rot = randomizeRotation
			? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
			: transform.rotation;

		GameObject spawned;

		if (spawnPrefab != null)
		{
			spawned = Instantiate(spawnPrefab, pos, rot);
			spawned.name = $"PuzzleItem_{itemId}";
			gameObject.SetActive(false); // 원본 프랍은 맵에서 사라짐
		}
		else
		{
			// 프리팹이 없으면 자기 자신을 옮겨 사용
			spawned = gameObject;
			transform.SetPositionAndRotation(pos, rot);
		}

		// ── 컴포넌트 안전망 ──
		var draggable = spawned.GetComponent<PuzzleDraggableItem>()
						?? spawned.GetComponentInChildren<PuzzleDraggableItem>();

		if (draggable == null)
		{
			Debug.LogWarning($"[DollHousePickup] {itemName}: PuzzleDraggableItem이 없어 " +
							 "자동 추가했습니다. 프리팹에 미리 붙여두세요.", spawned);
			draggable = spawned.AddComponent<PuzzleDraggableItem>();
		}

		if (spawned.GetComponentInChildren<Collider>() == null)
		{
			Debug.LogWarning($"[DollHousePickup] {itemName}: Collider가 없어 BoxCollider를 " +
							 "자동 추가했습니다. 크기가 안 맞으면 프리팹에서 직접 설정하세요.", spawned);
			spawned.AddComponent<BoxCollider>();
		}

		draggable.itemId = itemId;
		draggable.SetHomePositionToCurrent(); // 오답 시 여기로 돌아옵니다
		draggable.DisableDragging();          // 퍼즐에 들어가야 드래그 가능

		if (puzzle != null) puzzle.RegisterPickedItem(draggable);
		else Debug.LogWarning($"[DollHousePickup] {itemName}: puzzle 슬롯이 비어 있습니다.", this);
	}

	private Vector3 GetScatterPosition()
	{
		Transform origin = spawnPoint != null ? spawnPoint : transform;

		Vector3 offset = spawnOffset + new Vector3(
			Random.Range(-scatterRange.x, scatterRange.x),
			Random.Range(-scatterRange.y, scatterRange.y),
			Random.Range(-scatterRange.z, scatterRange.z));

		// TransformPoint 대신 rotation만 적용 — spawnPoint 스케일에 영향받지 않게
		return useSpawnPointAxes
			? origin.position + origin.rotation * offset
			: origin.position + offset;
	}

	// ── 기즈모 ────────────────────────────────────────────────

	private void OnDrawGizmosSelected()
	{
		Transform origin = spawnPoint != null ? spawnPoint : transform;

		Vector3 center = useSpawnPointAxes
			? origin.position + origin.rotation * spawnOffset
			: origin.position + spawnOffset;

		Gizmos.color = Color.magenta;
		Gizmos.matrix = Matrix4x4.TRS(
			center,
			useSpawnPointAxes ? origin.rotation : Quaternion.identity,
			Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, scatterRange * 2f);

		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.DrawLine(transform.position, center);
	}
}