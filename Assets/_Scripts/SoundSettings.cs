using System;
using System.Collections.Generic;
using UnityEngine;


public class SoundSettings : MonoBehaviour
{
    
    private const string MusicVolumeID = "MusicVolume";
    private const string SfxVolumeID = "SFXVolume";

    [SerializeField] private List<MixerVolumeDict> _musicDict;
    [SerializeField] private List<MixerVolumeDict> _sfXDict;

    [SerializeField] private BarDisplay _sfxDisplay;
    [SerializeField] private BarDisplay _musicDisplay;
    
    [Serializable]
    private struct MixerVolumeDict
    {
        public int VolumeAmount;
        public float BusAmount;
    }
    
    private int _currentMusicAmount;
    private int _currentSfxAmount;

    [SerializeField] private int _maxAmount;
    
    private void Start()
    {
        _currentMusicAmount = GetAmountFromVolume(_musicDict, GetMusicVolume());
        _currentSfxAmount = GetAmountFromVolume(_sfXDict, GetSfxVolume());
        
        _musicDisplay.Initialize(_currentMusicAmount);
        _sfxDisplay.Initialize(_currentSfxAmount);
    }

    public void IncreaseMusicVolume()
    {
        if (_currentMusicAmount >= _maxAmount)
        {
            return;
        }
        
        _currentMusicAmount++;
        SetMusicVolume(GetVolumeFromAmount(_musicDict, _currentMusicAmount));
        
        _musicDisplay.SetListFromAmountWithAnim(_currentMusicAmount);
    }

    public void DecreaseMusicVolume()
    {
        if (_currentMusicAmount < 1)
        {
            return;
        }
        
        _currentMusicAmount--;
        SetMusicVolume(GetVolumeFromAmount(_musicDict, _currentMusicAmount));
        
        _musicDisplay.SetListFromAmountWithAnim(_currentMusicAmount);
    }
    
    public void IncreaseVfxVolume()
    {
        if (_currentSfxAmount >= _maxAmount)
        {
            return;
        }
        
        _currentSfxAmount++;
        SetSfxVolume(GetVolumeFromAmount(_sfXDict, _currentSfxAmount));
        
        _sfxDisplay.SetListFromAmountWithAnim(_currentSfxAmount);
    }

    public void DecreaseVfxVolume()
    {
        if (_currentSfxAmount < 1)
        {
            return;
        }
        
        _currentSfxAmount--;
        SetSfxVolume(GetVolumeFromAmount(_sfXDict, _currentSfxAmount));
        
        _sfxDisplay.SetListFromAmountWithAnim(_currentSfxAmount);
    }

    private float GetVolumeFromAmount(List<MixerVolumeDict> map, int amount)
    {
        foreach (var data in map)
        {
            if (data.VolumeAmount == amount)
            {
                return data.BusAmount;
            }
        }

        return -1;
    }

    private int GetAmountFromVolume(List<MixerVolumeDict> map, float volume)
    {
        foreach (var data in map)
        {
            if (Math.Abs(data.BusAmount - volume) < 0.01f)
            {
                return data.VolumeAmount;
            }
        }

        return -1;
    }

    private void SetMusicVolume(float newValue)
    {
        SoundAssets.i.MusicBusVolume.SetGlobalValue(newValue);
    }
    
    private void SetSfxVolume(float newValue)
    {
        SoundAssets.i.SFXBusVolume.SetGlobalValue(newValue);
    }

    private float GetMusicVolume()
    {
        float returnVal = SoundAssets.i.MusicBusVolume.GetGlobalValue();
        return returnVal;
    }
    
    private float GetSfxVolume()
    {
        float returnVal = SoundAssets.i.SFXBusVolume.GetGlobalValue();
        return returnVal;
    }
}
