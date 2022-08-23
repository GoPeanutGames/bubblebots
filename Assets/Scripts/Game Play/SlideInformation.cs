using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideInformation
{
    public Vector2 From { get; }
    public Vector2 To { get; }

    public SlideInformation(Vector2 to, Vector2 from)
    {
        From = from;
        To = to;
    }
}
