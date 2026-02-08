using UnityEngine;

/// <summary>
/// 체력 시스템 인터페이스
/// 데미지, 힐링, 사망 이벤트 관리
/// </summary>
public interface IHealth
{
	int CurrentHealth { get; }
	int MaxHealth { get; }
	bool IsDead { get; }

	void TakeDamage(int amount);
	void Heal(int amount);

	event System.Action OnDeath;
	event System.Action<int> OnHealthChanged;
}