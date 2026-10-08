using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public static class SoundManager 
{
    public enum Sound {
        PlayerProjectileFire,
        PlayerProjectileHit,
        PlayerDeath,
        EnemyShoot,
        EnemyMove,
        EnemyDeath,
        StartGame,
        Pause,
        Resume,
        ButtonSelect,
        Music,
        LoseStreak,
        GainStreak,
        SceneLoad,
        LoginPass,
        LoginFail,
        Portal,
        Ranking,
        Shuffle,
        EnemyStop,
    }
    

    public static void Post(Sound sound, GameObject emitter = null)
    {
        AK.Wwise.Event wwiseEvent = GetEvent(sound);
        if (wwiseEvent != null)
            wwiseEvent.Post(emitter != null ? emitter : SoundAssets.i.gameObject);
    }

    public static void Stop(Sound sound, GameObject emitter)
    {
        AK.Wwise.Event wwiseEvent = GetEvent(sound);
        if (wwiseEvent != null && emitter != null)
        {
            wwiseEvent.Post(emitter);
        }
    }
    private static AK.Wwise.Event GetEvent(Sound sound)
    {
        foreach (SoundAssets.SoundEvent entry in SoundAssets.i.soundEvents)
        {
            if (entry.sound == sound && entry.wwiseEvent.IsValid())
            {
                return entry.wwiseEvent;
            }
        }
        Debug.LogError("Sound" + sound + " not found");
        return null;
    }
    public static void StopAllPlayingSounds()
    {
        AkUnitySoundEngine.StopAll();
    }

    public static void PauseAllPlayingSounds()
    {
        Post(Sound.Pause); 
    }
    
    
    public static void ResumeAllPlayingSounds()
    {
        Post(Sound.Resume);
    }
    public static void Reset()
    {
        StopAllPlayingSounds();
        
    }
    

}
