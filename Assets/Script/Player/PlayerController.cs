using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerData
{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Components")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerAnimAttack _playerAnimAttack; 
        public Animator Animator => _animator;
        [Header("Base Movement")]
        public float walkSpeed = 2f;
        public float runSpeed = 4f;

        [Header("Roll / Dodge")]
        public float rollSpeed = 8f;
        public float rollDuration = 0.5f;

        private float runAcceleration = 50f;
        private float drag = 20f;
        private float rotationSpeed = 15f;
        private float gravity = -9.81f;
        private float _verticalVelocity;

        private bool _isRolling;
        private Vector3 _rollDirection;
        private Coroutine _rollCoroutine;

        private PlayerLocomotionInput _playerLocomotionInput;
        private Vector3 _currentVelocity;

        public bool IsRolling => _isRolling;

        // Animator Parameter IDs
        public static readonly int VelocityXHash = Animator.StringToHash("VelocityX");
        public static readonly int VelocityYHash = Animator.StringToHash("VelocityY");
        public static readonly int IsRollHash = Animator.StringToHash("IsRoll");
        public static readonly int AttackTriggerHash = Animator.StringToHash("IsSlash");
        public static readonly int AttackTriggerHash2 = Animator.StringToHash("IsSlash2");
        public static readonly int ShootTriggerHash = Animator.StringToHash("IsShooting");
        public static readonly int IsIdleBoolHash = Animator.StringToHash("IsIdling");

        private void Awake()
        {
            if (Instance == null) Instance = this;

            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            
            // Mengambil referensi jika belum di-assign melalui Inspector
            if (_playerAnimAttack == null) _playerAnimAttack = GetComponent<PlayerAnimAttack>();
        }

        private void Update()
        {
            HandleMovement();
            HandleInputAttack(); // Memeriksa input serangan & shooting
            UpdateAnimator();
        }

        private void HandleInputAttack()
        {
            if (_playerAnimAttack == null) return;

            // Attack / Slash Input (Left Click Mouse atau Tombol J)
            bool isAttackPressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                                   (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame);

            if (isAttackPressed)
            {
                if (_isRolling) StopRoll();
                _playerAnimAttack.HandleAttack();
            }

            // Shoot Input (Right Click Mouse atau Tombol K)
            bool isShootPressed = (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
                                  (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame);

            if (isShootPressed)
            {
                if (_isRolling) StopRoll();
                _playerAnimAttack.HandleShoot();
            }
        }

        private void StopRoll()
        {
            _isRolling = false;
            if (_rollCoroutine != null)
            {
                StopCoroutine(_rollCoroutine);
                _rollCoroutine = null;
            }
        }

        private void HandleMovement()
        {
            // 1. Handling Gravitasi Dasar
            bool isGrounded = _characterController.isGrounded;
            if (isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -2f;
            }
            _verticalVelocity += gravity * Time.deltaTime;

            // 2. Hitung arah pergerakan horizontal (menggunakan Camera.main)
            Transform camTransform = Camera.main != null ? Camera.main.transform : null;
            Vector3 cameraForwardXZ = camTransform != null ? new Vector3(camTransform.forward.x, 0f, camTransform.forward.z).normalized : Vector3.forward;
            Vector3 cameraRightXZ = camTransform != null ? new Vector3(camTransform.right.x, 0f, camTransform.right.z).normalized : Vector3.right;

            Vector3 inputDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + 
                                     cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

            // 3. Handling Roll / Dodge Input (Tombol Space)
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !_isRolling)
            {
                StartRoll(inputDirection);
            }

            // Jika sedang rolling, pindahkan karakter maju ke depan sesuai _rollDirection & rollSpeed
            if (_isRolling)
            {
                Vector3 rollMove = _rollDirection * rollSpeed;
                rollMove.y = _verticalVelocity;
                _characterController.Move(rollMove * Time.deltaTime);
                return;
            }

            // 4. Pengecekan input lari (Shift) - Tidak bisa lari saat sedang Attack / Slash, Shoot, atau Roll
            bool canSprint = _playerAnimAttack == null || (!_playerAnimAttack.IsAttackingOrShooting && !_isRolling);
            bool isSprinting = canSprint && Keyboard.current != null && 
                               (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
            float targetSpeed = (isSprinting && inputDirection.sqrMagnitude > 0.01f) ? runSpeed : walkSpeed;

            // 5. Akselerasi & Drag Horizontal
            Vector3 movementDelta = inputDirection * runAcceleration * Time.deltaTime;
            _currentVelocity += movementDelta;

            Vector3 currentDrag = _currentVelocity.normalized * drag * Time.deltaTime;
            _currentVelocity = (_currentVelocity.magnitude > drag * Time.deltaTime) 
                ? _currentVelocity - currentDrag 
                : Vector3.zero;

            _currentVelocity = Vector3.ClampMagnitude(_currentVelocity, targetSpeed);

            // 6. Rotasi Karakter
            if (inputDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // 7. Eksekusi Pergerakan Total
            Vector3 finalMove = _currentVelocity;
            finalMove.y = _verticalVelocity;
            _characterController.Move(finalMove * Time.deltaTime);
        }

        private void StartRoll(Vector3 inputDirection)
        {
            _isRolling = true;

            if (_playerAnimAttack != null)
            {
                _playerAnimAttack.CancelAttackAndShoot();
            }

            // Menentukan arah roll: jika ada input pergerakan, roll ke arah input; jika tidak, roll ke depan karakter
            if (inputDirection.sqrMagnitude > 0.01f)
            {
                _rollDirection = inputDirection.normalized;
                transform.rotation = Quaternion.LookRotation(_rollDirection);
            }
            else
            {
                _rollDirection = transform.forward;
            }

            if (_animator != null)
            {
                _animator.SetBool(IsIdleBoolHash, false);
                _animator.ResetTrigger(IsRollHash);
                _animator.SetTrigger(IsRollHash);
            }

            if (_rollCoroutine != null)
            {
                StopCoroutine(_rollCoroutine);
            }
            _rollCoroutine = StartCoroutine(ResetIdleAfterRoll(rollDuration));
        }

        public void OnRollComplete()
        {
            _isRolling = false;

            if (_rollCoroutine != null)
            {
                StopCoroutine(_rollCoroutine);
                _rollCoroutine = null;
            }

            if (_animator != null)
            {
                bool isAttackingOrShooting = _playerAnimAttack != null && _playerAnimAttack.IsAttackingOrShooting;
                if (!isAttackingOrShooting)
                {
                    _animator.SetBool(IsIdleBoolHash, true);
                }
            }
        }

        private IEnumerator ResetIdleAfterRoll(float delay)
        {
            yield return new WaitForSeconds(delay);
            OnRollComplete();
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;

            bool canSprint = _playerAnimAttack == null || (!_playerAnimAttack.IsAttackingOrShooting && !_isRolling);
            bool isSprinting = canSprint && Keyboard.current != null && 
                               (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

            float currentMaxSpeed = isSprinting ? runSpeed : walkSpeed;
            float targetMultiplier = isSprinting ? 1.0f : 0.5f;

            Vector3 localVelocity = transform.InverseTransformDirection(_currentVelocity);

            Vector3 animVelocity = (currentMaxSpeed > 0f) 
                ? (localVelocity / currentMaxSpeed) * targetMultiplier 
                : Vector3.zero;

            _animator.SetFloat(VelocityXHash, animVelocity.x, 0.1f, Time.deltaTime);
            _animator.SetFloat(VelocityYHash, animVelocity.z, 0.1f, Time.deltaTime);
        }
    }
}