using UnityEngine;

/// <summary>
/// 각 스테이지 씬에 배치하여, 저장/불러오기 슬롯 UI에 표시할 이름을
/// Inspector에서 직접 지정할 수 있게 합니다.
///
/// [목적]
/// SaveSystem.cs에 씬 이름 → 표시 이름 매핑을 하드코딩하지 않고,
/// 씬 자체에 "이 씬의 표시 이름은 무엇인지"를 들고 있게 하여
/// 씬 이름이 바뀌거나 새 씬이 추가되어도 코드 수정 없이 대응할 수 있습니다.
///
/// [씬 배치]
/// 각 스테이지 씬(Stage1~5)에 빈 오브젝트로 하나씩 배치합니다.
/// GameManager나 StageManager처럼 매 씬에 반드시 존재해야 하는 매니저류와
/// 함께 두는 것을 권장합니다.
/// </summary>
public class StageInfo : MonoBehaviour
{
	[Header("저장 슬롯에 표시될 이름")]
	[Tooltip("예: 거실, 장례식장, 미로, 주방, 지하실")]
	[SerializeField] private string displayName = "";

	[Header("스테이지 번호 (기존 시스템과의 호환용)")]
	[Tooltip("기획서 6장 씬 구성 기준 번호. 1~5.")]
	[SerializeField] private int stageNumber = 1;

	public string DisplayName => displayName;
	public int StageNumber => stageNumber;

	/// <summary>
	/// 현재 씬에 배치된 StageInfo를 찾습니다. 없으면 null을 반환합니다.
	/// </summary>
	public static StageInfo FindInCurrentScene()
	{
		return FindAnyObjectByType<StageInfo>();
	}
}