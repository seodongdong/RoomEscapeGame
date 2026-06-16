using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// UI 레이어 스택 관리자
///
/// [기획서]
/// - ESC: 최상위 열린 UI부터 하나씩 닫기
/// - 모든 UI 닫힌 상태 + ESC → 일시정지
/// - E: 열린 UI가 있을 때만 닫기
///
/// [씬 배치]
/// 각 씬(Stage1, Stage2 등)에 빈 오브젝트로 배치.
/// DontDestroyOnLoad 하지 않음 → 씬 전환 시 자동 초기화.
/// pausePanel에 일시정지 UI 패널 연결 (없어도 동작).
/// </summary>
public class UILayerManager : MonoBehaviour
{
	private static UILayerManager _instance;
	public static UILayerManager Instance => _instance;

	[Header("일시정지 UI")]
	[Tooltip("ESC → 일시정지 시 활성화할 패널. 없어도 됨.")]
	[SerializeField] private GameObject pausePanel;

	private readonly Stack<(MonoBehaviour owner, System.Action onClose)> _stack
		= new Stack<(MonoBehaviour, System.Action)>();

	private bool _isPaused = false;

	private void Awake()
	{
		if (_instance != null && _instance != this) { Destroy(gameObject); return; }
		_instance = this;
	}

	private void OnDestroy()
	{
		if (_instance == this) { _instance = null; _stack.Clear(); }
	}

	// ── 스택 관리 ─────────────────────────────────────────────

	/// <summary>UI 열릴 때 등록. onClose = ESC/E 눌렸을 때 실행할 닫기 콜백</summary>
	public void Push(MonoBehaviour owner, System.Action onClose)
	{
		_stack.Push((owner, onClose));
		Debug.Log($"[UILayer] Push: {owner.GetType().Name} (depth={_stack.Count})");
	}

	/// <summary>UI가 버튼 등으로 직접 닫힐 때 스택에서 제거</summary>
	public void Pop(MonoBehaviour owner)
	{
		if (_stack.Count == 0) return;

		if (_stack.Peek().owner == owner)
		{
			_stack.Pop();
			Debug.Log($"[UILayer] Pop: {owner.GetType().Name} (depth={_stack.Count})");
		}
		else
		{
			// 중간 레이어가 닫힌 경우 — 스택 정리
			var temp = new Stack<(MonoBehaviour, System.Action)>();
			while (_stack.Count > 0)
			{
				var item = _stack.Pop();
				if (item.owner == owner) break;
				temp.Push(item);
			}
			while (temp.Count > 0) _stack.Push(temp.Pop());
		}
	}

	// ── ESC 처리 ──────────────────────────────────────────────

	/// <summary>Player.cs에서 ESC 눌릴 때 호출</summary>
	public void HandleEsc()
	{
		if (_stack.Count > 0)
		{
			var top = _stack.Pop();
			top.onClose?.Invoke();
			Debug.Log($"[UILayer] ESC → 닫기: {top.owner.GetType().Name}");
			return;
		}

		// 열린 UI 없음 → 일시정지 토글
		TogglePause();
	}

	public bool HasOpenUI => _stack.Count > 0;
	public bool IsPaused => _isPaused;

	// ── 일시정지 ─────────────────────────────────────────────

	private void TogglePause()
	{
		if (_isPaused) ResumeGame();
		else PauseGame();
	}

	private void PauseGame()
	{
		_isPaused = true;
		if (pausePanel != null) pausePanel.SetActive(true);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		GameManager.Instance?.StateManager.ChangeState(GameState.Paused);
		Debug.Log("[UILayer] 일시정지");
	}

	public void ResumeGame()
	{
		_isPaused = false;
		if (pausePanel != null) pausePanel.SetActive(false);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		GameManager.Instance?.StateManager.ChangeState(GameState.Playing);
		Debug.Log("[UILayer] 재개");
	}
}