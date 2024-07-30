using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(Animator))]
public class PlayerActions : MonoBehaviour
{
    public static PlayerActions instance { get; private set; }
    private bool _canInteract;
    private Interactable _interactable;
    [Header("Attack")]
    [SerializeField] private int _attackDamage;
    [SerializeField] private float _attackCooldownTime;
    [SerializeField] private bool _canAttack;
    [SerializeField] private Transform _sideAttackTransform, _upAttackTransform,_downAttackTransform;
    [SerializeField] private Vector2 _sideAttackArea, _upAttackArea, _downAttackArea;
    [SerializeField] LayerMask _attackableLayer;

    private Animator _animator;
    private Rigidbody2D _rb;
    public PlayerInputController _input;
    private void Start()
    {
        if (instance != null && instance != this || SceneManager.GetActiveScene().name == "MenuScene")
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);

        _animator = GetComponent<Animator>();
        _canAttack = true;
        _rb = GetComponent<Rigidbody2D>();

    }

    private void Awake()
    {

        _input = new PlayerInputController();
        _input.Player.Attack.performed += context => Attack();
        _input.Player.Interact.performed += context => Interact();
    }
    private void OnEnable()
    {
        _input.Enable();

    }
    private void OnDisable()
    {
        _input.Disable();
    }

    private void Attack()
    {
        if (_canAttack)
        {
            _canAttack = false;
            float yAxis = _input.Player.Vertical.ReadValue<float>();
            _animator.SetTrigger("IsAttack");
            Debug.Log("yAxis:" + yAxis);
            if (yAxis > 0)
            {
                Hit(_upAttackTransform, _upAttackArea, new Vector2(0,1));
                Debug.Log("UpAttack");
            }
            else if (yAxis < 0)
            {
                Hit(_downAttackTransform, _downAttackArea, new Vector2(0, -1));
                Debug.Log("DownAttack");
            }
            else
            {
                float xDir = transform.localScale.x;
                Hit(_sideAttackTransform, _sideAttackArea, new Vector2(xDir, 0));
                Debug.Log("SideAttack");
            }
            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(_attackCooldownTime);
        _canAttack = true;
    }
    private void Hit(Transform _attackTransform, Vector2 _attackArea, Vector2 knockbackDirection)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position,_attackArea,0,_attackableLayer);
        Debug.Log(objectsToHit.Length);
        foreach (Collider2D enemy in objectsToHit)
        {
            enemy.gameObject.GetComponent<Health>().TakeDamage(_attackDamage, knockbackDirection);
        }
    }
    private void Interact()
    {
        if (_canInteract) 
        {
            _interactable.Interact();
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            _canInteract = true;
            _interactable = collision.GetComponent<Interactable>();
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            _animator.SetTrigger("TakeDamage");

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Interactable"))
        {
            _canInteract = false;
            _interactable = null;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_upAttackTransform.position, _upAttackArea);
        Gizmos.DrawWireCube(_downAttackTransform.position, _downAttackArea);
        Gizmos.DrawWireCube(_sideAttackTransform.position,_sideAttackArea);
    }
}

