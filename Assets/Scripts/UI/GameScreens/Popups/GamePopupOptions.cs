using UnityEngine;
using UnityEngine.UI;

public class GamePopupOptions : GameScreenAnimatedEntryExit
{
    [Header("Music")]
    public Toggle MusicToggle;
    public GameObject MusicToggleOff;
    public GameObject MusicToggleOn;
    
    [Header("Hints")] 
    public Toggle HintsToggle;
    public GameObject HintsToggleOff;
    public GameObject HintsToggleOn;

    private bool finalMusicValue;
    private bool finalHintValue;

    private void Start()
    {
        InitialiseMusicToggle();
        InitialiseHintsToggle();
    }

    private void InitialiseMusicToggle()
    {
        bool muted = SoundManager.Instance.IsMuted();
        MusicToggle.SetIsOnWithoutNotify(!muted);
        MusicToggleOff.SetActive(muted);
        MusicToggleOn.SetActive(!muted);
    }

    private void InitialiseHintsToggle()
    {
        bool hinting = UserManager.Instance.GetPlayerHints();
        HintsToggle.SetIsOnWithoutNotify(hinting);
        HintsToggleOff.SetActive(!hinting);
        HintsToggleOn.SetActive(hinting);
    }

    public void MusicToggleValueChanged(bool value)
    {
        MusicToggleOff.SetActive(!value);
        MusicToggleOn.SetActive(value);
        finalMusicValue = value;
    }

    public void HintsToggleValueChanged(bool value)
    {
        HintsToggleOff.SetActive(!value);
        HintsToggleOn.SetActive(value);
        finalHintValue = value;
    }

    public bool GetFinalMusicValue()
    {
        return MusicToggle.isOn;
    }

    public bool GetFinalHintsValue()
    {
        return HintsToggle.isOn;
    }
}