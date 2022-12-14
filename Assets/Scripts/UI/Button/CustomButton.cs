using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : Button
{
    public string buttonId = ButtonId.DefaultId;

    protected override void Start()
    {
        base.Start();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (!interactable)
        {
            return;
        }

        GameEventString buttonTapEventData = new()
        {
            eventName = GameEvents.ButtonTap,
            stringData = buttonId
        };
        GameEventsManager.Instance.PostEvent(buttonTapEventData);
        //GameEventsManager.Instance.PostEvent(isAvailable ? soundId : unavailableSoundId);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (!interactable)
        {
            return;
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (!interactable)
        {
            return;
        }
    }
}
