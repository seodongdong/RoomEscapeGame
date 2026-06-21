using UnityEngine;

/// <summary>
/// 씬에 하나씩 존재하는 "Core 서비스"만 캐싱해서 제공하는 단일 접근점.
///
/// [포함 대상 - Core 4종만]
/// - UIManager   : 상호작용 프롬프트 / 대사 / 타이머
/// - AudioManager: BGM / SFX / 발소리
/// - Player      : 현재 씬의 플레이어
/// - SaveSystem  : 저장/불러오기
///
/// [포함하지 않는 것]
/// DiaryUI, InventoryUI_Complete, ItemViewer3D, ChaseSequence, Stage1TVGate 등
/// 개별 UI 패널이나 스테이지 전용 오브젝트는 여기 넣지 않습니다.
/// 이런 것들은 필요한 스크립트가 Inspector 슬롯이나 자신의 영역 안에서
/// 직접 참조해야 합니다. GameServices가 "전역 만능 검색창"이 되면
/// 기존 문제(서비스 로케이터 과다 사용)를 이름만 바꿔 재현하게 됩니다.
///
/// [동작 방식]
/// 씬이 바뀔 때마다 캐시가 비어있을 수 있으므로, 각 프로퍼티는
/// "캐시에 있으면 반환, 없으면 한 번만 찾아서 캐싱 후 반환" 방식입니다.
/// 즉 FindAnyObjectByType 호출 자체를 없애는 게 아니라,
/// "매 프레임/매 상호작용마다 반복 호출"하던 것을 "씬당 최초 1회"로 줄입니다.
///
/// [씬 전환 시]
/// OnSceneLoaded에서 캐시를 비웁니다. 새 씬의 UIManager 등을
/// 이전 씬의 (이미 파괴된) 인스턴스로 잘못 가리키는 일이 없도록 합니다.
///
/// [씬 배치]
/// 이 클래스는 MonoBehaviour가 아닌 순수 정적 클래스입니다.
/// 씬에 따로 배치할 GameObject가 필요 없습니다.
/// </summary>
public static class GameServices
{
	private static UIManager _ui;
	private static AudioManager _audio;
	private static Player _player;
	private static SaveSystem _saveSystem;

	private static bool _sceneLoadedHookRegistered = false;

	/// <summary>UIManager — 없으면 경고 로그 후 null 반환 (호출부에서 ?. 사용 권장)</summary>
	public static UIManager UI
	{
		get
		{
			EnsureSceneHook();
			if (_ui == null)
				_ui = Object.FindAnyObjectByType<UIManager>();
			return _ui;
		}
	}

	public static AudioManager Audio
	{
		get
		{
			EnsureSceneHook();
			if (_audio == null)
				_audio = Object.FindAnyObjectByType<AudioManager>();
			return _audio;
		}
	}

	public static Player Player
	{
		get
		{
			EnsureSceneHook();
			if (_player == null)
				_player = Object.FindAnyObjectByType<Player>();
			return _player;
		}
	}

	public static SaveSystem Save
	{
		get
		{
			EnsureSceneHook();
			if (_saveSystem == null)
				_saveSystem = Object.FindAnyObjectByType<SaveSystem>();
			return _saveSystem;
		}
	}

	/// <summary>
	/// 씬 전환 콜백을 1회만 등록합니다.
	/// (정적 클래스라 Awake가 없으므로, 첫 접근 시점에 늦은 등록을 합니다.)
	/// </summary>
	private static void EnsureSceneHook()
	{
		if (_sceneLoadedHookRegistered) return;
		_sceneLoadedHookRegistered = true;
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => ClearCache();
	}

	/// <summary>씬 전환 시 캐시를 비웁니다. 다음 접근 시 새 씬에서 다시 찾습니다.</summary>
	private static void ClearCache()
	{
		_ui = null;
		_audio = null;
		_player = null;
		_saveSystem = null;
	}
}
