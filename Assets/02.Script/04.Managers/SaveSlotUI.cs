using UnityEngine;
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
		Debug.Log($"[SaveSlotUI] 슬롯 {slotIndex} 저장 완료");
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

		// ★ 추가: 불러온 시점의 플레이 시간으로 GameManager 복원
		GameManager.Instance?.SetPlayTime(data.playTimeSeconds);

		ClosePanel();
		GameManager.Instance.StageManager.LoadStage(data.currentStage);
		Debug.Log($"[SaveSlotUI] 슬롯 {slotIndex} 불러오기 완료 — {data.currentStage}스테이지 로드");
	}

	private void RefreshAllSlots()
	{
		for (int i = 0; i < slotRows.Length; i++)
			RefreshSlot(i);
	}

	/// <summary>
	/// ★ 수정: "비어있음" 또는 "거실 · 1시간 23분" 형식으로 표시.
	/// </summary>
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

	/// <summary>
	/// ★ 추가: 초 단위 시간을 "1시간 23분" 또는 "45분" 형식의 문자열로 변환.
	/// </summary>
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
	/// ★ 수정: playTimeSeconds를 GameManager에서 가져와 기록.
	/// </summary>
	private GameData BuildCurrentGameData()
	{
		var data = new GameData();

		if (GameManager.Instance != null)
		{
			data.currentStage = GameManager.Instance.StageManager.CurrentStage;
			data.playTimeSeconds = GameManager.Instance.PlayTimeSeconds; // ★ 추가
		}

		var player = FindAnyObjectByType<Player>();
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
		}

		return data;
	}
}