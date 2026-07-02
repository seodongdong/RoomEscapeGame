using UnityEngine;

/// <summary>
/// Stage4 전용 — 요리 퍼즐 완성 시 아귀 비명을 트리거합니다.
///
/// [배치]
/// 씬에 빈 오브젝트로 하나 배치하고, foodPuzzle과 ghoul 슬롯을 연결하세요.
///
/// [필요성]
/// Stage4_ToyFoodPuzzle.OnPuzzleSolved는 C# event(델리게이트)라서
/// Unity Button OnClick처럼 Inspector에서 직접 함수를 드래그해서
/// 연결할 수 없습니다. 이 스크립트가 Awake에서 코드로 구독합니다.
/// </summary>
public class Stage4PuzzleToGhoulConnector : MonoBehaviour
{
	[SerializeField] private Stage4_ToyFoodPuzzle foodPuzzle;
	[SerializeField] private Stage4_GhoulCreature ghoul;

	private void Awake()
	{
		if (foodPuzzle == null)
		{
			Debug.LogError("[Stage4Connector] foodPuzzle이 연결되지 않았습니다.");
			return;
		}
		if (ghoul == null)
		{
			Debug.LogError("[Stage4Connector] ghoul이 연결되지 않았습니다.");
			return;
		}

		foodPuzzle.OnPuzzleSolved += () =>
		{
			Debug.Log("[Stage4Connector] 요리 완성 → 아귀 비명 트리거");
			ghoul.TriggerScream();
		};
	}
}