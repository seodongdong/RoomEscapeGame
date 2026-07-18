using UnityEngine;
using UnityEngine.SceneManagement; // ★ 추가
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
	[System.Serializable]
	public class SlotRow
	{
		public TextMeshProUGUI infoText;
		public Button saveButton;
		public Button loadButton;
	}

	[Header("팝업 패널")]
	[SerializeField] private GameObject slotSelectPanel;
	[SerializeField] private Button closeButton;

	[Header("슬롯 4개")]
	[SerializeField] private SlotRow[] slotRows = new SlotRow[SaveSystem.SLOT_COUNT];

	[Header("진입 버튼 (PausePanel)")]
	[SerializeField] private Button openPanelButton;

	[Header("연결")]
	[SerializeField] private SaveSystem saveSystem;

	private void Awake()
	{
		if (saveSystem == null)
			saveSystem = FindAnyObjectByType<SaveSystem>();

		if (saveSystem == null)
			Debug.LogError("[SaveSlotUI] SaveSystem을 찾을 수 없습니다!");

		openPanelButton?.onClick.AddListener(OpenPanel);
		closeButton?.onClick.AddListener(ClosePanel);

		for (int i = 0; i < slotRows.Length; i++)
		{
			int slotIndex = i;
			var row = slotRows[i];
			if (row == null) continue;

			row.saveButton?.onClick.AddListener(() => OnSaveClicked(slotIndex));
			row.loadButton?.onClick.AddListener(() => OnLoadClicked(slotIndex));
		}

		slotSelectPanel?.SetActive(false);
	}

	public void OpenPanel()
	{
		slotSelectPanel?.SetActive(true);
		RefreshAllSlots();
	}

	public void ClosePanel()
	{
		slotSelectPanel?.SetActive(false);
	}

	private void OnSaveClicked(int slotIndex)
	{
		GameData data = BuildCurrentGameData();
		saveSystem.SaveGame(slotIndex, data);
		RefreshSlot(slotIndex);
		Debug.Log($"[SaveSlotUI] 슬롯 {slotIndex} 저장 완료 (씬: {data.sceneName})");
	}

	private void OnLoadClicked(int slotIndex)
	{
		if (!saveSystem.HasSaveFile(slotIndex))
		{
			Debug.Log($"[SaveSlotUI] 슬롯 {slotIndex}에 저장 데이터 없음 — 불러오기 무시");
			return;
		}

		GameData data = saveSystem.LoadGame(slotIndex);
		if (data == null) return;

		if (string.IsNullOrEmpty(data.sceneName))
		{
			Debug.LogError($"[SaveSlotUI] 슬롯 {slotIndex}의 저장 데이터에 씬 이름이 없습니다. 이전 버전 저장 데이터일 수 있습니다.");
			return;
		}

		saveSystem.DeleteChaseAutosave();

		GameManager.Instance?.SetPlayTime(data.playTimeSeconds);

		// ★ 추가: 씬 로드 전에 복원할 데이터를 등록 (SaveLoader가 새 씬에서 꺼내 적용)
		GameManager.Instance?.SetPendingLoadData(data);

		ClosePanel();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		GameManager.Instance?.ChangeState(GameState.Playing);

		GameManager.Instance.StageManager.LoadSceneByName(data.sceneName, data.currentStage);

		Debug.Log($"[SaveSlotUI] 슬롯 {slotIndex} 불러오기 완료 — 씬 '{data.sceneName}' 로드");
	}

	private void RefreshAllSlots()
	{
		for (int i = 0; i < slotRows.Length; i++)
			RefreshSlot(i);
	}

	private void RefreshSlot(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= slotRows.Length) return;
		var row = slotRows[slotIndex];
		if (row == null) return;

		var info = saveSystem.GetSlotInfo(slotIndex);

		if (row.infoText != null)
		{
			row.infoText.text = info.hasSave
				? $"{info.stageName} · {FormatPlayTime(info.playTimeSeconds)}"
				: "비어있음";
		}

		if (row.loadButton != null)
			row.loadButton.interactable = info.hasSave;
	}

	private string FormatPlayTime(float seconds)
	{
		int totalMinutes = Mathf.FloorToInt(seconds / 60f);
		int hours = totalMinutes / 60;
		int minutes = totalMinutes % 60;

		if (hours > 0)
			return $"{hours}시간 {minutes}분";
		return $"{minutes}분";
	}

	/// <summary>
	/// ★ 수정: 현재 씬의 정확한 이름 + StageInfo의 표시 이름(있다면)을 함께 기록합니다.
	/// </summary>
	private GameData BuildCurrentGameData()
	{
		var data = new GameData();

		data.sceneName = SceneManager.GetActiveScene().name;

		var stageInfo = StageInfo.FindInCurrentScene();
		if (stageInfo != null)
			data.savedDisplayName = stageInfo.DisplayName;

		if (GameManager.Instance != null)
		{
			data.currentStage = (stageInfo != null)
				? stageInfo.StageNumber
				: GameManager.Instance.StageManager.CurrentStage;
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
			}

			// ★ InventoryUI에서 표시 정보도 함께 저장
			var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>(FindObjectsInactive.Include);
			if (inventoryUI != null)
			{
				foreach (var item in inventoryUI.GetAllItems())
				{
					data.AddInventoryItem(
						item.itemId,
						item.title,
						item.description ?? "",
						item.itemType == ItemType.Document ? "Document" : "UsableItem",
						item.date ?? ""
					);
				}
			}
		}

		var flashlight = FindAnyObjectByType<Flashlight>();
		if (flashlight != null)
			data.hasFlashlight = flashlight.HasFlashlight;

		return data;
	}
}
	