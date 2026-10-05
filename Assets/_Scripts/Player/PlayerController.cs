using System;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
   
    [SerializeField] private float _baseSpeed;
    [SerializeField] private float _smoothAmount;
    [SerializeField] private float _baseHealth;

    private float _speed;
    private float _health;
    
    [Range(0,100)]
    [SerializeField] private float _shieldSlowPercentage;
    [SerializeField] private GameObject _shieldObject;
    
    [SerializeField] private SpriteRenderer _bubbleShieldVisuals;
    public static event Action<int> BubbleShieldChange;
    
    private ShieldScript _shieldLogic;
    private PlayerProjectile _playerProjectile;
    private Vector3 _shieldDirection;
    private bool _shieldFlying;
    
    private int _currentBubbleShieldAmount;
    
    private Rigidbody2D _rigidBody;

    private GameManager _gameManager;
    
    private bool _aiming;
    private bool _bounce;
    private bool _bouncing;
    private float _currentBounceDistance;
    
    private float _timeMod;
    private float _timeModSave;
    private Vector2 _reflectNormal;
    private Vector2 _moveInput;
    private Vector2 _smoothMoveInput;
    private Vector2 _smoothMoveVelocity;
    

    // TODO: rework / remove portals?
    public bool teleporting;
    public static event Action OnPlayerDeath;

    public static PlayerController Instance; 
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _playerProjectile = _shieldObject.GetComponent<PlayerProjectile>();
        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        _speed = _baseSpeed;
        _health = _baseHealth;
    }

    void Update()
    {
        if (GameStateManager.Instance.IsPaused())
        {
            _rigidBody.linearVelocity = Vector2.zero;
            return;
        }
        
        GetInput();
        ShootInteraction();
        //ShieldInteraction();
    }
    
    private void ShootInteraction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _playerProjectile.RequestShootProjectile();
        }
    }
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
       _smoothMoveInput = Vector2.SmoothDamp(_moveInput, _smoothMoveInput, ref _smoothMoveVelocity, _smoothAmount);
       _rigidBody.linearVelocity = _baseSpeed * _moveInput;
        if (IsPlayerShielding())
        {
            _rigidBody.linearVelocity *= 1 - ( _shieldSlowPercentage / 100);
        }
        
    }
    private void GetInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector2(moveX, moveY).normalized;
    }
    
    private void ShieldInteraction()
    {
        // Handling Right Click
        if (Input.GetMouseButtonDown(1))
        {
            _playerProjectile.RequestOpenShield();
        }       
        if (Input.GetMouseButtonUp(1))
        {
            _playerProjectile.RequestCloseShield();
        }
    }

    private bool IsPlayerShielding()
    {
        return _playerProjectile.IsShielding();
    }

    public void Damage(float dmg, GameObject damageSource)
    {
        // TODO: Decide if we will need float dmg or int dmg
        _health -= FilterDamageThroughBubbleShield((int)dmg, damageSource);
        if (_health <= 0)
        {
            // TODO DIE SOUND
            OnPlayerDeath?.Invoke();
        }
    }

    public void Kill()
    {
        _health = 0;
        OnPlayerDeath?.Invoke();
    }

    public void RequestSpeedChange(float speedDataSpeedIncrease)
    {
        _speed += speedDataSpeedIncrease;
    }

    public void RequestSpeedReset()
    {
        _speed = _baseSpeed;
    }

    public void RequestRangeChange(float decreaseDataRangeDecrease)
    {
        _playerProjectile.RequestRangeChange(decreaseDataRangeDecrease);
    }

    public void RequestRangeReset()
    {
        _playerProjectile.RequestRangeReset();
    }

    public void RequestShieldAdd(int shieldDataBubbleAmount)
    {
        _currentBubbleShieldAmount = shieldDataBubbleAmount;
        RefreshBubbleVisuals();
    }

    public void RequestShieldReset()
    {
        _currentBubbleShieldAmount = 0;
        RefreshBubbleVisuals();
    }

    public void RequestBounceAmountChange(int newBounceAmount)
    {
        _playerProjectile.RequestMaxBounceChange(newBounceAmount);
    }

    public void RequestBounceAmountReset()
    {
        _playerProjectile.RequestMaxBounceReset();
    }
    

    private int FilterDamageThroughBubbleShield(int dmg, GameObject dmgSource)
    {
        if (_currentBubbleShieldAmount > 0)
        {
            var enemyController = dmgSource.GetComponent<EnemyController>();
            if (enemyController)
            {
                enemyController.Die();
            }
            _currentBubbleShieldAmount--;
            RefreshBubbleVisuals();
            SendBubbleChangedEvent(_currentBubbleShieldAmount);
            return dmg - 1;
        }
       
        return dmg;
    }

    private void SendBubbleChangedEvent(int bubbleAmount)
    {
        BubbleShieldChange?.Invoke(bubbleAmount);
    }

    private void RefreshBubbleVisuals()
    {
        if (_currentBubbleShieldAmount > 0)
        {
            ActivateBubbleShieldVisuals();
        }
        else
        {
            DeactivateBubbleShieldVisuals();
            
        }
    }

    private void DeactivateBubbleShieldVisuals()
    {
        _bubbleShieldVisuals.enabled = false;
    }

    private void ActivateBubbleShieldVisuals()
    {
        _bubbleShieldVisuals.enabled = true;
    }
}
