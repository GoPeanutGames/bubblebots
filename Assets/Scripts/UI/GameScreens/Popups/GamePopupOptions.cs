using UnityEngine;
using UnityEngine.UI;

public class GamePopupOptions : GameScreenAnimatedEntryExit
{
    public Toggle MusicToggle;
    public GameObject MusicToggleOff;
    public GameObject MusicToggleOn;

    private void Start()
    {
        bool muted = SoundManager.Instance.IsMuted();
        MusicToggle.SetIsOnWithoutNotify(!muted);
        MusicToggleOff.SetActive(muted);
        MusicToggleOn.SetActive(!muted);
    }

    public void ToggleValueChanged(bool value)
    {
        if (value)
        {
            MusicToggleOff.SetActive(false);
            MusicToggleOn.SetActive(true);
            SoundManager.Instance.UnMute();
        }
        else
        {
            MusicToggleOff.SetActive(true);
            MusicToggleOn.SetActive(false);
            SoundManager.Instance.Mute();
        }
    }
}