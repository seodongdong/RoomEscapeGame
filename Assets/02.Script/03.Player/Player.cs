using UnityEngine;

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

	private CharacterController _controller;
	private IInventory _inventory;
	private IUIManager _uiManager;
	private IInteractable _currentInteractable;

	private Vector3 _velocity;
	private bool _isGrounded;
	private float _verticalRotation = 0f;

	public IInventory Inventory => _inventory;
	public Transform Transform => transform;

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
	}

	private void Update()
	{
		if (GameManager.Instance != null)
		{
			var state = GameManager.Instance.StateManager.CurrentState;
			if (state == GameState.Puzzle || state == GameState.Paused)
			{
				if (_currentInteractable != null)
				{
					_currentInteractable = null;
					_uiManager?.HideInteractionPrompt();
				}
				return;
			}
		}

		var inventoryUI = FindAnyObjectByType<InventoryUI_Complete>();
		if (inventoryUI != null)
		{
			// 인벤토리가 열려있으면 조작 불가
			GameObject inventoryPanel = inventoryUI.GetComponent<InventoryUI_Complete>().transform.Find("InventoryPanel")?.gameObject;
			if (inventoryPanel != null && inventoryPanel.activeSelf)
				return;
		}

		HandleMouseLook();
		HandleMovement();
		HandleInteraction();
		HandleCursorToggle();
	}

	private void HandleMouseLook()
	{
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		transform.Rotate(Vector3.up * mouseX);

		_verticalRotation -= mouseY;
		cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
	}

	private void HandleMovement()
	{
		_isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

		if (_isGrounded && _velocity.y < 0)
			_velocity.y = -2f;

		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

		bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		float currentSpeed = isRunning ? runSpeed : walkSpeed;

		_controller.Move(moveDirection * currentSpeed * Time.deltaTime);

		_velocity.y += gravity * Time.deltaTime;
		_controller.Move(_velocity * Time.deltaTime);

		if (moveDirection.magnitude > 0.1f && _isGrounded)
			PlayFootstepSound();
	}

	private float _footstepTimer = 0f;
	private float _footstepInterval = 0.5f;

	private void PlayFootstepSound()
	{
		_footstepTimer += Time.deltaTime;

		float interval = Input.GetKey(KeyCode.LeftShift) ? _footstepInterval * 0.7f : _footstepInterval;

		if (_footstepTimer >= interval)
		{
			var audioManager = FindAnyObjectByType<AudioManager>();
			audioManager?.PlayFootstep();
			_footstepTimer = 0f;
		}
	}

	private void HandleInteraction()
	{
		// 임시 디버그 — 원인 찾으면 삭제
		Debug.Log($"현재 GameState: {GameManager.Instance?.StateManager.CurrentState}");

		RaycastHit hit;

		if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance, ~0, QueryTriggerInteraction.Collide))
		{
			var interactable = hit.collider.GetComponent<IInteractable>();

			if (interactable != null)
			{
				// ⭐ 매 프레임 프롬프트 갱신 (같은 오브젝트여도)
				_currentInteractable = interactable;
				_uiManager?.ShowInteractionPrompt(interactable.InteractionPrompt);
			}
			else
			{
				if (_currentInteractable != null)
				{
					_currentInteractable = null;
					_uiManager?.HideInteractionPrompt();
				}
			}
		}
		else
		{
			if (_currentInteractable != null)
			{
				_currentInteractable = null;
				_uiManager?.HideInteractionPrompt();
			}
		}

		if (Input.GetKeyDown(KeyCode.F) && _currentInteractable != null)
		{
			if (_currentInteractable.CanInteract(this))
				_currentInteractable.Interact(this);
		}


	
	}

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

	private void HandleCursorToggle()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (Cursor.lockState == CursorLockMode.Locked)
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
	}

	public void TakeDamage(int damage)
	{
		Die();
	}

	public void Die()
	{
		Debug.Log("플레이어 사망!");
		GameManager.Instance?.StateManager.ChangeState(GameState.GameOver);
		GameManager.Instance?.EndingManager.TriggerEnding(EndingType.GameOver);
		enabled = false;
	}

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