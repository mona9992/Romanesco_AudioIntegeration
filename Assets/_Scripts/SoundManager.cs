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
        PlayerShieldOpen,
        PlayerShieldHit,
        PlayerDeath,
        EnemyShoot,
        EnemyMove,
        EnemyDeath,
        Interactable,
        StartGame,
        Pause,
        GameOver,
        ButtonSelect,
        Music,
        LoseStreak,
        GainStreak,
        LoginPass,
        LoginFail,
        Portal,
        Ranking,
        Shuffle
    }

    public enum SoundType {
        Music,
        SFX
    }
    
    private static Dictionary<Sound, float> _soundTimer;
    private static GameObject _oneShotSoundGameObject;
    private static AudioSource _oneShotAudioSource;
    private static List<GameObject> _playingSounds;

    private static GameObject _musicGameObject;
    
    private static bool _sfxAllowed = true;

    public static void Initialize(){
        _soundTimer = new Dictionary<Sound, float>
        {
            [Sound.EnemyMove] = 0f
        };

    }

    public static void PlayOneShotSound(Sound sound, Vector3 position)
    {
        
        if (!CanPlaySound(sound))
        {
            return;
        }
        
        GameObject soundGameObject = new GameObject("3DSound")
        {
            transform =
            {
                position = position
            }
        };
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.clip = GetAudioClip(sound);
        audioSource.maxDistance = 100f;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0;
        audioSource.Play();
        Object.Destroy(soundGameObject, audioSource.clip.length);
    }

    public static void PlayOneShotSound(Sound sound){

        
        if (_oneShotSoundGameObject == null)
        {
             _oneShotSoundGameObject = new GameObject("OneShotSound");
             _oneShotAudioSource = _oneShotSoundGameObject.AddComponent<AudioSource>();
             _oneShotAudioSource.outputAudioMixerGroup = SoundAssets.i.Mixer_SFX;
        }
        if (CanPlaySound(sound))
        {
            _oneShotAudioSource.PlayOneShot(GetAudioClip(sound));
        }
    }
    

    private static GameObject CreateAndActivateSoundGameObject(Sound sound, Transform parent, bool isLooping, SoundType type, bool isRandom)
    {
        GameObject soundGameObject = new GameObject(sound.ToString())
        {
            transform = { parent = parent }
        };
        
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = GetAudioMixerGroup(type);
        audioSource.loop = isLooping;
        if(isRandom)
        audioSource.clip = GetRandomAudioClip(sound);
        else
        audioSource.clip = GetAudioClip(sound);
        audioSource.Play();

        return soundGameObject;
    }
    private static AudioMixerGroup GetAudioMixerGroup (SoundType type)
    {
        switch(type){
            default:
                return SoundAssets.i.Mixer_Master;
            case SoundType.Music:
                return SoundAssets.i.Mixer_Music;
            case SoundType.SFX:
                return SoundAssets.i.Mixer_SFX;
    }
    }
    public static void PlaySound(Sound sound, Transform parent, bool isLooping, SoundType type)
    {
        if (_playingSounds == null)
        {
            _playingSounds = new List<GameObject>();
        }
        
        _playingSounds.Add(CreateAndActivateSoundGameObject(sound, parent, isLooping, type,false));
    }
    public static void PlayRandomSound(Sound sound, Transform parent, bool isLooping, SoundType type)
    {
        if (_playingSounds == null)
        {
            _playingSounds = new List<GameObject>();
        }
        AudioClip clip = GetRandomAudioClip(sound); 
        _playingSounds.Add(CreateAndActivateSoundGameObject(sound, parent, isLooping, type,true));
    }
    
    public static void StopSound(Sound sound, Transform parent)
    {
        if (_playingSounds is not { Count: > 0 })
        {
            return;
        }
        foreach(GameObject soundGameObject in _playingSounds)
        {
            if(soundGameObject.name == sound.ToString() && soundGameObject.transform.parent == parent)
            {
                soundGameObject.GetComponent<AudioSource>().Stop(); 
                Object.Destroy(soundGameObject);
                _playingSounds.Remove(soundGameObject);
                return;
            }    
        }
    }
    
    private static bool CanPlaySound(Sound sound)
    {
        switch(sound){
            default:
                return true;
            case Sound.EnemyMove:
                if(_soundTimer.ContainsKey(sound))
                {
                    float lastTimePlayed = _soundTimer[sound];
                    float enemyMoveTimerMax = 0.5f;
                    if( lastTimePlayed + enemyMoveTimerMax < Time.time)
                    {
                        _soundTimer[sound] = Time.time;
                        return true;
                    }
                    return false;
                }
                return true;
        }
    }
    
    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach(SoundAssets.SoundAudioClip soundAudioClip in SoundAssets.i.soundAudioClips)
        {
            if(soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip;
            } 
        }
        Debug.LogError("Sound" + sound + " not found");
        return null;

    }
    private static AudioClip GetRandomAudioClip(Sound sound)
    {
        foreach(SoundAssets.SoundAudioClips soundAudioClipList in SoundAssets.i.soundAudioClipsList)
        {
            if(soundAudioClipList.sound == sound && soundAudioClipList.audioClips.Length>0)
            {
                int randomIndex = Random.Range( 0, soundAudioClipList.audioClips.Length );

                return soundAudioClipList.audioClips[randomIndex];
            } 
        }
        Debug.LogError("Sound" + sound + " not found");
        return null;

    }
    public static void PlayRandomOneShot(Sound sound)
    {

        if (_oneShotSoundGameObject == null)
        {
             _oneShotSoundGameObject = new GameObject("OneShotSound");
             _oneShotAudioSource = _oneShotSoundGameObject.AddComponent<AudioSource>();
             _oneShotAudioSource.outputAudioMixerGroup = SoundAssets.i.Mixer_SFX;
        }
        _oneShotAudioSource.PlayOneShot(GetRandomAudioClip(sound));

    }
    
    public static void StopAllPlayingSounds()
    {
        if (_playingSounds is not { Count: > 0 })
        {
            return;
        }
        foreach(GameObject soundGameObject in _playingSounds)
        {
            if(soundGameObject!=null) 
            {
                soundGameObject.GetComponent<AudioSource>().Stop(); 
                Object.Destroy(soundGameObject);
            }
        }
        
        Reset();
    }
    
    public static void PauseAllPlayingSounds()
    {
        if(_playingSounds is { Count: > 0 })
        {
            foreach(GameObject soundGameObject in _playingSounds)
            {
                if (soundGameObject != null)
                {
                    soundGameObject.GetComponent<AudioSource>().Pause(); 
                }
            }
        }
    }
    
    public static void ResumeAllPlayingSounds()
    {
        if(_playingSounds is { Count: > 0 })
        {
            foreach(GameObject soundGameObject in _playingSounds)
            {
                if (soundGameObject != null)
                {
                    soundGameObject.GetComponent<AudioSource>().Play(); 
                }
            }
        }
    }
    
    public static void Reset()
    {
        _playingSounds?.Clear();
    }



    

}
