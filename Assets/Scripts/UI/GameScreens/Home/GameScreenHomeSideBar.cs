using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameScreenHomeSideBar : GameScreenAnimatedShowHide
{
    public List<TMPro.TextMeshProUGUI> buttonsTexts;


    public async Task ApplySameSizeTexts()
    {
        await Task.Delay(100);
        buttonsTexts.ForEach(x => x.ForceMeshUpdate());


        float minSize = buttonsTexts.Select(x => x.fontSize).Min();
        buttonsTexts.ForEach(x =>
        {
            x.enableAutoSizing = false;
            x.fontSize = minSize;
        });
    }

    override protected void Awake()
    {
        base.Awake();
        _ = ApplySameSizeTexts();
    }


}