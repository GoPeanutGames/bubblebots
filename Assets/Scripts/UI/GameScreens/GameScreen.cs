using System;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
[System.Serializable]
[ExecuteInEditMode]
public class GameScreen : MonoBehaviour
{
    public enum ScreenLocation
    {
        Background,
        Foreground,
        Popups
    }

    public delegate void ButtonClickedHandler(string buttonName);
    public event ButtonClickedHandler ButtonClicked = delegate { };
    protected Canvas canvas;
    public ScreenLocation location;
    public Canvas GetCanvas { get => canvas; }


    public AnimationSetting animationSettings;
    protected virtual void Awake()
    {
        canvas = GetComponent<Canvas>();
    }


    protected virtual void OnEnable()
    {
    }

    public virtual void OnBackButton()
    {
    }

    public void OnButtonClicked(string buttonName)
    {
        ButtonClicked(buttonName);
    }

    public virtual bool useUICamera
    {
        get { return true; }
    }

    public void SetSortingOrder(int order)
    {
        if (canvas != null)
        {
            canvas.sortingOrder = order;
        }
        else
        {
            Debug.LogError("GameScreen should have a Canvas component " + gameObject.name);
        }
    }

    #region Animations
    public void PlayOpenAnimation()
    {
        gameObject.SetActive(true);

        if (animationSettings.AnimatorComponent != null)
        {
            animationSettings.AnimatorComponent.Play(animationSettings.OpenStateId);
        }
    }

    public void PlayCloseAnimation()
    {
        gameObject.SetActive(true);

        if (animationSettings.AnimatorComponent != null)
        {
             animationSettings.AnimatorComponent.Play(animationSettings.CloseStateId);
        }

    }

    #endregion
}

[Serializable]
public class AnimationSetting
{
    public Animator AnimatorComponent;
    public string OpenStateId;
    public string CloseStateId;
}