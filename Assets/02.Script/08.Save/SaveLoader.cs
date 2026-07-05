using UnityEngine;
using System.Collections;
using System.Linq;

public class SaveLoader : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(RestoreAfterSceneReady());
	}

	private IEnumerator RestoreAfterSceneReady()
	{
		yield return null;

		if (GameManager.Instance == null || !GameManager.Instance.HasPendingLoadData)
		{
			Debug.Log("[SaveLoader] 복원할 데이터 없음 — 평소처럼 진행");
			yield break;
		}

		GameData data = GameManager.Instance.ConsumePendingLoadData();
		if (data == null) yield break;

		RestorePlayerPosition(data);
		RestoreInventory(data);
		RestoreAlreadyCollectedClues(data);
		RestoreFlashlight(data);
		RestoreObjectStates(data); // ★ 추가

		Debug.Log($"[SaveLoader] 복원 완료");
	}

	/// <summary>씬의 ISaveableObject들 상태 복원</summary>
	private void RestoreObjectStates(GameData data)
	{
		if (data.savedObjectIds == null || data.savedObjectIds.Count == 0) return;

		var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
			.OfType<ISaveableObject>();

		int restored = 0;
		foreach (var s in saveables)
		{
			string state = data.GetObjectState(s.SaveId);
			if (state != null)
			{
				s.LoadState(state);
				restored++;
			}
		}

		Debug.Log($"[SaveLoader] 오브젝트 상태 복원: {restored}개");
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

	/// <summary>
	/// ★ 수정: PlayerInventory(판정용)뿐 아니라 InventoryUI_Complete(화면 표시용)에도
	/// 동일하게 등록해야 인벤토리 창에 실제로 보입니다.
	/// </summary>
	private void RestoreInventory(GameData data)
	{
		var player = GameServices.Player;
		if (player == null || data.collectedClues == null) return;

		var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>(FindObjectsInactive.Include);

		foreach (var clueId in data.collectedClues)
		{
			if (player.Inventory.HasItem(clueId)) continue;

			var clueItem = new ClueItem(clueId, clueId, "");
			player.Inventory.AddItem(clueItem); // 판정용 (기존)

			// ★ 추가: 인벤토리 창에 실제로 보이도록 UI용 데이터도 등록
			inventoryUI?.AddItem(new InventoryItemData
			{
				itemId = clueId,
				title = clueId, // 정확한 표시 이름은 각 단서의 ApplyAlreadyCollected에서 보완 가능
				itemType = ItemType.UsableItem,
				description = ""
			});

			GameManager.Instance?.ClueTracker.RegisterClue(clueId);
		}
	}

	private void RestoreAlreadyCollectedClues(GameData data)
	{
		if (data.collectedClues == null || data.collectedClues.Count == 0) return;

		var restorables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
			.OfType<ISaveRestorable>();

		foreach (var restorable in restorables)
		{
			if (data.collectedClues.Contains(restorable.RestoreItemId))
				restorable.ApplyAlreadyCollected();
		}
	}

	/// <summary>
	/// ★ 추가: 손전등 보유 상태를 복원하고, 보유 중이었다면 손전등 획득 오브젝트도
	/// 다시 나타나지 않도록 비활성화합니다(ISaveRestorable과 동일한 흐름 재사용).
	/// </summary>
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