using System;
using UnityEngine;

[Serializable]
public class GemComponent
{
    [SerializeField] GemType gemType;
    public GemType GemType => gemType;
    [SerializeField] int count;
    public int Count => count;

    public GemComponent(GemType type, int count)
    {
        this.gemType = type;
        this.count = count;
    }
    public void DecreaseCount()
    {
        if (count > 0) count--;
    }
    public void ResetCount()
    {
        count = 0;
    }
}
public enum GemType
{
    Pink,       // Đá hồng
    Orange,      // Đá cam
    Purple      // Đá tím
}