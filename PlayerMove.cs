using NUnit.Framework.Constraints;
using System.Collections;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    public static PlayerMove instance { get; private set; }
    [Header("Move")]
    [SerializeField] private float _moveSpeed = 10;
    private float _direction;
    [Header("Dash")]
    private bool _canDash;
    [SerializeField] private float _dashForce;
    [SerializeField] private float _dashTime;
    [SerializeField] private float _dashCooldown;
    private float _dashCooldownTimer;
    [Header("Jump")]
    [SerializeField] private float _jumpForce = 1;
    [SerializeField] private int _jumpAmount = 1;
    [SerializeField] private int _maxJumpAmount = 1;
    [SerializeField] private Transform _feetPos;
    private bool _isGrounded;
    [SerializeField] private float _checkRadius;
    [SerializeField] private LayerMask _whatIsGround;
    private bool _isFacingRight;
    private bool _isDashing;

    private float _baseGravity;
    private Animator _animator;
    private Rigidbody2D _rb;
    private PlayerInputController _input;

    private void Awake()
    {
        _input = new PlayerInputController();
        _input.Player.Jump.performed += context => Jump();
        _input.Player.Jump.canceled += context => EndJump();
        _input.Player.Dash.performed += context => StartDash();
    }
    void Start()
    {
        if (instance != null && instance != this || SceneManager.GetActiveScene().buildIndex == 0)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
        _rb = GetComponent<Rigidbody2D>();
        _baseGravity = _rb.gravityScale;
        _animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        _input.Enable();
    }
    private void OnDisable()
    {
        _input.Disable();
    }

    private void Update()
    {
        if(_rb.velocity.y < -0.1f)
        {
            _animator.SetBool("IsFalling", true);
            EndJump();
        }
        else
        {
            _animator.SetBool("IsFalling", false);
        }
        IsGrounded();
        Flip();
    }
    void FixedUpdate()
    {
        if (!_isDashing)
        {
            _direction = _input.Player.Move.ReadValue<float>();
            _rb.velocity = new Vector2(_direction * _moveSpeed, _rb.velocity.y);
            if (_direction != 0 && !_animator.GetBool("IsFalling")) 
            {
                _animator.SetBool("IsRunning", true);
            }
            else
            {
                _animator.SetBool("IsRunning", false);
            }
        }
    }

    private void Jump()
    {
        if (_jumpAmount > 0 && _isGrounded && !_isDashing)
        {
            _jumpAmount--;
            _animator.SetBool("IsJumping", true);
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
    }
    private void EndJump()
    {
        if (_rb.velocity.y > 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, -0.2f);
        }
        _animator.SetBool("IsJumping", false);
    }
    private void IsGrounded()
    {
        _isGrounded = Physics2D.OverlapCircle(_feetPos.position, _checkRadius, _whatIsGround);
        if (_isGrounded) {
            _jumpAmount = _maxJumpAmount;
            _canDash = true;
        }
    }
    private void StartDash()
    {
        if (_canDash && _dashCooldownTimer == 0)
        {
            StartCoroutine(Dash());
            _animator.SetBool("IsDashing", true);
            _isDashing = true;
            _canDash = false;
        }
    }
    IEnumerator Dash()
    {
        if (_canDash && _dashCooldownTimer == 0)
        {
            _dashCooldownTimer = _dashCooldown;
            _rb.gravityScale = 0;
            _rb.velocity = new Vector2(_dashForce * transform.localScale.x, 0);

            yield return new WaitForSeconds(_dashTime);

            _animator.SetBool("IsDashing", false);
            _isDashing = false;

            _rb.gravityScale = _baseGravity;
            yield return new WaitForSeconds(_dashCooldown);
            _dashCooldownTimer = 0;
        }
        
    }
    private void Flip()
    {
        if (_rb.velocity.x < 0)
        {
            transform.localScale = new Vector2(-1,transform.localScale.y);
            _isFacingRight = false;
        }
        else if (_rb.velocity.x > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            _isFacingRight = true;
        }
    }
}
