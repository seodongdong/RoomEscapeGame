using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 퍼즐 화면 상단의 사탕 5개 버튼 컴포넌트
///
/// 각 사탕 버튼에 이 스크립트를 붙이고, Inspector에서 색상과 비주얼을 설정합니다.
/// 선택 시 테두리/색조 변화로 선택 상태를 표시합니다.
/// </summary>
[RequireComponent(typeof(Button))]
public class CandyButton : MonoBehaviour
{
	[Header("사탕 색상")]
	[SerializeField] private Color candyColor = Color.red;

	[Header("선택 상태 비주얼")]
	[SerializeField] private Image candyImage;              // 사탕 이미지
	[SerializeField] private GameObject selectedOutline;    // 선택됐을 때 표시할 테두리 오브젝트

	[Header("선택 색조")]
	[SerializeField] private Color selectedTint = new Color(1f, 1f, 0.5f, 1f);   // 선택 시 노란빛
	[SerializeField] private Color normalTint = Color.white;

	private Button _button;
	private Stage2_AltarCandyPuzzle _puzzle;
	private bool _isSelected = false;

	public Color CandyColor => candyColor;

	public void Initialize(Stage2_AltarCandyPuzzle puzzle)
	{
		_puzzle = puzzle;
		_button = GetComponent<Button>();

		// 버튼 클릭 리스너 등록
		_button.onClick.RemoveAllListeners();
		_button.onClick.AddListener(OnClicked);

		// 사탕 이미지에 색상 적용
		if (candyImage != null)
			candyImage.color = candyColor;

		SetSelected(false);
	}

	private void OnClicked()
	{
		if (_puzzle == null) return;
		_puzzle.SelectCandy(candyColor);
	}

	/// <summary>
	/// 선택 상태 시각 업데이트 (Stage2_AltarCandyPuzzle에서 호출)
	/// </summary>
	public void SetSelected(bool selected)
	{
		_isSelected = selected;

		// 테두리 오브젝트 활성/비활성
		if (selectedOutline != null)
			selectedOutline.SetActive(selected);

		// 이미지 색조 변경
		if (candyImage != null)
			candyImage.color = selected ? selectedTint * candyColor : candyColor;
	}
}