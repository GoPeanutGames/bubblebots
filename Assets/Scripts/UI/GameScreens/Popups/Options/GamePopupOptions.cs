using TMPro;
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

    [Header("Other")]
    public Image playerAvatar;
    public TextMeshProUGUI playerUsername;

    private void Start()
    {
        InitialiseMusicToggle();
        InitialiseHintsToggle();
        SetPlayerAvatar(UserManager.Instance.GetPlayerAvatar());
        RefreshPlayerUsername();
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
    }

    public void HintsToggleValueChanged(bool value)
    {
        HintsToggleOff.SetActive(!value);
        HintsToggleOn.SetActive(value);
    }

    public bool GetFinalMusicValue()
    {
        return MusicToggle.isOn;
    }

    public bool GetFinalHintsValue()
    {
        return HintsToggle.isOn;
    }

    public void SetPlayerAvatar(int avatar)
    {
        playerAvatar.sprite = UserManager.Instance.PlayerAvatars[avatar];
    }

    public void RefreshPlayerUsername()
    {
        playerUsername.text = UserManager.Instance.GetPlayerUserName();
    }
}