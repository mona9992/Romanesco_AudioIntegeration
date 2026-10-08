using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundAssets : MonoBehaviour
{
    private static SoundAssets _i;

    public static SoundAssets i{
        get {
            if (_i == null)
            {
                _i =Instantiate(Resources.Load<SoundAssets>("SoundAssets"));
            }
            return _i; 
            }
    }
    
    public AK.Wwise.RTPC MusicBusVolume;
    public AK.Wwise.RTPC SFXBusVolume;
    public SoundEvent[] soundEvents;

    
    [System.Serializable]
    public class SoundEvent{
        public SoundManager.Sound sound;
        public AK.Wwise.Event wwiseEvent;
    }

}
