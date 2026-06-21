using UnityEngine;
using UnityEngine.SceneManagement; // ★ 추가
using System.Collections;

public class GirlRescueTrigger : MonoBehaviour, IInteractable
{
	[Header("References")]
	[SerializeField] private Transform girlTransform;
	[SerializeField] private GameObject boxVisual;
	[SerializeField] private string requiredKeyId = "basement_key";

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string noKeyDialogue = "열쇠가 필요하다.";
	[TextArea(2, 5)]
	[SerializeField] private string rescueDialogue = "...! 이 안에 누군가 있다!";

	[Header("저장 중 프롬프트 (기획서 명시)")]
	[SerializeField] private string savingPromptText = "저장 중...";
	[SerializeField] private float savingPromptDuration = 0.8f;

	private bool _isOpened;

	public string InteractionPrompt => "[F] 제설함 상자 열기";

	public bool CanInteract(IPlayer player) => !_isOpened;

	public void Interact(IPlayer player)
	{
		var uiManager = GameServices.UI;

		if (!player.Inventory.HasItem(requiredKeyId))
		{
			uiManager?.ShowDialogue(speaker, noKeyDialogue);
			return;
		}

		_isOpened = true;

		girlTransform.gameObject.SetActive(true);
		uiManager?.ShowDialogue(speaker, rescueDialogue);

		StartCoroutine(AutosaveAndStartChase(uiManager));
	}

	private IEnumerator AutosaveAndStartChase(UIManager uiManager)
	{
		// 코루틴이 안전하게 시작된 뒤 박스 비주얼을 끕니다.
		boxVisual?.SetActive(false);

		var saveSystem = FindAnyObjectByType<SaveSystem>();
		if (saveSystem != null)
		{
			GameData data = BuildAutosaveData();
			saveSystem.SaveChaseAutosave(data);
			Debug.Log($"[GirlRescueTrigger] 추격전 자동 저장 완료 (씬: {data.sceneName})");
		}
		else
		{
			Debug.LogWarning("[GirlRescueTrigger] SaveSystem을 찾을 수 없어 자동 저장을 건너뜁니다.");
		}

		uiManager?.ShowInteractionPrompt(savingPromptText);
		yield return new WaitForSeconds(savingPromptDuration);
		uiManager?.HideInteractionPrompt();

		var chaseSequence = FindAnyObjectByType<ChaseSequence>();
		chaseSequence?.StartChase();
	}

	/// <summary>
	/// ★ 수정: 현재 씬의 StageInfo 표시 이름도 함께 기록합니다.
	/// </summary>
	private GameData BuildAutosaveData()
	{
		var data = new GameData(); // ← data는 여기서 먼저 선언되어야 합니다

		data.sceneName = SceneManager.GetActiveScene().name;

		var stageInfo = StageInfo.FindInCurrentScene();
		if (stageInfo != null)
			data.savedDisplayName = stageInfo.DisplayName;

		if (GameManager.Instance != null)
		{
			data.currentStage = (stageInfo != null) ? stageInfo.StageNumber : 5;
			data.playTimeSeconds = GameManager.Instance.PlayTimeSeconds;
		}

		var player = GameServices.Player;
		if (player != null)
		{
			data.playerPosition = player.transform.position;

			var inventory = player.Inventory;
			if (inventory != null)
			{
				data.collectedClues.Clear();
				foreach (var item in inventory.GetAllItems())
					data.collectedClues.Add(item.ItemId);

				data.hasCamcorder = inventory.HasItem("camcorder");
			}
		}

		// ★ 손전등 상태 기록 (data 선언 이후에 위치해야 함)
		var flashlight = FindAnyObjectByType<Flashlight>();
		if (flashlight != null)
			data.hasFlashlight = flashlight.HasFlashlight;

		return data;
	}
}