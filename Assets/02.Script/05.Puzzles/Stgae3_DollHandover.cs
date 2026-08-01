using UnityEngine;
using System.Collections;

/// <summary>
/// 3스테이지: 완성된 인형을 인형 크리처에게 건네주는 상호작용.
///
/// [기획서]
/// "조립된 인형을 옆의 인형 크리처에게 건네주면
///  → 나무인형 획득 + 출구 열림 (효과음 재생)"
///
/// [동작]
/// - 제작대 퍼즐(Stage3_DollAssemblyTable)이 완료되기 전에는 프롬프트가 뜨지 않습니다.
/// - 완료 후 F키 → 완성 인형이 크리처 손 위치로 이동 → 나무인형 지급
///   → 출구 문 잠금 해제 + 효과음
///
/// [씬 설정]
/// 1. 인형 크리처 오브젝트에 이 스크립트 + Collider 부착
/// 2. assemblyTable: Stage3_DollAssemblyTable 연결
/// 3. assembledDollObject: 제작대의 완성 인형과 같은 오브젝트 연결
/// 4. handPoint: 크리처가 인형을 받아 드는 위치 (빈 오브젝트)
/// 5. exitDoor: 출구의 PuzzleSolveDoor 연결
/// </summary>
public class Stage3_DollHandover : InteractableBase, ISaveRestorable
{
	[Header("연결")]
	[SerializeField] private Stage3_DollAssemblyTable assemblyTable;
	[SerializeField] private GameObject assembledDollObject;
	[Tooltip("크리처가 인형을 받아 드는 위치")]
	[SerializeField] private Transform handPoint;
	[Tooltip("출구 문. 건네준 뒤 자유 출입으로 전환됩니다.")]
	[SerializeField] private PuzzleSolveDoor exitDoor;

	[Header("목각인형 지급")]
	[SerializeField] private string woodenDollId = "wooden_doll_stage3";
	[SerializeField] private string woodenDollName = "나무인형";
	[TextArea(1, 3)]
	[SerializeField] private string woodenDollDescription = "낡은 나무인형이다. 어딘가에 놓을 자리가 있을 것 같다.";
	[SerializeField] private GameObject woodenDollPrefab;

	[Header("연출")]
	[SerializeField] private float handoverDuration = 0.8f;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)][SerializeField] private string notReadyDialogue = "아직 인형이 완성되지 않았다.";
	[TextArea(2, 4)][SerializeField] private string handoverDialogue = "...받아줬다.";
	[TextArea(2, 4)][SerializeField] private string rewardDialogue = "대신 나무인형을 받았다. 출구가 열린 것 같다.";

	[Header("효과음")]
	[SerializeField] private string handoverSFX = "item_pickup";
	[SerializeField] private string doorUnlockSFX = "door_unlock";

	private bool _handedOver = false;
	private bool _isPlaying = false;

	/// <summary>인형을 크리처에게 건네주고 나무인형까지 받았는지 여부.</summary>
	public bool HasHandedOver => _handedOver && !_isPlaying;

	// ── ISaveRestorable ───────────────────────────────────────

	public string RestoreItemId => woodenDollId;

	public void ApplyAlreadyCollected()
	{
		_handedOver = true;

		if (assembledDollObject != null && handPoint != null)
		{
			assembledDollObject.SetActive(true);
			assembledDollObject.transform.SetParent(handPoint);
			assembledDollObject.transform.localPosition = Vector3.zero;
		}

		exitDoor?.UnlockFreeAccess();
	}

	// ── InteractableBase ──────────────────────────────────────

	public override string InteractionPrompt
	{
		get
		{
			if (_handedOver || _isPlaying) return "";
			if (assemblyTable == null || !assemblyTable.IsAssembled) return "";
			return "[F] 인형 건네주기";
		}
	}

	public override bool CanInteract(IPlayer player)
	{
		if (_handedOver || _isPlaying) return false;
		return assemblyTable != null && assemblyTable.IsAssembled;
	}

	protected override void OnInteract(IPlayer player)
	{
		if (_handedOver || _isPlaying) return;

		if (assemblyTable == null || !assemblyTable.IsAssembled)
		{
			GameServices.UI?.ShowDialogue(speaker, notReadyDialogue);
			return;
		}

		StartCoroutine(HandoverRoutine(player));
	}

	// ── 건네주기 시퀀스 ───────────────────────────────────────

	private IEnumerator HandoverRoutine(IPlayer player)
	{
		_isPlaying = true;

		GameServices.Audio?.PlaySFX(handoverSFX);

		// 완성 인형을 크리처 손 위치로 이동
		if (assembledDollObject != null && handPoint != null)
		{
			Vector3 start = assembledDollObject.transform.position;
			Quaternion startRot = assembledDollObject.transform.rotation;

			float elapsed = 0f;
			while (elapsed < handoverDuration)
			{
				elapsed += Time.deltaTime;
				float t = Mathf.SmoothStep(0f, 1f, elapsed / handoverDuration);
				assembledDollObject.transform.position = Vector3.Lerp(start, handPoint.position, t);
				assembledDollObject.transform.rotation = Quaternion.Slerp(startRot, handPoint.rotation, t);
				yield return null;
			}

			assembledDollObject.transform.SetParent(handPoint);
			assembledDollObject.transform.localPosition = Vector3.zero;
		}

		_handedOver = true;
		GameServices.UI?.ShowDialogue(speaker, handoverDialogue);

		yield return new WaitForSeconds(1.2f);

		// 나무인형 지급
		ClueRegistrar.RegisterUsableItem(
			player, woodenDollId, woodenDollName, "", woodenDollDescription, woodenDollPrefab);

		// 출구 개방
		GameServices.Audio?.PlaySFX(doorUnlockSFX);
		exitDoor?.UnlockFreeAccess();

		GameServices.UI?.ShowDialogue(speaker, rewardDialogue);

		Debug.Log("[DollHandover] 인형 건네줌 → 나무인형 지급 + 출구 개방");

		_isPlaying = false;
	}
}