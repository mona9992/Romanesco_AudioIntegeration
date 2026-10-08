using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileJuice : MonoBehaviour
{
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _shakeMagnitude;
    [SerializeField] private float _freezeFrameDuration;
    
    [SerializeField] private ParticleSystem _explosionParticle;
    [SerializeField] private ParticleSystem _sparkParticle;
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Color _startingColor;
    
    private GameManager _gameManager;
    private CameraManager _cameraManager;

    private Animator _playerShieldAnimator;
    
    // Rider efficiency proposals
    private static readonly int IdleAnimHash = Animator.StringToHash("Idle");
    private static readonly int ShieldAnimHash = Animator.StringToHash("Shield");

    [SerializeField] private Color _inactiveColor;
    
    [SerializeField] private List<StreakLogic.ColorStreakAmountPair> _colorList;

    private void Awake()
    {
        StreakLogic.StreakReached += UpdateTrailColorFromStreak;
    }
    
    private void OnDestroy()
    {
        StreakLogic.StreakReached -= UpdateTrailColorFromStreak;
    }

    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _cameraManager = Camera.main.GetComponent<CameraManager>();
        _playerShieldAnimator = GetComponent<Animator>();
        _startingColor = _spriteRenderer.color;
        _colorList = StreakLogic.Instance.GetColorList();
    }

    public void ExplosionEffect(Vector2 pos)
    {
        ParticleSystem ps = Instantiate(_explosionParticle, pos, Quaternion.identity) as ParticleSystem;
        ps.Play();
        Destroy(ps.gameObject, 0.2f);
        
        ColorsController.Instance.StartProjectileColorFlash();
        
        SoundManager.Post(SoundManager.Sound.PlayerProjectileHit, gameObject);
    }

    public void SparkEffect(Vector2 pos, Vector2 dir)
    {
        ParticleSystem ps = Instantiate(_sparkParticle, pos, Quaternion.identity) as ParticleSystem;
        ps.transform.up = dir;
        ps.Play();
        Destroy(ps.gameObject, 0.2f); 
        
    }

    public void CameraShake()
    {
        StartCoroutine(_cameraManager.Shake(_shakeDuration, _shakeMagnitude));
    }
    
    public void ShieldOpenAnim()
    {
        _playerShieldAnimator.SetTrigger(ShieldAnimHash);
    }

    public void ShieldCloseAnim()
    {
        _playerShieldAnimator.SetBool(ShieldAnimHash, false);
    }

    public void IdleAnim()
    {
        _playerShieldAnimator.SetTrigger(IdleAnimHash);
    }

    public void FreezeFrames()
    {
        _cameraManager.FreezeFrames(_freezeFrameDuration);
    }

    private void UpdateTrailColorFromStreak(int streakAmount)
    {
        _trailRenderer.startColor = GetColorFromStreak(streakAmount);
    }
    
    private Color GetColorFromStreak(int streakAmount)
    {
        for ( int i = _colorList.Count - 1; i > 0; i--)
        {
            if (streakAmount >= _colorList[i].StreakAmount)
            {
                return _colorList[i].Color;
            }
        }

        return _colorList[0].Color;
    }

    public void ReturnVFX()
    {
        SetSpriteAlpha(0.2f);
        _trailRenderer.startColor = _inactiveColor;
    }

    public void ArrivalVFX()
    {
        ResetColor();
        UpdateTrailColorFromStreak(StreakLogic.Instance.CurrentStreak());
    }
    
    private void SetSpriteAlpha(float alpha)
    {
        Color currentColor = _spriteRenderer.color;
        _spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
    }

    private void ResetColor()
    {
        _spriteRenderer.color = _startingColor;
    }
    
    private void ChangeBulletColor(Color color)
    {
        _spriteRenderer.color = color;
    }

}
