using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBoardCopy : MonoBehaviour
{
    [SerializeField] RectTransform holderTile;
    [SerializeField] GameObject TitlePrefab;

    private List<TileNumberCopy> tileNumberCopys = new List<TileNumberCopy>();

    private int numberTile;
    private void Start()
    {
        numberTile = BoardConfig.COUNTTILE_DEFAULT;
        this.CreateDisplay();
    }
    private void CreateDisplay()
    {
        tileNumberCopys.Clear();

        foreach (Transform child in holderTile)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < numberTile; i++)
        {
            //Debug.Log("Create: " + i);
            var obj = Instantiate(TitlePrefab, holderTile);
            TileNumberCopy tileNumberCopy = obj.GetComponent<TileNumberCopy>();
            tileNumberCopys.Add(tileNumberCopy);
            obj.gameObject.SetActive(true);
        }
    }
    public void CopyTile(List<TileNumber> tileNumber)
    {
        for (int i = 0; i < tileNumberCopys.Count; i++)
        {
            if (tileNumber[i].Value != -1)
                tileNumberCopys[i].SetImageNumber(tileNumber[i].NumberImage.sprite, tileNumber[i].NumberImage.color);
        }
    }
}
