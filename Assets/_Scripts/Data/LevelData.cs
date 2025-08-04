using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData : ScriptableObject
{
    public List<int> boardNumbers; // mảng 1 chiều
    public int columnCount; // số cột
}
