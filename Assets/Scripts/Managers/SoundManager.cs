using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource MetamaskSource;
    public AudioSource StartMusicSource;
    public AudioSource StartButtonSource;
    public AudioSource ErrorMessageSource;

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

    public void StopStartMusic()
    {
        StartMusicSource?.Stop();
    }
}
