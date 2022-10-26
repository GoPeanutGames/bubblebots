using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    public AudioSource MetamaskSource;
    public AudioSource StartMusicSource;
    public AudioSource StartButtonSource;
    public AudioSource ErrorMessageSource;
    public AudioSource RobotSelectionSource;
    public AudioSource LevelMusicSource;

    public AudioSource ClickSound;

    public void PlayMetamaskEffect()
    {
        MetamaskSource?.Play();
    }

    public void PlayStartButtonEffect()
    {
        StartButtonSource?.Play();
    }

    public void PlayErrorMessageEffect()
    {
        ErrorMessageSource?.Play();
    }

    public void PlayStartMusic()
    {
        StartMusicSource?.Play();
    }

    public void PlayRobotSelectionMusic()
    {
        RobotSelectionSource?.Play();
    }

    public void PlayLevelMusic()
    {
        LevelMusicSource?.Play();
    }

    public void StopStartMusic()
    {
        StartMusicSource?.Stop();
    }

    public void FadeOutStartMusic()
    {
        if (StartMusicSource == null)
        {
            return;
        }

        StartCoroutine(FadeOutStartMusicNow());
    }

    public void FadeOutLevelMusic()
    {
        if (LevelMusicSource == null)
        {
            return;
        }

        StartCoroutine(FadeOutLevelMusicNow());
    }

    public void FadeOutRobotSelectionMusic()
    {
        if (RobotSelectionSource == null)
        {
            return;
        }

        StartCoroutine(FadeOutRobotSelectionMusicNow());
    }

    IEnumerator FadeOutStartMusicNow()
    {
        StartMusicSource.DOFade(0, 0.5f);

        yield return new WaitForSeconds(0.5f);

        StartMusicSource.Stop();
        StartMusicSource.volume = 1;
    }

    IEnumerator FadeOutLevelMusicNow()
    {
        LevelMusicSource.DOFade(0, 0.5f);

        yield return new WaitForSeconds(0.5f);

        LevelMusicSource.Stop();
        LevelMusicSource.volume = 1;
    }

    IEnumerator FadeOutRobotSelectionMusicNow()
    {
        RobotSelectionSource.DOFade(0, 0.5f);

        yield return new WaitForSeconds(0.5f);

        RobotSelectionSource.Stop();
        RobotSelectionSource.volume = 1;
    }

    public void FadeInRobotSelectionMusic()
    {
        RobotSelectionSource.volume = 0;
        RobotSelectionSource.Play();
        RobotSelectionSource.DOFade(1, 0.5f);
    }

    public void PlayClickSound()
    {
        ClickSound?.Play();
    }
}
