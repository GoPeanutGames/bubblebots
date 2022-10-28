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

    bool soundOn = true;

    public void PlayMetamaskEffect()
    {
        if (!soundOn)
        {
            return;
        }

        MetamaskSource?.Play();
    }

    public void PlayStartButtonEffect()
    {
        if (!soundOn)
        {
            return;
        }

        StartButtonSource?.Play();
    }

    public void PlayErrorMessageEffect()
    {
        if (!soundOn)
        {
            return;
        }

        ErrorMessageSource?.Play();
    }

    public void PlayStartMusic()
    {
        StartMusicSource?.Play();

        if (!soundOn)
        {
            StartMusicSource.volume = 0;
        }
    }

    public void PlayRobotSelectionMusic()
    {
        RobotSelectionSource?.Play();

        if (!soundOn)
        {
            RobotSelectionSource.volume = 0;
        }
    }

    public void PlayLevelMusic()
    {
        LevelMusicSource?.Play();

        if (!soundOn)
        {
            LevelMusicSource.volume = 0;
        }
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
        if (!soundOn)
        {
            return;
        }

        if (LevelMusicSource == null)
        {
            return;
        }

        StartCoroutine(FadeOutLevelMusicNow());
    }

    public void FadeOutRobotSelectionMusic()
    {
        if (!soundOn)
        {
            return;
        }

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

        if (soundOn)
        {
            StartMusicSource.volume = 1;
        }
    }

    IEnumerator FadeOutLevelMusicNow()
    {
        LevelMusicSource.DOFade(0, 0.5f);

        yield return new WaitForSeconds(0.5f);

        LevelMusicSource.Stop();

        if (soundOn)
        {
            LevelMusicSource.volume = 1;
        }
    }

    IEnumerator FadeOutRobotSelectionMusicNow()
    {
        RobotSelectionSource.DOFade(0, 0.5f);

        yield return new WaitForSeconds(0.5f);

        RobotSelectionSource.Stop();

        if (soundOn)
        {
            RobotSelectionSource.volume = 1;
        }
    }

    public void FadeInRobotSelectionMusic()
    {
        RobotSelectionSource.volume = 0;
        RobotSelectionSource.Play();

        if (soundOn)
        {
            RobotSelectionSource.DOFade(1, 0.5f);
        }
    }

    public void PlayClickSound()
    {
        if (!soundOn)
        {
            return;
        }

        ClickSound?.Play();
    }

    public void SetVolume(float level)
    {
        MetamaskSource.volume = level;
        StartMusicSource.volume = level;
        StartButtonSource.volume = level;
        ErrorMessageSource.volume = level;
        RobotSelectionSource.volume = level;
        LevelMusicSource.volume = level;
        ClickSound.volume = level;

        soundOn = level == 1;
    }
}
