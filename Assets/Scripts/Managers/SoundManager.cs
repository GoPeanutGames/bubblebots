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

    public AudioSource LightningSound;
    public AudioSource BombSound;
    public AudioSource ColorSound;
    public AudioSource HammerSound;
    public AudioSource ClickSound;
    public AudioSource ComboSound1;
    public AudioSource ComboSound2;
    public AudioSource ComboSound3;
    public AudioSource ComboSound4;
    public AudioSource ComboSound5;

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
        FadeOutRobotSelectionMusic();
        StartMusicSource?.Play();

        if (!soundOn)
        {
            StartMusicSource.volume = 0;
        }
    }

    public void PlayRobotSelectionMusic()
    {
        FadeOutStartMusic();
        RobotSelectionSource?.Play();

        if (!soundOn)
        {
            RobotSelectionSource.volume = 0;
        }
    }

    public void PlayLevelMusic()
    {
        FadeOutRobotSelectionMusic();
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

        //ClickSound?.Play();
    }

    public void PlayBombSound()
    {
        if (!soundOn)
        {
            return;
        }

        BombSound?.Play();
    }

    public void PlayLightningSound()
    {
        if (!soundOn)
        {
            return;
        }

        LightningSound?.Play();
    }

    public void PlayColorSound()
    {
        if (!soundOn)
        {
            return;
        }

        ColorSound?.Play();
    }

    public void PlayHammerSound()
    {
        if (!soundOn)
        {
            return;
        }

        HammerSound?.Play();
    }

    public void PlayComboSound(int combo)
    {
        if (!soundOn)
        {
            return;
        }

        switch(combo)
        {
            case 0:
            case 1:
                ComboSound1?.Play();
                break;
            case 2:
                ComboSound2?.Play();
                break;
            case 3:
                ComboSound3?.Play();
                break;
            case 4:
                ComboSound4?.Play();
                break;
            default:
                ComboSound5?.Play();
                break;
        }
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
        BombSound.volume = level;
        ColorSound.volume = level;
        HammerSound.volume = level;
        LightningSound.volume = level;
        ComboSound1.volume = level;
        ComboSound2.volume = level;
        ComboSound3.volume = level;
        ComboSound4.volume = level;
        ComboSound5.volume = level;

        soundOn = level == 1;
    }
}
