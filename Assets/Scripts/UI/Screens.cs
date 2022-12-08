using System;
using System.Collections.Generic;
using UnityEngine;

public class Screens : MonoSingleton<Screens>
{
    [System.Serializable]
    public class ScreenLocationParent
    {
        public GameScreen.ScreenLocation location;
        public Transform parent;
    }

    public GameScreen[] screens;
    public Camera uiCamera = null;
    private List<GameScreen> screensStack = new List<GameScreen>();

    public List<ScreenLocationParent> screenLocationParentsMobile;
    public List<ScreenLocationParent> screenLocationParentsDesktop;

    public GameObject desktopCanvas;
    public GameObject mobileCanvas;
    private List<ScreenLocationParent> screenLocationParents;

    #region Screens
    public T PushScreen<T>(bool unique = false) where T : GameScreen
    {
        T screen = null;

        if (unique)
        {
            // see if is already instantiated
            screen = FindScreen<T>(Instance.screensStack);

            if (screen != null)
            {
                Instance.screensStack.Remove(screen);
            }
        }

        if (screen == null)
        {
            T screenPrefab = FindScreen<T>(Instance.screens);

            if (screenPrefab != null)
            {
                screen = Instantiate(screenPrefab, GetParent(screenPrefab.GetComponent<T>().location)) as T;
            }
            else
            {
                throw new Exception(typeof(T).ToString() + " screen not found!");
            }
        }

        return PushScreen<T>(screen);
    }

    public T PushScreen<T>(T screen) where T : GameScreen
    {
        Instance.screensStack.Add(screen);
        RefreshScreensSortingOrder();
        SetCamera(screen);
        return screen;
    }
    private void Start()
    {
        if (Application.isMobilePlatform)
        {
            screenLocationParents = screenLocationParentsMobile;
            desktopCanvas.SetActive(false);
            RectTransform rTransformUI = mobileCanvas.GetComponent<RectTransform>();
            rTransformUI.anchorMax = new Vector2(1, 1);
            rTransformUI.anchorMin = new Vector2(0, 0);
            rTransformUI.offsetMax = new Vector2(0, 0);
            rTransformUI.offsetMin = new Vector2(0, 0);
            rTransformUI.localScale = new Vector3(1, 1, 1);
        } 
        else
        {
            screenLocationParents = screenLocationParentsDesktop;
            mobileCanvas.SetActive(false);
        }
    }

    public Transform GetParent(GameScreen.ScreenLocation location)
    {
        return screenLocationParents.Find(x => x.location == location)?.parent;
    }

    private static void RefreshScreensSortingOrder()
    {
        for (int i = 0; i < Instance.screensStack.Count; i++)
        {
            Instance.screensStack[i].SetSortingOrder((i + 1) * 50);
            //if (CurrentHud != null)
            //{
            //    CurrentHud.SetHudType((i + 1) * 50 + 1, Instance.screensStack[i].HudType);
            //}
        }
    }
    public void BringToFront<T>() where T : GameScreen
    {
        T screen = FindScreen<T>(Instance.screensStack);
        if (screen != null)
        {
            screen.SetSortingOrder(Instance.screensStack.Count * 50 + 1);
       } 
}
    static T FindScreen<T>(IEnumerable<GameScreen> arrScreens) where T : GameScreen
    {
        GameScreen screen = null;

        foreach (GameScreen s in arrScreens)
        {
            if (s != null &&
                s.gameObject.GetComponent<T>() != null)
            {
                screen = s;
                break;
            }
        }

        return (T)screen;
    }

    public static bool CheckScreenIsOpen<T>() where T : GameScreen
    {
        GameScreen screen = FindScreen<T>(Instance.screensStack);
        return screen != null;
    }

    public void PopScreen<T>(T screen) where T : GameScreen
    {
        //GameEventsManager.Instance.PostEvent(GameEvents.PopupClose);
        Instance.screensStack.Remove(screen);
        RefreshScreensSortingOrder();
        if (screen != null)
        {
            DestroyScreen(screen);
        }
    }

    public void PopScreen()
    {
        if (Instance.screensStack.Count > 0)
        {
            GameScreen screen = Instance.screensStack[Instance.screensStack.Count - 1];
            RefreshScreensSortingOrder();
            if (screen != null)
            {
                Instance.screensStack.Remove(screen);
                DestroyScreen(screen);
            }
        }
    }

    public void PopScreen<T>() where T : GameScreen
    {
        T screen = FindScreen<T>(Instance.screensStack);
        if (screen != null)
        {
            PopScreen(screen);
        }
    }

    public static GameScreen GetCurrentScreen()
    {
        if (Instance.screensStack != null)
        {
            for (int i = Instance.screensStack.Count - 1; i >= 0; i--)
            {
                if (Instance.screensStack[i].gameObject.activeSelf)
                {
                    return Instance.screensStack[i];
                }
            }
        }
        return null;
    }

    public static bool HasScreen<T>() where T : GameScreen
    {
        return FindScreen<T>(Instance.screensStack) != null;
    }

    public static void DestroyScreen(GameScreen screen)
    {
        Destroy(screen.gameObject);
    }

    private static void SetCamera(GameScreen screen)
    {
        if (!screen.useUICamera)
        {
            return;
        }

        if (Instance.uiCamera == null || !Instance.uiCamera.gameObject.activeInHierarchy)
        {
            return;
        }

        Canvas canvas = screen.GetComponent<Canvas>();
        canvas.worldCamera = Instance.uiCamera;
        //canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        //// URP requires that overlay cameras be added to the main camera's overlay stack.
        //var cameraData = Camera.main?.GetUniversalAdditionalCameraData();
        //if (cameraData != null && !cameraData.cameraStack.Contains(Instance.uiCamera))
        //{
        //    cameraData.cameraStack.Add(Instance.uiCamera);
        //}
    }

    #endregion
}