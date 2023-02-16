using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoSingleton<SoundManager>
{
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource lightningSource;

    public AudioClip StartMusic;
    public AudioClip RobotSelectMusic;
    public AudioClip LevelMusic;
    public AudioClip NetherModeMusic;
    public List<AudioClip> ComboSfxs;
    public AudioClip HammerSfx;
    public AudioClip ColorSfx;
    public AudioClip LightningSfx;
    public AudioClip BombSfx;
    public AudioClip StartButtonSfx;
    public AudioClip MetamaskSfx;

    public AudioClip lightningMissile;
    public AudioClip lightningExplosion;

    private bool muted = false;

    private static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }

    private static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume, Action onFadeEnd)
    {
        float currentTime = 0;
        float start = audioSource.volume;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        onFadeEnd.Invoke();
        yield break;
    }

    private void SetMute(bool m)
    {
        muted = m;
        musicSource.mute = muted;
        sfxSource.mute = muted;
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

    public void PlayStartButtonSfx()
    {
        sfxSource.PlayOneShot(StartButtonSfx);
    }

    public void PlayMetamaskSfx()
    {
        sfxSource.PlayOneShot(MetamaskSfx);
    }

    public void PlayStartMusicNew()
    {
        musicSource.clip = StartMusic;
        musicSource.Play();
    }

    public void PlayRobotSelectMusicNew()
    {
        musicSource.clip = RobotSelectMusic;
        musicSource.Play();
    }

    public void PlayLevelMusicNew()
    {
        musicSource.clip = LevelMusic;
        musicSource.Play();
    }

    public void PlayNetherModeMusic()
    {
        musicSource.clip = NetherModeMusic;
        musicSource.Play();
    }
}
