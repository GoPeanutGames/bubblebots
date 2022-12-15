using UnityEngine.EventSystems;

public class CustomStoreButton : CustomButton
{
    private int bundleId;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable)
        {
            return;
        }

        GameEventStore buttonTapEventData = new()
        {
            eventName = GameEvents.ButtonTap,
            stringData = buttonId,
            bundleId = bundleId
        };
        GameEventsManager.Instance.PostEvent(buttonTapEventData);
        //GameEventsManager.Instance.PostEvent(isAvailable ? soundId : unavailableSoundId);
    }

    public void SetBundleId(int bundleId)
    {
        this.bundleId = bundleId;
    }
}
