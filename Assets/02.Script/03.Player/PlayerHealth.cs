using UnityEngine;

/// <summary>
/// 체력 시스템
/// </summary>
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
		_currentHealth = Mathf.Max(0, _currentHealth - amount);
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