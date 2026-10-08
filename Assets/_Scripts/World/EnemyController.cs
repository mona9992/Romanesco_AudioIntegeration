using System;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyController : MonoBehaviour
{

    public enum EnemyType
    {
        Stationary,
        Melee,
        Patrol
    }

    public EnemyType enemyMode;
    public float speed = 10f;
    public float rotationSpeed = 10f;
    //public AudioClip meleeAttackSound;
    private bool isSoundPlaying= false;


    [Space(20)]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private bool isPatrollingForward = true;


    [Space(10)]
    public bool lookAtPlayer = true; // To indicate if the enemy should look at player; also for future to indicate if player is in enemy range.


    public float bulletTimeIntervals = 1f; // Seconds to wait between each enemy bullet shot.
    private bool shootTimedown = false;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;

    public int _scoreToAddOnDeath = 10;

    private float time;
    private Transform player;
    private GameManager gm;

    public float detectionRadius = 10f;
    public LayerMask obstacleLayer;

    private PlayAudio pa;
    private Rigidbody2D rb;

    public static event Action EnemyDeath;
    public static event Action<Vector3> EnemyDeathWithLocation;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        gm = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        pa = GetComponent<PlayAudio>();
    }

    // Update is called once per frame
    void Update()
    {
        if (! GameStateManager.Instance.IsRunning())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Calculate the direction towards the player
        Vector2 directionToPlayer = player.position - transform.position;

        // Perform a raycast to check for obstacles between the enemy and the player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRadius, obstacleLayer);

        // Check if the raycast hit the player
        if (hit.collider != null)
        {
            if (hit.collider.gameObject == player.gameObject || hit.transform.CompareTag("Shield"))
            {
                lookAtPlayer = true;
            }
            else
            {
                lookAtPlayer = false;
            }
        }

        switch (enemyMode)
        {
            case EnemyType.Melee when lookAtPlayer:
            {
                Vector3 direction = ( player.position - transform.position).normalized;
                rb.linearVelocity = direction * speed;
                if(!isSoundPlaying){
                    
                    SoundManager.Post(SoundManager.Sound.EnemyMove, this.gameObject);
                    isSoundPlaying = true;
                }
                
                break;
            }
            case EnemyType.Melee:
            {
                rb.linearVelocity = Vector3.zero;
                if(isSoundPlaying){
                    
                SoundManager.Stop(SoundManager.Sound.EnemyStop, this.gameObject);
                isSoundPlaying = false;
                
                }

                break;
            }
            case EnemyType.Stationary:
            {
                Vector3 direction = (player.position - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Rotate the object around the Z-axis
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                time += Time.deltaTime;
                if (!shootTimedown)
                {
                    BasicShoot();
                }

                if (time > bulletTimeIntervals)
                {
                    time = 0;
                    shootTimedown = false;
                }
                break;
            }
        }
    }

    public void BasicShoot() 
    {       
        SoundManager.Post(SoundManager.Sound.EnemyShoot, gameObject);
        GameObject newBullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        shootTimedown = true; // Flag to indicate there should be timedown between each shot.
    }

    public void Die() 
    {
        EnemyDeath?.Invoke();
        EnemyDeathWithLocation?.Invoke(transform.position);
        if(isSoundPlaying)
        {
                SoundManager.Stop(SoundManager.Sound.EnemyStop, this.gameObject);
                isSoundPlaying = false;
        }

        SoundManager.Post(SoundManager.Sound.EnemyDeath);
        //pa.PlayOneShotSound(1);
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player")) 
        {
            collision.gameObject.GetComponent<PlayerController>().Damage(5, gameObject);
        }
    }


}
