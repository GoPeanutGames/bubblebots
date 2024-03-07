using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ChangeGridOnPlatform : MonoBehaviour
{
    public Vector2 cellSizeDesktop;
    public Vector2 cellSizeMobile;
    public Vector2 cellSpacingDesktop;
    public Vector2 cellSpacingMobile;
    private GridLayoutGroup _gridLayoutGroup;
    
    void Start()
    {
        _gridLayoutGroup = GetComponent<GridLayoutGroup>();
        if (Application.isMobilePlatform){
            _gridLayoutGroup.cellSize = cellSizeMobile;
            _gridLayoutGroup.spacing = cellSpacingMobile;
        }
        else{
            _gridLayoutGroup.cellSize = cellSizeDesktop;
            _gridLayoutGroup.spacing = cellSpacingDesktop;
        }
    }
}
