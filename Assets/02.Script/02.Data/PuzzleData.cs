using UnityEngine;

/// <summary>
/// 퍼즐 데이터 (ScriptableObject)
/// 퍼즐 메타데이터 관리
/// </summary>
[CreateAssetMenu(fileName = "PuzzleData", menuName = "Game/Puzzle Data")]
public class PuzzleData : ScriptableObject
{
	[Header("Basic Info")]
	public string puzzleId;
	public string puzzleName;
	public int stageNumber;

	[Header("UI")]
	[TextArea(3, 10)]
	public string hint;
	[TextArea(2, 5)]
	public string successMessage;
	public Sprite puzzleIcon;

	[Header("Reward")]
	public string rewardItemId; // 퍼즐 해결 시 획득 아이템 (예: 열쇠)

#if UNITY_EDITOR
	[ContextMenu("Set Default Values")]
	private void SetDefaultValues()
	{
		hint = "퍼즐을 풀어보세요.";
		successMessage = "퍼즐을 해결했습니다!";
		Debug.Log("기본값 설정 완료!");
	}
#endif
}