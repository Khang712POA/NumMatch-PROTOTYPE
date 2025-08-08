using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileNumberCopy : MonoBehaviour
{
    [SerializeField] Image imageNumber;
    public void SetImageNumber(Sprite sprite, Color color)
    {
        imageNumber.gameObject.SetActive(true);
        imageNumber.sprite = sprite;
        imageNumber.color = color;
    }
}
