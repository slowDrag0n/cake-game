using Lofelt.NiceVibrations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundController : MonoBehaviour
{
    public Sound[] Bgms;
    [Space]
    public Sound[] Sounds;

    Dictionary<SoundType, Sound> _soundData;

    public static SoundController Instance;

    AudioSource _bgmSrc;
    AudioSource _oneShotSrc;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _soundData = new Dictionary<SoundType, Sound>();

        for(int i = 0; i < Sounds.Length; i++)
        {
            _soundData.Add(Sounds[i].Type, Sounds[i]);
        }
    }

    public void PlayBgm()
    {
        if(Bgms == null)
            return;

        if(Bgms.Length == 0)
            return;

        var randBgmIndex = Random.Range(0, Bgms.Length);
        var selectedBgm = Bgms[randBgmIndex];

        if(selectedBgm.Clip == null)
            return;

        if(_bgmSrc == null)
            _bgmSrc = gameObject.AddComponent<AudioSource>();
        else if(_bgmSrc.isPlaying && _bgmSrc.clip == selectedBgm.Clip)
            return;

        _bgmSrc.clip = selectedBgm.Clip;
        _bgmSrc.volume = selectedBgm.Volume;
        _bgmSrc.pitch = 1;
        _bgmSrc.mute = !Profile.MusicEnabled;
        _bgmSrc.loop = true;
        _bgmSrc.Play();
    }

    internal void UpdateBgmMuteState()
    {
        if(_bgmSrc == null)
            return;

        _bgmSrc.mute = !Profile.MusicEnabled;
    }



    public void PlaySound(SoundType type, float pitch = 1f, bool oneShot = false)
    {
        if(Profile.HapticsEnabled)
            HapticPatterns.PlayEmphasis(1.0f, 0.0f);

        if(_soundData.ContainsKey(type) == false)
        {
            Debug.LogError("Sound type Not Found: " + type.ToString());
            return;
        }

        AudioClip clip = _soundData[type].Clip;
        float vol = _soundData[type].Volume;

        if(clip == null)
            return;

        if(oneShot)
        {
            if(_oneShotSrc == null) _oneShotSrc = gameObject.AddComponent<AudioSource>();
            _oneShotSrc.clip = clip;
            _oneShotSrc.volume = vol;
            _oneShotSrc.pitch = pitch;
            _oneShotSrc.mute = !Profile.SoundEnabled;
            if(_oneShotSrc.isPlaying == false) _oneShotSrc.Play();

            return;
        }

        var src = gameObject.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = vol;
        src.pitch = pitch;
        src.mute = !Profile.SoundEnabled;
        src.Play();
        Destroy(src, clip.length);
    }

    public void PlaySound(AudioClip clipToPlay, float volume = 1f, float pitch = 1f, bool oneShot = false)
    {
        if(Profile.HapticsEnabled)
            HapticPatterns.PlayEmphasis(1.0f, 0.0f);

        if(clipToPlay == null)
            return;

        AudioClip clip = clipToPlay;

        if(oneShot)
        {
            if(_oneShotSrc == null) _oneShotSrc = gameObject.AddComponent<AudioSource>();
            _oneShotSrc.clip = clip;
            _oneShotSrc.volume = volume;
            _oneShotSrc.pitch = pitch;
            _oneShotSrc.mute = !Profile.SoundEnabled;
            if(_oneShotSrc.isPlaying == false) _oneShotSrc.Play();

            return;
        }

        var src = gameObject.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.mute = !Profile.SoundEnabled;
        src.Play();
        Destroy(src, clip.length);
    }

    //public void PlayPopSound()
    //{
    //    // If the timer has reached timeout, reset the pitch and timer
    //    if(Time.time >= _currentTimer + _timeout)
    //    {
    //        _currentPitch = PitchMinMax.x;
    //    }
    //    // If the timer hasn't reached timeout, increase the pitch
    //    else
    //    {
    //        _currentPitch += PitchIncFac;
    //        // Clamp pitch to pitchMax
    //        _currentPitch = Mathf.Min(_currentPitch, PitchMinMax.y);
    //    }

    //    // Set audio clip and pitch, then play
    //    PlaySound(SoundType.SketchPop, _currentPitch);

    //    _currentTimer = Time.time;

    //    // Update timeout
    //    _timeout = Timeout;

    //}
}

[Serializable]
public class Sound
{
    public SoundType Type;
    public AudioClip Clip;
    [Range(0f, 1f)] public float Volume = .6f;
}

