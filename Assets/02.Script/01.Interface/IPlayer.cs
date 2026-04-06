using UnityEngine;

// 플레이어 기능 인터페이스

public interface IPlayer
{
    IInventory Inventory { get; }       // 플레이어 인벤토리
    IHealth Health { get; }             // 플레이어 체력
    Transform Transform { get; }        // 플레이어 트랜스폼
    void TakeDamage(int damage);        // 플레이어가 데미지를 입음
    void Die();                         // 플레이어 사망 처리
}
