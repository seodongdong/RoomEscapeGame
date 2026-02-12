using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어 컨트롤러
/// ⭐ 모델링 크기에 맞게 속도/거리 조절 가능
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
	[SerializeField] private float verticalLookLimit = 80f;

	[Header("Interaction Settings")]
	[SerializeField] private float interactionDistance = 3f; // ⭐ 조절 가능

	[Header("Ground Check")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private float groundDistance = 0.4f;
	[SerializeField] private LayerMask groundMask;

	[Header("Health")]
	[SerializeField] private int maxHealth = 100;

	private CharacterController _controller;
	private IInventory _inventory;
	private IHealth _health;
	private IUIManager _uiManager;
	private IInteractable _currentInteractable;

	private Vector3 _velocity;
	private bool _isGrounded;
	private float _verticalRotation = 0f;

	public IInventory Inventory => _inventory;
	public IHealth Health => _health;
	public Transform Transform => transform;

	private void Awake()
	{
		InitializeComponents();
	}

	private void Start()
	{
		_uiManager = FindAnyObjectByType<UIManager>();
		UpdateHealthUI(_health.CurrentHealth);
	}

	private void Update()
	{
		if (GameManager.Instance != null)
		{
			var state = GameManager.Instance.StateManager.CurrentState;

			// 조작 차단 상태
			if (state == GameState.Puzzle ||
				state == GameState.Paused ||
				state == GameState.Viewer)   // 🆕 Viewer 추가
			{
				if (_currentInteractable != null)
				{
					_currentInteractable = null;
					_uiManager?.HideInteractionPrompt();
				}

				// Paused 상태에서만 ESC 처리
				if (state == GameState.Paused)
				{
					HandleCursorToggle();
				}
				return;
			}
		}

		HandleMouseLook();
		HandleMovement();
		HandleInteraction();
		HandleCursorToggle();
	}

	private void InitializeComponents()
	{
		_controller = GetComponent<CharacterController>();
		_inventory = new PlayerInventory();
		_health = new PlayerHealth(maxHealth);

		_health.OnDeath += Die;
		_health.OnHealthChanged += UpdateHealthUI;

		if (cameraTransform == null)
		{
			cameraTransform = GetComponentInChildren<Camera>().transform;
		}

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private bool CanMove()
	{
		if (GameManager.Instance == null) return true;

		var state = GameManager.Instance.StateManager.CurrentState;
		return state != GameState.Puzzle && state != GameState.Paused;
	}

	private void HandleMouseLook()
	{
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		transform.Rotate(Vector3.up * mouseX);

		_verticalRotation -= mouseY;
		_verticalRotation = Mathf.Clamp(_verticalRotation, -verticalLookLimit, verticalLookLimit);
		cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
	}

	private void HandleMovement()
	{
		_isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

		if (_isGrounded && _velocity.y < 0)
		{
			_velocity.y = -2f;
		}

		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

		bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		float currentSpeed = isRunning ? runSpeed : walkSpeed;

		_controller.Move(moveDirection * currentSpeed * Time.deltaTime);

		_velocity.y += gravity * Time.deltaTime;
		_controller.Move(_velocity * Time.deltaTime);

		if (moveDirection.magnitude > 0.1f && _isGrounded)
		{
			PlayFootstepSound(isRunning);
		}
	}

	private float _footstepTimer = 0f;
	private float _footstepInterval = 0.5f;

	private void PlayFootstepSound(bool isRunning)
	{
		_footstepTimer += Time.deltaTime;

		float interval = isRunning ? _footstepInterval * 0.7f : _footstepInterval;

		if (_footstepTimer >= interval)
		{
			var audioManager = FindAnyObjectByType<AudioManager>();
			audioManager?.PlayFootstep();
			_footstepTimer = 0f;
		}
	}

	private void HandleInteraction()
	{
		RaycastHit hit;

		if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactionDistance))
		{
			var interactable = hit.collider.GetComponent<IInteractable>();

			if (interactable != null && interactable != _currentInteractable)
			{
				SetCurrentInteractable(interactable);
			}
		}
		else
		{
			if (_currentInteractable != null)
			{
				SetCurrentInteractable(null);
			}
		}

		if (Input.GetKeyDown(KeyCode.F) && _currentInteractable != null)
		{
			if (_currentInteractable.CanInteract(this))
			{
				_currentInteractable.Interact(this);
			}
		}
	}

	public void SetCurrentInteractable(IInteractable interactable)
	{
		_currentInteractable = interactable;

		if (_uiManager != null)
		{
			if (interactable != null)
			{
				_uiManager.ShowInteractionPrompt(interactable.InteractionPrompt);
			}
			else
			{
				_uiManager.HideInteractionPrompt();
			}
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
		_health.TakeDamage(damage);
		StartCoroutine(DamageEffect());
	}

	private IEnumerator DamageEffect()
	{
		float duration = 0.2f;
		float elapsed = 0f;
		Vector3 originalPos = cameraTransform.localPosition;

		while (elapsed < duration)
		{
			float x = Random.Range(-0.1f, 0.1f);
			float y = Random.Range(-0.1f, 0.1f);

			cameraTransform.localPosition = originalPos + new Vector3(x, y, 0f);

			elapsed += Time.deltaTime;
			yield return null;
		}

		cameraTransform.localPosition = originalPos;
	}

	public void Die()
	{
		Debug.Log("[Player] 사망!");
		GameManager.Instance?.StateManager.ChangeState(GameState.GameOver);
		enabled = false;
	}

	private void UpdateHealthUI(int currentHealth)
	{
		_uiManager?.UpdateHealthUI(currentHealth, _health.MaxHealth);
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