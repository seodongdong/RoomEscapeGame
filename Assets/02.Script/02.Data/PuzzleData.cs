using UnityEngine;

/// <summary>
/// ÆÛÁñ µ¥ÀÌÅÍ (ScriptableObject)
/// ÆÛÁñ ¸ÞÅ¸µ¥ÀÌÅÍ °ü¸®
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
	public string rewardItemId; // ÆÛÁñ ÇØ°á ½Ã È¹µæ ¾ÆÀÌÅÛ (¿¹: ¿­¼è)

#if UNITY_EDITOR
	[ContextMenu("Set Default Values")]
	private void SetDefaultValues()
	{
		hint = "ÆÛÁñÀ» Ç®¾îº¸¼¼¿ä.";
		successMessage = "ÆÛÁñÀ» ÇØ°áÇß½À´Ï´Ù!";
		Debug.Log("±âº»°ª ¼³Á¤ ¿Ï·á!");
	}
#endif
}