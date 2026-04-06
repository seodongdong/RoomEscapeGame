using UnityEngine;

// 상호작용 가능한 오브젝트 인터페이스

public interface IInteractable
{
    string InteractionPrompt { get;}   // F키를 눌러 상호작용
    void Interact(IPlayer player);
    bool CanInteract(IPlayer player);
}
