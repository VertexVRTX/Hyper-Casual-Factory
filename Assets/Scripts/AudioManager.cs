using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct SoundEffect
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
    [Range(0f, 0.3f)] public float pitchRandomness;

    public SoundEffect(AudioClip clip, float volume = 1f, float pitchRandomness = 0.05f)
    {
        this.clip = clip;
        this.volume = volume;
        this.pitchRandomness = pitchRandomness;
    }
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("UI Sounds")]
    public SoundEffect buttonClickSound = new SoundEffect(null, 1f);
    public SoundEffect freezeAbilitySound = new SoundEffect(null, 1f);
    public SoundEffect timerWarningSound = new SoundEffect(null, 0.8f);
    public SoundEffect winSound = new SoundEffect(null, 1f);
    public SoundEffect loseSound = new SoundEffect(null, 1f);

    [Header("Box Interaction Sounds")]
    public SoundEffect correctSortSound = new SoundEffect(null, 1f);
    public SoundEffect wrongSortSound = new SoundEffect(null, 1f);
    public SoundEffect boxDropOnConveyorSound = new SoundEffect(null, 0.6f);
    public SoundEffect tapeUnwrapSound = new SoundEffect(null, 1f);
    public SoundEffect iceMeltSound = new SoundEffect(null, 1f);
    public SoundEffect glassBreakSound = new SoundEffect(null, 0.9f);

    [Header("Bonus Sounds")]
    public SoundEffect bonusScoreSound = new SoundEffect(null, 1f);
    public SoundEffect bonusTimeSound = new SoundEffect(null, 1f);

    private AudioSource _audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlaySFX(SoundEffect sfx)
    {
        if (sfx.clip == null || _audioSource == null) return;

        float originalPitch = _audioSource.pitch;

        if (sfx.pitchRandomness > 0f)
        {
            _audioSource.pitch = 1f + Random.Range(-sfx.pitchRandomness, sfx.pitchRandomness);
        }

        _audioSource.PlayOneShot(sfx.clip, sfx.volume);

        _audioSource.pitch = originalPitch;
    }
}
