using UnityEngine;
/// <summary>
/// 상호작용 가능한 모든 오브젝트의 인터페이스
/// F키로 상호작용할 수 있는 단서, 문, 퍼즐 등에 사용
/// </summary>
public interface IInteractable
{
	string InteractionPrompt { get; }

	void Interact(IPlayer player);
	bool CanInteract(IPlayer player);
}