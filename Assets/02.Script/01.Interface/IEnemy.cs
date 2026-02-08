using UnityEngine;

/// <summary>
/// 적(크리처/범인) 인터페이스
/// AI 추격, 정지, 공격 행동 정의
/// </summary>
public interface IEnemy
{
	bool IsChasing { get; }

	void Chase(Transform target);
	void StopChasing();
	void AttackTarget(IPlayer target);
}