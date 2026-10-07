using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedalProvider : MonoBehaviour
{

    [SerializeField] private GameObject _medalPrefab; 
    [SerializeField] private List<MedalInfo> _medalInfos;
    [SerializeField] private List<MedalThresholds> _medalThresholds;
    
    private Dictionary<MedalType, MedalInfo> _medalInfosDict = new Dictionary<MedalType, MedalInfo>();
    public static MedalProvider Instance;
    
    public enum MedalType
    {
        SP,
        S,
        A,
        B,
        C,
        D,
        None
    }

    public IEnumerator SpawnMedalList(Vector3 position, float delay, float yDistance, List<GameObject> textObjects)
    {
        SoundManager.Post(SoundManager.Sound.Ranking);
        int count = 0;
        foreach (var medalType in CalculateMedalTypes())
        {
            SpawnMedal(medalType, position + Vector3.down * (yDistance * count), textObjects[count]);
            yield return new WaitForSeconds(delay);
            count++;
        }
    }

    [Serializable]
    public struct MedalInfo
    {
        public MedalType Type;
        public string Letter;
        public Color Color;
    }
    
    [Serializable]
    public struct MedalThresholds
    {
        public string MedalName;
        public List<MedalThresholdSection> Thresholds;
    }
    
    [Serializable]
    public struct MedalThresholdSection
    {
        
        public float LowerBorder;
        public MedalType Type;
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
    }

    private void Start()
    {
        CreateMedalInfoDict();
    }

    private void CreateMedalInfoDict()
    {
        foreach (var medalInfo in _medalInfos)
        {
            _medalInfosDict.Add(medalInfo.Type, medalInfo);
        }
    }
    
    private void SpawnMedal(MedalType medalType, Vector3 position, GameObject textObject)
    {
        GameObject medal = Instantiate(_medalPrefab, position, Quaternion.identity);
        medal.GetComponent<MedalLogic>().SetupMedal(_medalInfosDict[medalType], textObject);
        medal.GetComponent<MedalLogic>().ActivateMedal(position);
    }

    private MedalType GetTypeFromThresholdsList(MedalThresholds thresholds, float value)
    {
        for (int i = 0; i < thresholds.Thresholds.Count; i++)
        {
            if (value >= thresholds.Thresholds[i].LowerBorder)
            {
                return thresholds.Thresholds[i].Type;
            }
        }

        return MedalType.None;
    }
    
    private List<MedalType> CalculateMedalTypes()
    {
        StatisticManager.Statistics totalStats = StatisticManager.Instance.GetStatistics();
        float shotAccuracy  = totalStats.GetShotAccuracy();
        float highestStreak = totalStats.HighestStreak;
        float bounceKills   = totalStats.BounceKills;
        
        var medalTypes = new List<MedalType>
        {
            GetTypeFromThresholdsList(_medalThresholds[0], shotAccuracy),
            GetTypeFromThresholdsList(_medalThresholds[1], highestStreak),
            GetTypeFromThresholdsList(_medalThresholds[2], bounceKills)
        };
        return medalTypes;
    }
}
