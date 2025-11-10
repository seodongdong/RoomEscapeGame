using UnityEngine;
using System.Collections;
using TMPro;

public class PuzzleTrigger : MonoBehaviour, IInteractable
{
	// 퍼즐 참조
	[SerializeField] private PuzzleBase puzzle;

	// 상호작용 프롬프트
	public string InteractionPrompt => "[F] 퍼즐 시작하기";

	public bool CanInteract(IPlayer player)
	{
		return !puzzle.IsSolved;
	}

	public void Interact(IPlayer player)
	{
		puzzle.StartPuzzle();
	}

	// 플레이어가 트리거 영역에 들어올 때 현재 상호작용 가능한 객체 설정
	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<Player>(out var player))
		{
			player.SetCurrentInteractable(this);
		}
	}

	// 플레이어가 트리거 영역에서 나갈 때 현재 상호작용 가능한 객체 해제
	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent<Player>(out var player))
		{
			player.SetCurrentInteractable(null);
		}
	}
}