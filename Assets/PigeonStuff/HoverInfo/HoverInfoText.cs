using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverInfoText : HoverInfo
{
    [SerializeField] [TextArea] string text;

    public void SetText(string s) => text = s;

    public override string GetInfo() => text;
}