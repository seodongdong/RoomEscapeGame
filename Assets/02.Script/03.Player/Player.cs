using UnityEngine;
using System.Collections;

// 플레이어 캐릭터를 제어하는 클래스

public class Player : MonoBehaviour, IPlayer
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    
    // 컴포넌트 및 인터페이스 참조
    private CharacterController _controller;
    private IInventory _inventory;
    private IHealth _health;
    private IUIManager _uiManager;
    private IInteractable _currentInteractable;
    
    // 내부 상태 변수
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _verticalRotation = 0f;
    
    // 인터페이스 구현
    public IInventory Inventory => _inventory;
    public IHealth Health => _health;
    public Transform Transform => transform;

    private void Awake()
    {
        // 컴포넌트 및 인터페이스 초기화
        _controller = GetComponent<CharacterController>();
        _inventory = new PlayerInventory();
        _health = new PlayerHealth(maxHealth);
        _health.OnDeath += Die;
        _health.OnHealthChanged += UpdateHealthUI;
        
        // 카메라 참조 설정
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }
        
        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        // UI 매니저 참조 설정
        _uiManager = FindAnyObjectByType<UIManager>();
        // 초기 체력 UI 업데이트
        UpdateHealthUI(_health.CurrentHealth);
    }

    private void Update()
    {
        if (GameManager.Instance != null)
        {
            // 현재 게임 상태 확인
            var state = GameManager.Instance.StateManager.CurrentState;
            if (state == GameState.Puzzle || state == GameState.Paused)
            {
                return;
            }
        }
        
        // 입력 처리
        HandleMouseLook();
        HandleMovement();
        HandleInteraction();
        HandleCursorToggle();
    }

    // 마우스 움직임 처리
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity; // 마우스 X 축 입력
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity; // 마우스 Y 축 입력
        
        transform.Rotate(Vector3.up * mouseX);  // 플레이어 회전
        
        _verticalRotation -= mouseY;            // 수직 회전 계산
        cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);    // 카메라 회전 적용
    }

    // 플레이어 이동 처리
    private void HandleMovement()
    {
        // 지면 체크
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        // 지면에 있을 때 수직 속도 초기화
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }
        
        float horizontal = Input.GetAxis("Horizontal"); // 좌우 이동 입력
        float vertical = Input.GetAxis("Vertical");     // 앞뒤 이동 입력
        
        // 이동 방향 계산
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        
        // 이동 속도 결정
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        // 실제 이동 처리
        _controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        
        // 중력 적용
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
        
        // 발소리 재생
        if (moveDirection.magnitude > 0.1f && _isGrounded)
        {
            PlayFootstepSound();
        }
    }

    private float _footstepTimer = 0f;          // 발소리 타이머
    private float _footstepInterval = 0.5f;     // 발소리 재생 간격
    
    // 발소리 재생 처리
    private void PlayFootstepSound()
    {
        _footstepTimer += Time.deltaTime; 
        
        // 달리기 시 발소리 재생 간격 단축
        float interval = Input.GetKey(KeyCode.LeftShift) ? _footstepInterval * 0.7f : _footstepInterval;
        
        // 발소리 재생
        if (_footstepTimer >= interval)
        {
            var audioManager = FindAnyObjectByType<AudioManager>();
            audioManager?.PlayFootstep();
            _footstepTimer = 0f;
        }
    }

    // 상호작용 처리
    private void HandleInteraction()
    {
        RaycastHit hit;
        float interactionDistance = 3f;
        
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
            if (_currentInteractable.CanInteract(this)) // 상호작용 가능 여부 확인
            {
                _currentInteractable.Interact(this);    // 상호작용 수행
            }
        }
    }

    // 현재 상호작용 가능한 오브젝트 설정
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

    // 마우스 커서 토글 처리
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

    // 플레이어가 데미지를 입었을 때 호출되는 메서드
    public void TakeDamage(int damage)
    {
        _health.TakeDamage(damage);
        StartCoroutine(DamageEffect());
    }

    // 데미지 효과 코루틴 (카메라 흔들림)
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

    // 플레이어 사망 처리
    public void Die()
    {
        Debug.Log("플레이어 사망!");
        GameManager.Instance?.StateManager.ChangeState(GameState.GameOver);
        enabled = false;
    }

    // 체력 UI 업데이트
    private void UpdateHealthUI(int currentHealth)
    {
        _uiManager?.UpdateHealthUI(currentHealth, _health.MaxHealth);
    }

    // 디버그용 지오메트리 그리기
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
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * 3f);
        }
    }
}