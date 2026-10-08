using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ProjectileJuice))]
public class PlayerProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float _flyRange;
    [SerializeField] private float _flySpeed;
    [SerializeField] private float _returnSpeed;
    [SerializeField] private float _maxBounceAmount;
    private float _startMaxBounceAmount;

    private float _startFlyRange;

    [Header("Shield Settings")] 
    [SerializeField] private float _shieldDistanceToPlayer;

    // Get set in start
    private Rigidbody2D _rigidBody2D;
    private CircleCollider2D _circleCollider2D;
    private PlayerController _playerController;
    private Camera _mainCam;
    
    private ProjectileJuice _projectileJuice;
    private GameObject _shieldColliderGameObject;
    
    // used internally
    private float _distanceTravelled;
    private int _currentBounceCount;
    private Vector2 _moveDirectionBeforeBounce;
    private bool _bouncedLastFrame;
    private bool _bouncedThisFrame;
    
    private ProjectileState _state = ProjectileState.Shield;

    // Seems to work consistently but should be remade
    private const float SetbackDistanceForCollisionRayCast = 1f;

    public static event Action ProjectileShot;
    public static event Action ProjectileReturning;
    public static event Action ProjectileBounce;
    
    public bool IsShielding()
    {
        return _state is ProjectileState.Shield;
    }
    
    private enum ProjectileState
    {
        Flying,
        Returning,
        Idle,
        Shield
    }
    
    private void Start()
    {

        _rigidBody2D = GetComponent<Rigidbody2D>();
        _rigidBody2D.linearVelocity = Vector2.zero;

        _circleCollider2D = GetComponent<CircleCollider2D>();
        _projectileJuice = GetComponent<ProjectileJuice>();
        _shieldColliderGameObject = GetComponentInChildren<PlayerShieldColliderLogic>().gameObject;
        _shieldColliderGameObject.SetActive(false);

        _playerController = transform.parent.GetComponent<PlayerController>();
        _mainCam = Camera.main;

        _state = ProjectileState.Idle;

        _startFlyRange = _flyRange;
        _startMaxBounceAmount = _maxBounceAmount;
    }
    
    private void Update()
    {
        switch (_state)
        {
            case ProjectileState.Flying:
                CountDistance();
                CheckFlyingProgress();
                break;
            case ProjectileState.Returning:
                ReturnProjectile();
                break;
            case ProjectileState.Idle:
                PositionShieldAroundPlayer();
                break;
            case ProjectileState.Shield:
                PositionShieldAroundPlayer();
                break;
        }
    }

    private void LateUpdate()
    {
        // TODO: Try at a workaround needs to be continued -> waiting for fresh eyes tomorrow
        if (_state == ProjectileState.Flying)
        {
            PreventBounceGlitchingThroughWall();
        }
    }

    // TODO: fix corner skipping properly
    private void PreventBounceGlitchingThroughWall()
    {
        if (!_bouncedLastFrame && !_bouncedThisFrame) return;
        if (_bouncedThisFrame)
        {
            _bouncedThisFrame = false;
            _bouncedLastFrame = true;
            return;
        }
        _bouncedLastFrame = false;
        _bouncedThisFrame = false;
        
        
        if (IsProjectileInWall())
        {
            //SetProjectileVelocityAndDirection(_moveDirectionBeforeBounce * (_flySpeed * -1f));
        }
    }

    private void CheckFlyingProgress()
    {
        if (_state != ProjectileState.Flying) return;
        
        if (IsBulletOutsideRoom())
        {
            EndFlight();
            return;
        }

        if (_distanceTravelled > _flyRange)
        {
            EndFlight();
        }
    }

    private Vector3 GetShieldDirection()
    {
        // Shield positioning 
        Vector3 shieldDirection = _mainCam.ScreenToWorldPoint(Input.mousePosition) - _playerController.transform.position;
        shieldDirection = new Vector3(shieldDirection.x, shieldDirection.y, 0).normalized;
        return shieldDirection;
    }

    private void PositionShieldAroundPlayer()
    {
        if (GameStateManager.Instance.IsRunning())
        {
            transform.position = _playerController.transform.position + (GetShieldDirection() * _shieldDistanceToPlayer);
            transform.up = GetShieldDirection();
        }
    }

    private void EndFlight()
    {
        _rigidBody2D.linearVelocity = Vector2.zero;
        _state = ProjectileState.Returning;
        _currentBounceCount = 0;
        ProjectileReturning?.Invoke();
    }
    
    private void CountDistance()
    {
        if (_state == ProjectileState.Flying)
        {
            _distanceTravelled += _rigidBody2D.linearVelocity.magnitude * Time.deltaTime;
        }
    }
    
    public void RequestShootProjectile()
    {
        if (IsShootingAllowed())
        {
            ShootProjectile();

        }
    }
    
    private bool IsShootingAllowed()
    {
        return _state is ProjectileState.Idle && !IsProjectileInWall();
    }

    private bool IsProjectileInWall()
    {
        Vector2 colliderPos = transform.position + (Vector3)_circleCollider2D.offset;
        Collider2D[] results = Physics2D.OverlapCircleAll(colliderPos, _circleCollider2D.radius);
        foreach (var hitCollider in results)
        {
            if (hitCollider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }

    private void ShootProjectile()
    {
        ProjectileShot?.Invoke();
        
        Vector3 shieldDirection = GetShieldDirection();
        
        SetProjectileVelocityAndDirection(shieldDirection * _flySpeed);
        
        _state = ProjectileState.Flying;
        _distanceTravelled = 0;
        SoundManager.Post(SoundManager.Sound.PlayerProjectileFire, gameObject);
        //_fireProjectileSoundEvent.Post(gameObject);
        ShootVFX();
        
        KillCurrentIntersectingEnemies();
    }


    private void KillCurrentIntersectingEnemies()
    {
        var overlaps = Physics2D.OverlapCircleAll(transform.position, _circleCollider2D.radius * 1.5f).ToList();
        foreach (var collision in overlaps)
        {
            if (collision.CompareTag("Enemy"))
            {
                Debug.Log("INTERSECTING ENEMY KILLED");
                RequestEnemyHit(collision.transform);
            }
        }
    }
    
    private void ShootVFX()
    {
        _projectileJuice.ShieldCloseAnim();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (_state)
        {
            // Ignore any collision while returning
            case ProjectileState.Returning:
                return;
            case ProjectileState.Flying:
            {
                if (other.gameObject.CompareTag("Wall"))
                {
                    RequestBounce();
                }
                if (other.gameObject.CompareTag("Enemy"))
                {
                    RequestEnemyHit(other.transform);
                }
                break;
            }
        }
    }

    private Vector2 GetNormalToClosestWall()
    {
        // Let the raycast start a bit behind the point of collision to prevent it starting inside of the collider returning wrong normals
        Vector2 rayCastStartPosition = transform.position - (transform.up * (SetbackDistanceForCollisionRayCast));
        RaycastHit2D hit = Physics2D.Raycast(rayCastStartPosition, transform.up);
        if (hit)
        {
            return hit.normal;
        }
        return Vector2.zero;
    }

    private bool RequestBounce()
    { 
        BounceVFX(transform.position);
        
        if (IsBounceAllowed())
        {
            Bounce(GetNormalToClosestWall());
            return true;
        }
        
        EndFlight();
        return false;
    }
    
    private void Bounce(Vector2 normal)
    {
        _moveDirectionBeforeBounce = _rigidBody2D.linearVelocity.normalized;
        _bouncedThisFrame = true;
        
        ProjectileBounce?.Invoke();
        
        Vector2 newDirection = Vector2.Reflect(_rigidBody2D.linearVelocity.normalized, normal).normalized;
        SetProjectileVelocityAndDirection( newDirection * _flySpeed);
        _currentBounceCount++;
    }

    private void BounceVFX(Vector2 pos)
    {
        _projectileJuice.ExplosionEffect(pos);
        _projectileJuice.CameraShake();
    }

    private void SetProjectileVelocityAndDirection(Vector2 velocity)
    {
        _rigidBody2D.linearVelocity = velocity;
        transform.up = velocity.normalized;
    }
    
    private bool IsBounceAllowed()
    {
        return _currentBounceCount + 1 < _maxBounceAmount;
    }

    private void ReturnProjectile()
    {
        // Code to teleport bullet back instead of flying
        /*PositionShieldAroundPlayer();   
        EndReturn();*/
        
        SetProjectileVelocityAndDirection(GetReturnDirection() * _returnSpeed);
        
        _projectileJuice.ReturnVFX();
        if (HasProjectileReachedPlayer())
        {
            EndReturn();
        }
    }

    private bool HasProjectileReachedPlayer()
    {
        return Vector2.Distance(transform.position, _playerController.transform.position) <= _shieldDistanceToPlayer;
    }

    private Vector2 GetReturnDirection()
    {
        return (_playerController.transform.position - transform.position).normalized;
    }

    private void EndReturn()
    {
        _state = ProjectileState.Idle;
        SetProjectileVelocityAndDirection(Vector2.zero);

        _projectileJuice.ArrivalVFX();
    }
    private void RequestEnemyHit(Transform enemyTransform)
    {
        EnemyController enemyController = enemyTransform.GetComponent<EnemyController>();
        enemyController.Die();
        EnemyHitVFX(enemyTransform.transform.position);
        EndFlight();
    }
    
    private void EnemyHitVFX(Vector2 pos)
    {
        _projectileJuice.ExplosionEffect(pos);
        _projectileJuice.CameraShake();
        _projectileJuice.FreezeFrames();
    }

    public void ShieldHit(Collider2D collision)
    {
        ShieldHitVFX(collision);
        Destroy(collision.gameObject);
    }

    private void ShieldHitVFX(Collider2D collision)
    {
        _projectileJuice.SparkEffect(collision.transform.position,  -1 * collision.transform.right);
    }

    private bool IsBulletOutsideRoom()
    {
        Vector2 bounds = GameManager.Instance.GetRoomBounds();
        Vector3 roomPoint = CameraManager.Instance.GetCurrentCamGoal();
        Vector3 dist = roomPoint - transform.position;
        float xDistance = Mathf.Abs(dist.x);
        float yDistance = Mathf.Abs(dist.y);
        
        return xDistance > bounds.x || yDistance > bounds.y;
    }

    public void RequestOpenShield()
    {
        if (_state == ProjectileState.Idle)
        {
            OpenShield();
        }
    }
    
    private void OpenShield()
    {
        
        _state = ProjectileState.Shield;
        _shieldColliderGameObject.SetActive(true);
       // SoundManager.PlaySound(SoundManager.Sound.PlayerShieldOpen,this.transform, true, SoundManager.SoundType.SFX);
        _projectileJuice.ShieldOpenAnim();
        
    }

    public void RequestCloseShield()
    {
        if (_state == ProjectileState.Shield)
        {
            CloseShield();
        }
    }

    private void CloseShield()
    {
        _state = ProjectileState.Idle;
        _projectileJuice.IdleAnim();
        _shieldColliderGameObject.SetActive(false);
        //SoundManager.StopSound(SoundManager.Sound.PlayerShieldOpen, this.transform);
    }
    

    public void RequestRangeChange(float rangeChange)
    {
        _flyRange += rangeChange;
    }

    public void RequestRangeReset()
    {
        _flyRange = _startFlyRange;
    }

    public void RequestMaxBounceChange(float newMaxBounceAmount)
    {
        _maxBounceAmount = newMaxBounceAmount;
    }

    public void RequestMaxBounceReset()
    {
        _maxBounceAmount = _startMaxBounceAmount;
    }
}
