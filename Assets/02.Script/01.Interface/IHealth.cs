using UnityEngine;

// 플레이어의 체력 시스템 인터페이스

public interface IHealth
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsDead { get; }
    void TakeDamage(int damage);
    void Heal(int amount);
    event System.Action OnDeath;                // 체력 0이 되었을 때 발생하는 이벤트
    event System.Action<int> OnHealthChanged;   // 체력이 변경되었을 때 발생하는 이벤트
}
