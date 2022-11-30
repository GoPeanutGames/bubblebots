using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUISoundToggle : MonoBehaviour
{
    public GameObject OffButton;
    public GameObject OnButton;

    SoundManager soundManager;

    void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
        if (SoundManager.Instance.soundOn)
        {
            SoundOn();
        }
        else
        {
            SoundOff();
        }
    }

    public void SoundOn()
    {
        soundManager.SetVolume(1);

        OffButton.SetActive(true);
        OnButton.SetActive(false);
    }

    public void SoundOff()
    {
        soundManager.SetVolume(0);

        OnButton.SetActive(true);
        OffButton.SetActive(false);
    }
}
