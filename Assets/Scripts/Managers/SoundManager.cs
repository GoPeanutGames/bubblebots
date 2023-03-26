using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoSingleton<SoundManager>
{
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource lightningSource;

    [Header("BGM")]
    public AudioClip homeBgm;
    public AudioClip robotSelectBgm;
    public AudioClip battleBgm;
    [Header("Battle SFX")]
    public List<AudioClip> ComboSfxs;
    public AudioClip HammerSfx;
    public AudioClip ColorSfx;
    public AudioClip LightningSfx;
    public AudioClip BombSfx;
    public AudioClip lightningMissile;
    public AudioClip lightningExplosion;
    [Header("Game SFX")] 
    public AudioClip modeSelectSfx;
    public AudioClip loginSuccessSfx;
    public AudioClip battleLostSfx;
    public AudioClip battleStartSfx;

    private bool muted = false;

    private void Start()
    {
        bool musicOn = UserManager.Instance.GetPlayerSettings().music;
        SetMute(!musicOn);
    }

    private static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume, Action onFadeEnd = null)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        onFadeEnd?.Invoke();
        yield break;
    }

    private void SetMute(bool m)
    {
        muted = m;
        musicSource.mute = muted;
        sfxSource.mute = muted;
        lightningSource.mute = muted;
    }

    public void FadeInMusic(float volume = 1, float time = 0.5f)
    {
        StopAllCoroutines();
        StartCoroutine(SoundManager.StartFade(musicSource, time, volume));
    }

    public void FadeOutMusic(float volume = 0, float time = 0.5f)
    {
        StopAllCoroutines();
        StartCoroutine(SoundManager.StartFade(musicSource, time, volume));
    }

    public void FadeOutMusic(Action onFadeEnd, float volume = 0, float time = 0.5f)
    {
        StopAllCoroutines();
        StartCoroutine(SoundManager.StartFade(musicSource, time, volume, onFadeEnd));
    }
    public void Mute()
    {
        SetMute(true);
    }

    public void UnMute()
    {
        SetMute(false);
    }

    public bool IsMuted()
    {
        return muted;
    }

    public void PlayComboSfx(int combo)
    {
        if(combo > 5)
        {
            combo = 5;
        }
        sfxSource.PlayOneShot(ComboSfxs[combo]);
    }

    public void PlayLightningMissile()
    {
        lightningSource.PlayOneShot(lightningMissile);
    }

    public void PlayLightningExplosion()
    {
        lightningSource.PlayOneShot(lightningExplosion);
    }

    public void PlayHammerSfx()
    {
        sfxSource.PlayOneShot(HammerSfx);
    }

    public void PlayColorSfx()
    {
        sfxSource.PlayOneShot(ColorSfx);
    }

    public void PlayLightningSfx()
    {
        sfxSource.PlayOneShot(LightningSfx);
    }

    public void PlayBombSfx()
    {
        sfxSource.PlayOneShot(BombSfx);
    }
    
    public void PlayHomeMusic()
    {
        if (musicSource.clip == homeBgm) return;
        musicSource.clip = homeBgm;
        musicSource.Play();
    }

    public void PlayRobotSelectMusic()
    {
        if (musicSource.clip == robotSelectBgm) return;
        musicSource.clip = robotSelectBgm;
        musicSource.Play();
    }
    
    public void PlayBattleBgm()
    {
        if (musicSource.clip == battleBgm) return;
        musicSource.clip = battleBgm;
        musicSource.Play();
    }

    public void PlayModeSelectedSfx()
    {
        sfxSource.PlayOneShot(modeSelectSfx);
    }
    
    public void PlayLoginSuccessSfx()
    {
        sfxSource.PlayOneShot(loginSuccessSfx);
    }
    
    public void PlayBattleLostSfx()
    {
        sfxSource.PlayOneShot(battleLostSfx);
    }
    
    public void PlayBattleStartSfx()
    {
        sfxSource.PlayOneShot(battleStartSfx);
    }
}
