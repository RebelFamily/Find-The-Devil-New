using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq; // Added for the updated script

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource loopingSfxSource;
    [SerializeField] private bool hapticsEnabled = true;

    private const string HapticsKey = "HapticsEnabled";

    public enum GameSound
    {
        None,
        ButtonClick_Normal,
        ButtonClick_Confirm,
        UIPopupOpen,
        UIPopupClose,
        PlayerFootstep,
        DoorOpen,
        WoodDoorOpen,
        ItemPickup,
        PowerUpCollect,
        Scanner_Activate,
        Scanner_Ping,
        Scanner_Success,
        Scanner_Fail,
        Alien_Footstep,
        Punching_Bag,
        Devil_Laugh,
        NPC_Boxing,
        Gun_Shoot,
        Explosion,
        Impact_Flesh,
        Music_MainMenu,
        Music_InGame_City,
        Music_InGame_Countryside,
        Music_InGame_RescueLevel,
        Music_InGame_MetaBG,
        Music_Victory,
        Music_Defeat,
        Dialogue_Beep,
        Dialogue_Complete,
        WarningAlarm,
        ObjectiveComplete,
        Elevatorbell,
        CarAccident,
        KidsLaugh,
        Hooter,
        MetaEnemyKill,
        Meta_Completed,
        Meta_Building_Appear,
        Elevator_OpenDoors,
        Elevator_CloseDoors,
        Elevator_GoingUP,
        Devil_Dialogue,
        Music_InGame_DiscoLevel,
        Cage_DoorOpen,
        Meta_CoinsSpend,
        Character_Slip,
        Character_Falldown,
        DevilKingFall_Down,
        Phone_Rinning,
        Glass_Breaking,
        Disco_Resscue_Level,
        Jazz_Dinner_BG,
        Night_Level_Audio,
        CoinsOn_Death,
        Fighting,
        GymBG_Boxing,
        Devil_Weeeee,
        Drone,
        HitSound,
        Confettipop,
        LevelCompleteLaugh,
        KingDevilTalk
        
        
    }
    
    public enum GunSounds
    {
        GunSouns_1,
        GunSouns_2,
        GunSouns_3,
        GunSouns_4,
        GunSouns_5,
        GunSouns_6,
        GunSouns_7,
        GunSouns_8,
        GunSouns_9,
        GunSouns_10,
        GunSouns_11,
        GunSouns_12
        
        
    }
 
    [Serializable]
    public struct AudioClipEntry
    {
        public GameSound soundType;
        public AudioClip clip;
    }
    
    // NEW: Struct for GunSounds mapping
    [Serializable]
    public struct GunAudioClipEntry
    {
        public GunSounds soundType;
        public AudioClip clip;
    }

    [Header("Audio Clips (SFX)")]
    [Tooltip("Assign all your Sound Effect clips here and map them to their GameSound type.")]
    [SerializeField] private AudioClipEntry[] sfxClips;

    [Header("Audio Clips (Music/Ambience)")]
    [Tooltip("Assign all your Music and Ambience clips here and map them to their GameSound type.")]
    [SerializeField] private AudioClipEntry[] musicClips;
    
    [Header("Audio Clips (Guns Sound/SFX)")]
    [Tooltip("Assign all your Music and Ambience clips here and map them to their GameSound type.")]
    [SerializeField] private GunAudioClipEntry[] gunsClips; // Changed to use the new GunAudioClipEntry struct
    
    

    private Dictionary<GameSound, AudioClip> _sfxDictionary = new Dictionary<GameSound, AudioClip>();
    private Dictionary<GameSound, AudioClip> _musicDictionary = new Dictionary<GameSound, AudioClip>();
    private Dictionary<GunSounds, AudioClip> _gunDictionary = new Dictionary<GunSounds, AudioClip>(); // NEW: Gun SFX Dictionary
    
    private Coroutine _musicPlaybackCoroutine;

    public void Init()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.spatialBlend = 0;
            sfxSource.playOnAwake = false; 
        }
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.spatialBlend = 0;
            musicSource.playOnAwake = false;
        }
        if (loopingSfxSource == null) 
        {
            loopingSfxSource = gameObject.AddComponent<AudioSource>();
            loopingSfxSource.spatialBlend = 0;
            loopingSfxSource.loop = false; 
            loopingSfxSource.playOnAwake = false;
        }

        _sfxDictionary.Clear(); 
        foreach (var entry in sfxClips)
        {
            if (entry.clip != null)
            {
                if (!_sfxDictionary.ContainsKey(entry.soundType))
                {
                    _sfxDictionary.Add(entry.soundType, entry.clip);
                }
                else
                {
                    Debug.LogWarning($"AudioManager: Duplicate SFX entry for {entry.soundType}. Using the first one found. Please check your sfxClips array.", this);
                }
            }
            else
            {
                Debug.LogWarning($"AudioManager: SFX clip for {entry.soundType} is null. Please assign it in the Inspector.", this);
            }
        }

        _musicDictionary.Clear(); 
        foreach (var entry in musicClips)
        {
            if (entry.clip != null)
            {
                if (!_musicDictionary.ContainsKey(entry.soundType))
                {
                    _musicDictionary.Add(entry.soundType, entry.clip);
                }
                else
                {
                    Debug.LogWarning($"AudioManager: Duplicate Music entry for {entry.soundType}. Using the first one found. Please check your musicClips array.", this);
                }
            }
            else
            {
                Debug.LogWarning($"AudioManager: Music clip for {entry.soundType} is null. Please assign it in the Inspector.", this);
            }
        }
        
        // NEW: Populate Gun SFX Dictionary
        _gunDictionary.Clear();
        foreach (var entry in gunsClips)
        {
            if (entry.clip != null)
            {
                if (!_gunDictionary.ContainsKey(entry.soundType))
                {
                    _gunDictionary.Add(entry.soundType, entry.clip);
                }
                else
                {
                    Debug.LogWarning($"AudioManager: Duplicate Gun SFX entry for {entry.soundType}. Using the first one found. Please check your gunsClips array.", this);
                }
            }
            else
            {
                Debug.LogWarning($"AudioManager: Gun SFX clip for {entry.soundType} is null. Please assign it in the Inspector.", this);
            }
        }

        hapticsEnabled = PlayerPrefs.GetInt(HapticsKey, 1) == 1;
    }

    public void PlaySFX(GameSound soundType)
    {
        if (soundType == GameSound.None) return;

        if (_sfxDictionary.TryGetValue(soundType, out AudioClip clip))
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: SFX clip for '{soundType}' not found in dictionary. Please assign it in the Inspector.", this);
        }
    }
    
    // NEW: Play Gun SFX method
    public void PlayGunSFX(GunSounds soundType)
    {
        if (_gunDictionary.TryGetValue(soundType, out AudioClip clip))
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: Gun SFX clip for '{soundType}' not found in dictionary. Please assign it in the Inspector.", this);
        }
    }


    public void PlaySFX(GameSound soundType, float delay)
    {
        if (soundType == GameSound.None) return;

        if (_sfxDictionary.TryGetValue(soundType, out AudioClip clip))
        {
            if (clip != null && sfxSource != null)
            {
                StartCoroutine(PlaySFXAfterDelay(clip, delay));
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: SFX clip for '{soundType}' not found in dictionary. Please assign it in the Inspector.", this);
        }
    }

    private IEnumerator PlaySFXAfterDelay(AudioClip clip, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        sfxSource.PlayOneShot(clip);
    }

    public void PlayLoopingSFX(GameSound soundType, bool enableLoop)
    {
        if (loopingSfxSource == null)
        {
            return;
        }

        if (enableLoop)
        {
            if (soundType == GameSound.None)
            {
                loopingSfxSource.Stop();
                loopingSfxSource.clip = null;
                return;
            }

            if (_sfxDictionary.TryGetValue(soundType, out AudioClip clip))
            {
                if (clip != null)
                {
                    if (loopingSfxSource.clip != clip || !loopingSfxSource.isPlaying)
                    {
                        loopingSfxSource.clip = clip;
                        loopingSfxSource.loop = true;
                        loopingSfxSource.Play();
                    }
                }
            }
        }
        else
        {
            if (loopingSfxSource.isPlaying)
            {
                loopingSfxSource.Stop();
            }
            loopingSfxSource.clip = null;
        }
    }

    // --- NEW METHOD ---
    /// <summary>
    /// Stops the currently playing looping SFX.
    /// </summary>
    public void StopLoopingSFX()
    {
        if (loopingSfxSource != null && loopingSfxSource.isPlaying)
        {
            loopingSfxSource.loop = false;
            loopingSfxSource.Stop();
            loopingSfxSource.clip = null; // Clear the clip to prevent accidental replays
        }
    }
    // --------------------

    public void PlayMusic(GameSound soundType, bool loop = true)
    {
        // Stop any existing music coroutine to prevent conflicts
        if (_musicPlaybackCoroutine != null)
        {
            StopCoroutine(_musicPlaybackCoroutine);
            _musicPlaybackCoroutine = null;
        }

        if (soundType == GameSound.None)
        {
            musicSource?.Stop(); 
            return;
        }

        if (_musicDictionary.TryGetValue(soundType, out AudioClip clip))
        {
            if (clip != null && musicSource != null)
            {
                if (musicSource.clip != clip || !musicSource.isPlaying)
                {
                    musicSource.clip = clip;
                    musicSource.loop = loop;
                    musicSource.Play();
                }
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: Music clip for '{soundType}' not found in dictionary. Please assign it in the Inspector.", this);
        }
    }
    
    public void PlayMusic(GameSound soundType, float delay, bool loop = true)
    {
        // Stop any existing music coroutine before starting a new one
        if (_musicPlaybackCoroutine != null)
        {
            StopCoroutine(_musicPlaybackCoroutine);
            _musicPlaybackCoroutine = null;
        }

        if (soundType == GameSound.None)
        {
            musicSource?.Stop();
            return;
        }

        if (_musicDictionary.TryGetValue(soundType, out AudioClip clip))
        {
            if (clip != null && musicSource != null)
            {
                // Store the coroutine reference so it can be stopped later
                _musicPlaybackCoroutine = StartCoroutine(PlayMusicAfterDelay(clip, delay, loop));
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: Music clip for '{soundType}' not found in dictionary. Please assign it in the Inspector.", this);
        }
    }

    private IEnumerator PlayMusicAfterDelay(AudioClip clip, float delay, bool loop)
    {
        // Wait for the initial delay
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        // Loop indefinitely if 'loop' is true
        if (loop)
        {
            while (true)
            {
                if (musicSource != null)
                {
                    musicSource.clip = clip;
                    musicSource.loop = false; // Play once per loop
                    musicSource.Play();
                }

                // Wait until the audio clip has finished playing
                while (musicSource != null && musicSource.isPlaying)
                {
                    yield return null;
                }
                
                // Wait for the delay again before replaying
                if (delay > 0)
                {
                    yield return new WaitForSeconds(delay);
                }
            }
        }
        else
        {
            // If not looping, just play once after the initial delay
            if (musicSource != null)
            {
                musicSource.clip = clip;
                musicSource.loop = false;
                musicSource.Play();
            }
        }
    }

    /// <summary>
    /// Stops the music that was started with a delay and loop.
    /// </summary>
    public void StopLoopingMusic()
    {
        if (_musicPlaybackCoroutine != null)
        {
            StopCoroutine(_musicPlaybackCoroutine);
            _musicPlaybackCoroutine = null;
        }
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void StopMusic()
    {
        // Make sure to stop the coroutine as well when stopping music
        if (_musicPlaybackCoroutine != null)
        {
            StopCoroutine(_musicPlaybackCoroutine);
            _musicPlaybackCoroutine = null;
        }
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void StopSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }

    public void ToggleHaptics(bool enable)
    {
        hapticsEnabled = enable;
        PlayerPrefs.SetInt(HapticsKey, hapticsEnabled ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Haptics toggled to: {hapticsEnabled}");
    }

    public void TriggerHapticFeedback()
    {
        if (hapticsEnabled)
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate(); 
#endif
            Debug.Log("Haptic feedback triggered (if platform supports it).");
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(volume);
        if (loopingSfxSource != null) loopingSfxSource.volume = Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = Mathf.Clamp01(volume);
    }
}