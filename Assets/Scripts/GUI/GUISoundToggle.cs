using UnityEngine;

public class GUISoundToggle : MonoBehaviour
{
    public GameObject OffButton;
    public GameObject OnButton;

    void Start()
    {
        if (SoundManager.Instance.IsMuted())
        {
            SoundOff();
        }
        else
        {
            SoundOn();
        }
    }

    public void SoundOn()
    {
        SoundManager.Instance.UnMute();
        OffButton.SetActive(true);
        OnButton.SetActive(false);
    }

    public void SoundOff()
    {
        SoundManager.Instance.Mute();
        OnButton.SetActive(true);
        OffButton.SetActive(false);
    }
}
