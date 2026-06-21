using UnityEngine;

/// <summary>
/// 씬 시작 시 스테이지 씬에 반드시 존재해야 하는 Manager가
/// 실제로 있는지 검증합니다.
///
/// [기존 문제]
/// UILayerManager.Instance가 null인 씬에서 Player.cs가 조용히
/// fallback 로직으로 우회하도록 되어 있어, "이 씬 세팅이 잘못됐다"는
/// 사실 자체를 아무도 알아차리지 못하는 구조였습니다.
///
/// [동작]
/// 이 스크립트는 어떤 동작도 대신 수행하지 않습니다. 누락된 매니저를
/// Console에 에러로 표시만 합니다 — fallback 동작 자체는 각 스크립트
/// (Player.cs 등)에 그대로 남겨둡니다. 즉 "게임이 멈추지는 않지만,
/// 개발자는 반드시 알아야 한다"는 원칙입니다.
///
/// [씬 배치]
/// 각 스테이지 씬(Stage1~5)에 빈 GameObject로 하나만 배치하세요.
/// 메인메뉴, 엔딩 씬에는 배치하지 않습니다(필수 매니저 구성이 다름).
/// </summary>
public class SceneRequirementChecker : MonoBehaviour
{
	private void Start()
	{
		bool ok = true;

		ok &= Require<UILayerManager>();
		ok &= Require<UIManager>();
		ok &= Require<AudioManager>();
		ok &= Require<SaveSystem>();
		ok &= Require<SaveLoader>();
		ok &= Require<StageInfo>();

		if (ok)
			Debug.Log($"[SceneRequirementChecker] '{gameObject.scene.name}' — 필수 매니저 구성 정상.");
	}

	private bool Require<T>() where T : Object
	{
		var found = FindAnyObjectByType<T>();
		if (found == null)
		{
			Debug.LogError($"[SceneRequirementChecker] 씬 '{gameObject.scene.name}'에 {typeof(T).Name}이 없습니다! " +
							"씬 구성 기준을 확인하세요.");
			return false;
		}
		return true;
	}
}
