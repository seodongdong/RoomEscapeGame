using UnityEngine;
using System.Collections;

/// <summary>
/// 환경 단서 3D 뷰어
/// - F키 상호작용 시 오브젝트 확대
/// - 마우스 드래그로 360도 회전
/// - E 또는 ESC로 원위치 복귀
/// </summary>
public class ObjectViewer3D : MonoBehaviour, IInteractable
{
	[Header("Clue Info")]
	[SerializeField] private string clueId;
	[SerializeField] private string clueName;
	[TextArea(2, 5)]
	[SerializeField] private string description;

	[Header("Viewer Settings")]
	[SerializeField] private float zoomDistance = 1.5f;     // 카메라로부터 얼마나 앞에 위치할지
	[SerializeField] private float zoomDuration = 0.5f;     // 확대 애니메이션 시간
	[SerializeField] private float rotateSpeed = 3f;        // 회전 속도
	[SerializeField] private Vector3 viewScale = Vector3.one; // 뷰어에서 오브젝트 크기

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue;               // 처음 볼 때 대사

	[Header("UI")]
	[SerializeField] private GameObject viewerHintUI;       // "드래그하여 회전 / E로 닫기" 안내

	// 원래 상태 저장
	private Vector3 _originalPosition;
	private Quaternion _originalRotation;
	private Vector3 _originalScale;
	private Transform _originalParent;

	// 뷰어 상태
	private bool _isViewing = false;
	private bool _isRegistered = false;
	private bool _isDragging = false;
	private Vector3 _lastMousePosition;

	// 카메라 참조
	private Camera _playerCamera;
	private Player _player;

	public string InteractionPrompt => $"[F] {clueName} 살펴보기";

	public bool CanInteract(IPlayer player)
	{
		return !_isViewing;
	}

	private void Start()
	{
		_playerCamera = Camera.main;
		_player = FindAnyObjectByType<Player>();

		// 원래 상태 저장
		_originalPosition = transform.position;
		_originalRotation = transform.rotation;
		_originalScale = transform.localScale;
		_originalParent = transform.parent;
	}

	private void Update()
	{
		if (!_isViewing) return;

		HandleRotation();
		HandleExit();
	}

	public void Interact(IPlayer player)
	{
		if (_isViewing) return;

		// 처음 볼 때 단서 등록 + 대사
		if (!_isRegistered)
		{
			_isRegistered = true;
			GameManager.Instance.ClueTracker.RegisterClue(clueId);

			var uiManager = FindAnyObjectByType<UIManager>();
			if (!string.IsNullOrEmpty(dialogue))
			{
				uiManager?.ShowDialogue(speaker, dialogue);
			}
		}

		StartCoroutine(ZoomIn());
	}

	// 확대 코루틴
	private IEnumerator ZoomIn()
	{
		_isViewing = true;

		// 게임 상태 변경 (이동 차단)
		GameManager.Instance?.StateManager.ChangeState(GameState.Viewer);

		// 커서 활성화 (드래그용)
		Cursor.lockState = CursorLockMode.Confined; // 화면 밖으로 못 나가게
		Cursor.visible = true;

		// 플레이어 비활성화
		if (_player != null) _player.enabled = false;

		// 목표 위치 계산 (카메라 앞)
		Vector3 targetPos = _playerCamera.transform.position
						  + _playerCamera.transform.forward * zoomDistance;
		Quaternion targetRot = Quaternion.identity;
		Vector3 targetScale = viewScale;

		// 확대 애니메이션
		float elapsed = 0f;
		Vector3 startPos = transform.position;
		Quaternion startRot = transform.rotation;
		Vector3 startScale = transform.localScale;

		while (elapsed < zoomDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / zoomDuration;
			float smoothT = Mathf.SmoothStep(0f, 1f, t);

			transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
			transform.rotation = Quaternion.Lerp(startRot, targetRot, smoothT);
			transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);

			yield return null;
		}

		transform.position = targetPos;
		transform.rotation = targetRot;
		transform.localScale = targetScale;

		// 힌트 UI 표시
		viewerHintUI?.SetActive(true);
	}

	// 마우스 드래그로 회전
	private void HandleRotation()
	{
		// 마우스 버튼 누를 때
		if (Input.GetMouseButtonDown(0))
		{
			_isDragging = true;
			_lastMousePosition = Input.mousePosition;
		}

		// 마우스 버튼 뗄 때
		if (Input.GetMouseButtonUp(0))
		{
			_isDragging = false;
		}

		// 드래그 중 회전
		if (_isDragging)
		{
			Vector3 delta = Input.mousePosition - _lastMousePosition;

			// 좌우 드래그 → Y축 회전
			float rotY = -delta.x * rotateSpeed * Time.deltaTime * 60f;
			// 상하 드래그 → X축 회전
			float rotX = delta.y * rotateSpeed * Time.deltaTime * 60f;

			transform.Rotate(Vector3.up, rotY, Space.World);
			transform.Rotate(Vector3.right, rotX, Space.World);

			_lastMousePosition = Input.mousePosition;
		}
	}

	// 뷰어 종료
	private void HandleExit()
	{
		if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
		{
			StartCoroutine(ZoomOut());
		}
	}

	// 원위치 복귀 코루틴
	private IEnumerator ZoomOut()
	{
		// 힌트 UI 숨김
		viewerHintUI?.SetActive(false);

		float elapsed = 0f;
		Vector3 startPos = transform.position;
		Quaternion startRot = transform.rotation;
		Vector3 startScale = transform.localScale;

		while (elapsed < zoomDuration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / zoomDuration;
			float smoothT = Mathf.SmoothStep(0f, 1f, t);

			transform.position = Vector3.Lerp(startPos, _originalPosition, smoothT);
			transform.rotation = Quaternion.Lerp(startRot, _originalRotation, smoothT);
			transform.localScale = Vector3.Lerp(startScale, _originalScale, smoothT);

			yield return null;
		}

		// 원위치 복귀
		transform.position = _originalPosition;
		transform.rotation = _originalRotation;
		transform.localScale = _originalScale;

		_isViewing = false;
		_isDragging = false;

		// 플레이어 다시 활성화
		if (_player != null) _player.enabled = true;

		// 커서 잠금 복귀
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		// 게임 상태 복귀
		GameManager.Instance?.StateManager.ChangeState(GameState.Playing);
	}

	// Scene 뷰에서 줌 거리 확인
	private void OnDrawGizmosSelected()
	{
		if (Camera.main != null)
		{
			Gizmos.color = Color.cyan;
			Vector3 viewPos = Camera.main.transform.position
							+ Camera.main.transform.forward * zoomDistance;
			Gizmos.DrawWireSphere(viewPos, 0.2f);
			Gizmos.DrawLine(Camera.main.transform.position, viewPos);
		}
	}
}