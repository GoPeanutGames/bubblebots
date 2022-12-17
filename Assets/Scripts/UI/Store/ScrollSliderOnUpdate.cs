using UnityEngine;
using UnityEngine.UI;

public class ScrollSliderOnUpdate : MonoBehaviour
{
    public Slider scrollSlider;

    public void UpdateScrollRect(ScrollRect rect)
    {
        scrollSlider.SetValueWithoutNotify(rect.verticalNormalizedPosition);
    }
}