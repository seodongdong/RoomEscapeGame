using UnityEngine;
using System.Collections;

// 플레이어의 체력을 관리하는 클래스

public class PlayerHealth : IHealth
{
    private int _currentHealth;
    private int _maxHealth;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _currentHealth <= 0;

    public event System.Action OnDeath;
    public event System.Action<int> OnHealthChanged;

    public PlayerHealth(int maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }   

    public void TakeDamage(int amount)
    {
        _currentHealth = Mathf.Max(_currentHealth - amount);
        OnHealthChanged?.Invoke(_currentHealth);

        if (IsDead)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth);
    }
}
