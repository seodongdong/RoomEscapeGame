using UnityEngine;

// 적/크리처 인터페이스

public interface IEnemy
{
    void Chase(Transform target);
    void StopChasing();
    void AttackTarget(IPlayer target);
    bool IsChasing { get;}
}
