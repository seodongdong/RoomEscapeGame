using UnityEngine;

public interface IPlayer
{
	IInventory Inventory { get; }
	Transform Transform { get; }
	void TakeDamage(int damage); // 즉사용으로 유지
	void Die();
	void SetCurrentInteractable(IInteractable interactable);
}