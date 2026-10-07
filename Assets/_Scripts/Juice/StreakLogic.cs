using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreakLogic : MonoBehaviour
{
    [SerializeField] private GameObject _streakIndicator;
    
    private int _streakCount;
    private ActionType _receivedActionType = ActionType.None;
    private bool _receivedBounce;

    public static event Action<int> StreakReached;
    public static event Action BounceKillDetected;

    public static StreakLogic Instance;
    
    [SerializeField] private List<ColorStreakAmountPair> _colorList;
    [Serializable]
    public struct ColorStreakAmountPair
    {
        public Color Color;
        public int StreakAmount;
    }
    
    private enum ActionType
    {
        Shot,
        Kill,
        None
    }

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
        
        EnemyController.EnemyDeathWithLocation      += ReactToEnemyDeath;
        PlayerProjectile.ProjectileShot             += ReactToShot;
        PlayerProjectile.ProjectileReturning        += ReactToReturn;
        PlayerProjectile.ProjectileBounce           += ReactToBounce;
    }

    private void OnDestroy()
    {
        EnemyController.EnemyDeathWithLocation      -= ReactToEnemyDeath;
        PlayerProjectile.ProjectileShot             -= ReactToShot;
        PlayerProjectile.ProjectileReturning        -= ReactToReturn;
        PlayerProjectile.ProjectileBounce           -= ReactToBounce;
    }

    private void ReactToShot()
    {
        _receivedActionType = ActionType.Shot;
        _receivedBounce = false;
    }
    
    private void ReactToBounce()
    {
        _receivedBounce = true;
    }

    private void ReactToReturn()
    {
        if (_receivedActionType == ActionType.Shot)
        {
            EndStreak();
        }
    }

    private void ReactToEnemyDeath(Vector3 pos)
    {
        ExtendStreak(pos);
        _receivedActionType = ActionType.Kill;
    }

    /*private void StartStreak(Vector3 pos)
    {
        SetStreakCount(1);
    }*/

    private void ExtendStreak(Vector3 pos)
    {
        int extensionCount = _streakCount + 1;
        if (_receivedBounce)
        {
            Debug.Log("Bounce kill");
            BounceKillDetected?.Invoke();
            SpawnBounceIndicator(PlayerController.Instance.transform.position + new Vector3(0 , 0.75f, 0));
        }
        
        SetStreakCount(extensionCount);
        
    }

    private void NextStreakStageReached()
    {
        if (_streakCount % 5 == 0 && _streakCount > 0)
        {
            SoundManager.Post(SoundManager.Sound.GainStreak);
            StartCoroutine(StreakChangeColorFlash(GetCurrentStreakColor()));
        }
    }

    private void EndStreak()
    {
        if (_streakCount > 3)
        {
            SoundManager.Post(SoundManager.Sound.LoseStreak);
            SpawnStreakEndedIndicator(PlayerController.Instance.transform.position + new Vector3(0 , 0.75f, 0));
        }
        
        //StartCoroutine(StreakChangeColorFlash(Color.grey));
        SetStreakCount(0);
        
    }

    private void SetStreakCount(int newStreakCount)
    {
        _streakCount = newStreakCount;
        NextStreakStageReached();
        StreakReached?.Invoke(_streakCount);
    }

    private void SpawnStreakEndedIndicator(Vector3 pos)
    {
        GameObject indicator = Instantiate(_streakIndicator, pos, Quaternion.identity);
        indicator.transform.SetParent(GameManager.Instance.transform);
        indicator.transform.position = pos;
        StreakIndicator indicatorScript = indicator.GetComponent<StreakIndicator>();
        indicatorScript.Activate("Streak Over", Color.red);
    }
    
    private void SpawnBounceIndicator(Vector3 pos)
    {
        GameObject indicator = Instantiate(_streakIndicator, pos, Quaternion.identity);
        indicator.transform.SetParent(GameManager.Instance.transform);
        indicator.transform.position = pos;
        StreakIndicator indicatorScript = indicator.GetComponent<StreakIndicator>();
        indicatorScript.Activate("Bounce Kill", GetCurrentStreakColor());
    }

    public int CurrentStreak()
    {
        return _streakCount;
    }

    public List<ColorStreakAmountPair> GetColorList()
    {
        return _colorList;
    }
    
    private IEnumerator StreakChangeColorFlash(Color color)
    {
        ColorsController.Instance.StartGenericColorFlash(0.15f, color);
        yield return new WaitForSeconds(0.25f);
        ColorsController.Instance.StartGenericColorFlash(0.15f, color);
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

    public Color GetPreviousStreakColor()
    {
        return GetColorFromStreak(_streakCount - 4);
    }
    
    public Color GetCurrentStreakColor()
    {
        return GetColorFromStreak(_streakCount);
    }
}
