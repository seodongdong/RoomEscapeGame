using UnityEngine;

/// <summary>
/// 플레이어 컨트롤러
/// 
/// [ESC 동작 기획서 기준]
/// - ESC: UILayerManager에 위임 (열린 UI 닫기 → 없으면 일시정지)
/// - E  : 열린 UI가 있을 때만 닫기
/// - I  : 인벤토리 토글 (InventoryUI_Complete에 위임)
/// </summary>
public class Player : MonoBehaviour, IPlayer
{
	[Header("Movement Settings")]
	[SerializeField] private float walkSpeed = 5f;
	[SerializeField] private float runSpeed = 8f;
	[SerializeField] private float gravity = -9.81f;

	[Header("Camera Settings")]
	[SerializeField] private Transform cameraTransform;
	[SerializeField] private float mouseSensitivity = 2f;

	[Header("Interaction Settings")]
	[SerializeField] private float interactionDistance = 3f;

	[Header("Ground Check")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private float groundDistance = 0.4f;
	[SerializeField] private LayerMask groundMask;

	// ── 캐싱 ─────────────────────────────────────────────────
	private CharacterController _controller;
	private IInventory _inventory;
	private IUIManager _uiManager;
	private InventoryUI_Complete _inventoryUI;
	private AudioManager _audioManager;
	private IInteractable _currentInteractable;

	private Vector3 _velocity;
	private bool _isGrounded;
	private float _verticalRotation = 0f;

	public IInventory Inventory => _inventory;
	public Transform Transform => transform;

	// ── 초기화 ────────────────────────────────────────────────
	private void Awake()
	{
		_controller = GetComponent<CharacterController>();
		_inventory = new PlayerInventory();

		if (cameraTransform == null)
			cameraTransform = GetComponentInChildren<Camera>().transform;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Start()
	{
		_uiManager = FindAnyObjectByType<UIManager>();
		_inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();
		_audioManager = FindAnyObjectByType<AudioManager>();
	}

	// ── 매 프레임 ─────────────────────────────────────────────
	private void Update()
	{
		// ── ESC ───────────────────────────────────────────────
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (UILayerManager.Instance != null)
				UILayerManager.Instance.HandleEsc();
			else
				FallbackTogglePause(); // UILayerManager 없는 씬용 fallback
		}
		// ── E: 열린 UI 닫기 (일시정지는 ESC만) ──────────────
		else if (Input.GetKeyDown(KeyCode.E))
		{
			if (UILayerManager.Instance != null && UILayerManager.Instance.HasOpenUI)
				UILayerManager.Instance.HandleEsc();
		}

		// ── I: 인벤토리 토글 ─────────────────────────────────
		if (Input.GetKeyDown(KeyCode.I))
		{
			if (_inventoryUI != null)
			{
				if (_inventoryUI.IsOpen)
					_inventoryUI.CloseInventory();
				else
					_inventoryUI.OpenInventory();
			}
		}

		// ── 게임 상태 차단 ────────────────────────────────────
		if (GameManager.Instance != null)
		{
			var state = GameManager.Instance.StateManager.CurrentState;
			if (state == GameState.Puzzle ||
				state == GameState.Paused ||
				state == GameState.Viewer)
			{
				ClearInteractable();
				return;
			}
		}

		// ── UILayerManager에 열린 UI 있으면 차단 ─────────────
		if (UILayerManager.Instance != null && UILayerManager.Instance.HasOpenUI)
		{
			ClearInteractable();
			return;
		}

		HandleMouseLook();
		HandleMovement();
		HandleInteraction();
	}

	// ── UILayerManager 없는 씬 fallback ──────────────────────
	private bool _fallbackPaused = false;
	private void FallbackTogglePause()
	{
		_fallbackPaused = !_fallbackPaused;
		if (_fallbackPaused)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			GameManager.Instance?.StateManager.ChangeState(GameState.Paused);
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			GameManager.Instance?.StateManager.ChangeState(GameState.Playing);
		}
	}

	// ── 시점 ──────────────────────────────────────────────────
	private void HandleMouseLook()
	{
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		transform.Rotate(Vector3.up * mouseX);
		_verticalRotation = Mathf.Clamp(_verticalRotation - mouseY, -90f, 90f);
		cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
	}

	// ── 이동 ──────────────────────────────────────────────────
	private void HandleMovement()
	{
		_isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

		if (_isGrounded && _velocity.y < 0)
			_velocity.y = -2f;

		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		Vector3 moveDir = transform.right * horizontal + transform.forward * vertical;

		bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		float currentSpeed = isRunning ? runSpeed : walkSpeed;

		_controller.Move(moveDir * currentSpeed * Time.deltaTime);
		_velocity.y += gravity * Time.deltaTime;
		_controller.Move(_velocity * Time.deltaTime);

		if (moveDir.magnitude > 0.1f && _isGrounded)
			PlayFootstepSound();
	}

	private float _footstepTimer = 0f;
	private float _footstepInterval = 0.5f;

	private void PlayFootstepSound()
	{
		_footstepTimer += Time.deltaTime;
		float interval = Input.GetKey(KeyCode.LeftShift)
			? _footstepInterval * 0.7f : _footstepInterval;
		if (_footstepTimer >= interval)
		{
			_audioManager?.PlayFootstep();
			_footstepTimer = 0f;
		}
	}

	// ── 상호작용 ───────────────────────────────────────────────
	private void HandleInteraction()
	{
		RaycastHit hit;
		if (Physics.Raycast(cameraTransform.position, cameraTransform.forward,
			out hit, interactionDistance, ~0, QueryTriggerInteraction.Collide))
		{
			var interactable = hit.collider.GetComponent<IInteractable>();
			if (interactable != null)
			{
				_currentInteractable = interactable;
				string prompt = interactable.InteractionPrompt;
				if (!string.IsNullOrEmpty(prompt))
					_uiManager?.ShowInteractionPrompt(prompt);
				else
					_uiManager?.HideInteractionPrompt();
			}
			else
			{
				ClearInteractable();
			}
		}
		else
		{
			ClearInteractable();
		}

		if (Input.GetKeyDown(KeyCode.F) && _currentInteractable != null)
		{
			if (_currentInteractable.CanInteract(this))
				_currentInteractable.Interact(this);
		}
	}

	private void ClearInteractable()
	{
		if (_currentInteractable != null)
		{
			_currentInteractable = null;
			_uiManager?.HideInteractionPrompt();
		}
	}

	// ── 외부 호출 ─────────────────────────────────────────────
	public void SetCurrentInteractable(IInteractable interactable)
	{
		_currentInteractable = interactable;
		if (_uiManager != null)
		{
			if (interactable != null)
				_uiManager.ShowInteractionPrompt(interactable.InteractionPrompt);
			else
				_uiManager.HideInteractionPrompt();
		}
	}

	// ── 데미지/사망 ───────────────────────────────────────────
	public void TakeDamage(int damage) => Die();

	public void Die()
	{
		Debug.Log("[Player] 사망!");
		GameManager.Instance?.StateManager.ChangeState(GameState.GameOver);
		GameManager.Instance?.EndingManager.TriggerEnding(EndingType.GameOver);
		enabled = false;
	}

	// ── 기즈모 ────────────────────────────────────────────────
	private void OnDrawGizmosSelected()
	{
		if (groundCheck != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
		}
		if (cameraTransform != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance);
		}
	}
}