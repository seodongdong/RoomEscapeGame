using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 저장 데이터 복원기.
///
/// [v3 변경사항]
/// 1. ClueTracker 복원 추가 — 배치용 단서가 저장/복원되도록.
///    이전에는 인벤토리 단서만 복원돼서, 인형의 집 조각처럼 인벤토리에
///    안 들어가는 단서가 불러오기 후 전부 사라졌습니다.
/// 2. ClueTracker를 먼저 비우고 덮어씀 — GameManager가 DontDestroyOnLoad라
///    이전 세션의 단서가 남아 있던 문제 해결.
/// 3. ISaveRestorable 매칭에 trackedClues + collectedClues를 합쳐서 사용.
///
/// [복원 순서가 중요합니다]
/// ClueTracker → 인벤토리 → 이미 획득한 단서 오브젝트 → 손전등 → 오브젝트 상태
/// 배치용 단서가 ApplyAlreadyCollected에서 퍼즐 조각을 스폰하고 퍼즐에
/// 등록하기 때문에, 퍼즐의 LoadState(오브젝트 상태)보다 먼저 와야 합니다.
/// </summary>
public class SaveLoader : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(RestoreAfterSceneReady());
	}

	private IEnumerator RestoreAfterSceneReady()
	{
		// 씬의 모든 Awake / Start가 끝난 뒤 복원
		yield return null;

		if (GameManager.Instance == null || !GameManager.Instance.HasPendingLoadData)
		{
			Debug.Log("[SaveLoader] 복원할 데이터 없음 — 평소처럼 진행");
			yield break;
		}

		GameData data = GameManager.Instance.ConsumePendingLoadData();
		if (data == null) yield break;

		RestoreClueTracker(data);
		RestorePlayerPosition(data);
		RestoreInventory(data);
		RestoreAlreadyCollectedClues(data);
		RestoreFlashlight(data);
		RestoreObjectStates(data);

		Debug.Log("[SaveLoader] 복원 완료");
	}

	/// <summary>
	/// ★ 추가: ClueTracker를 저장된 목록으로 교체합니다.
	/// 이전 목록을 비우기 때문에, 진행하다가 앞선 슬롯을 불러와도
	/// 단서 개수가 정확히 되돌아갑니다.
	/// </summary>
	private void RestoreClueTracker(GameData data)
	{
		var tracker = GameManager.Instance?.ClueTracker;
		if (tracker == null) return;

		// 구버전 저장 데이터 호환 — trackedClues가 없으면 인벤토리 목록으로 대체
		var clues = (data.trackedClues != null && data.trackedClues.Count > 0)
			? data.trackedClues
			: data.collectedClues;

		tracker.RestoreClues(clues);
	}

	/// <summary>저장된 단서 전체 목록 (인벤토리 + 배치용)</summary>
	private HashSet<string> BuildAllClueSet(GameData data)
	{
		var set = new HashSet<string>();

		if (data.collectedClues != null)
			foreach (var id in data.collectedClues) set.Add(id);

		if (data.trackedClues != null)
			foreach (var id in data.trackedClues) set.Add(id);

		return set;
	}

	private void RestoreObjectStates(GameData data)
	{
		if (data.savedObjectIds == null || data.savedObjectIds.Count == 0) return;

		var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
			.OfType<ISaveableObject>();

		int restored = 0;
		var missing = new List<string>();

		foreach (var s in saveables)
		{
			if (string.IsNullOrEmpty(s.SaveId))
			{
				Debug.LogWarning($"[SaveLoader] SaveId가 비어 있는 오브젝트: {(s as MonoBehaviour)?.name}",
					s as MonoBehaviour);
				continue;
			}

			string state = data.GetObjectState(s.SaveId);
			if (state != null)
			{
				s.LoadState(state);
				restored++;
			}
			else
			{
				missing.Add(s.SaveId);
			}
		}

		Debug.Log($"[SaveLoader] 오브젝트 상태 복원: {restored}/{data.savedObjectIds.Count}개");

		if (missing.Count > 0)
			Debug.LogWarning($"[SaveLoader] 저장 데이터에 없던 오브젝트: {string.Join(", ", missing)}");
	}

	private void RestorePlayerPosition(GameData data)
	{
		var player = GameServices.Player;
		if (player == null)
		{
			Debug.LogWarning("[SaveLoader] Player를 찾을 수 없어 위치를 복원하지 못했습니다.");
			return;
		}

		var controller = player.GetComponent<CharacterController>();
		if (controller != null) controller.enabled = false;

		player.transform.position = data.playerPosition;

		if (controller != null) controller.enabled = true;
	}

	private void RestoreInventory(GameData data)
	{
		var player = GameServices.Player;
		if (player == null || data.collectedClues == null) return;

		var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>(FindObjectsInactive.Include);

		foreach (var clueId in data.collectedClues)
		{
			if (player.Inventory.HasItem(clueId)) continue;

			var savedItemData = data.GetInventoryItemData(clueId);

			string title = savedItemData?.title ?? clueId;
			string description = savedItemData?.description ?? "";
			string date = savedItemData?.date ?? "";
			ItemType itemType = savedItemData?.itemType ?? ItemType.UsableItem;

			player.Inventory.AddItem(new ClueItem(clueId, title, description));

			inventoryUI?.AddItem(new InventoryItemData
			{
				itemId = clueId,
				title = title,
				description = description,
				itemType = itemType,
				date = date
			});
		}
	}

	/// <summary>
	/// ★ 수정: 배치용 단서까지 포함한 전체 목록으로 매칭합니다.
	/// Stage1_DollHousePickupClue가 여기서 퍼즐 조각을 다시 스폰합니다.
	/// </summary>
	private void RestoreAlreadyCollectedClues(GameData data)
	{
		var allClues = BuildAllClueSet(data);
		if (allClues.Count == 0) return;

		var restorables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
			.OfType<ISaveRestorable>();

		int count = 0;
		foreach (var restorable in restorables)
		{
			if (allClues.Contains(restorable.RestoreItemId))
			{
				restorable.ApplyAlreadyCollected();
				count++;
			}
		}

		Debug.Log($"[SaveLoader] 이미 획득한 단서 오브젝트 복원: {count}개");
	}

	private void RestoreFlashlight(GameData data)
	{
		var flashlight = FindAnyObjectByType<Flashlight>();
		if (flashlight == null) return;

		flashlight.RestoreHasFlashlight(data.hasFlashlight);

		if (data.hasFlashlight)
		{
			var pickup = FindAnyObjectByType<FlashlightPickup>(FindObjectsInactive.Include);
			pickup?.ApplyAlreadyCollected();
		}
	}
}