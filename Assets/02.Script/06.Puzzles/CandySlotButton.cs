using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 퍼즐 화면의 방석(사진) 슬롯 버튼
///
/// 16개의 방석 사진 각각에 이 컴포넌트를 붙입니다.
/// 버튼 클릭 시 Stage2_AltarCandyPuzzle.OnSlotClicked(slotIndex)를 호출합니다.
///
/// [Inspector 설정 방법]
/// - slotIndex: 0~15 (방석 번호, 왼쪽 위에서 오른쪽 아래 순)
/// - puzzle: Stage2_AltarCandyPuzzle 컴포넌트 연결
/// </summary>
[RequireComponent(typeof(Button))]
public class CandySlotButton : MonoBehaviour
{
	[Header("슬롯 설정")]
	[SerializeField] private int slotIndex;   // 0~15
	[SerializeField] private Stage2_AltarCandyPuzzle puzzle;

	private Button _button;

	private void Awake()
	{
		_button = GetComponent<Button>();
		_button.onClick.RemoveAllListeners();
		_button.onClick.AddListener(OnClicked);
	}

	private void OnClicked()
	{
		if (puzzle == null)
		{
			Debug.LogError($"[CandySlotButton] puzzle 연결이 필요합니다! (slotIndex: {slotIndex})");
			return;
		}
		puzzle.OnSlotClicked(slotIndex);
	}
}