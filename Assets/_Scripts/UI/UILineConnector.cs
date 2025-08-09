using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum EquationType
{
    StraighType1,
    StraighType2,
    MainDiagonal,
    SubDiagonal,
    None
}
public class UILineConnector : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private Image lineImage;
    [SerializeField] private float duration = 1f; // Time Show Line

    [SerializeField] private int straightX;
    [SerializeField] private int straightY;

    [SerializeField] private int straightDefaultX;
    [SerializeField] private int straightDefaultY;

    [SerializeField] private int diagonalX;
    [SerializeField] private int diagonalY;

    [SerializeField] private int diagonalDefaultX;
    [SerializeField] private int diagonalDefaultY;

    [SerializeField] private int straightWidth;
    [SerializeField] private int DiagonalWidth;

    [SerializeField] private int witdhDefault;

    private bool isDrawing = false; //Check

    public void StartDrawing(int indexRow, int indexColumn, int minRow, int maxRow, int minColumn, int maxColumn, int rowA, int rowB, EquationType equationType)
    {
        Debug.Log("Index Drawing: " + indexRow);
        if (!isDrawing)
        {
            lineImage.rectTransform.localScale = new Vector3(1, 1, 1);

            this.CaseLine(indexRow, indexColumn, minRow, maxRow, minColumn, maxColumn, rowA, rowB, equationType);

            StartCoroutine(DrawLineForDuration());
            StartCoroutine(AnimateScaleY(Vector2.one, 0.25f, lineImage));
        }
    }
    private IEnumerator DrawLineForDuration()
    {
        isDrawing = true;
        lineImage.enabled = true;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            yield return null;
        }

        lineImage.enabled = false;
        isDrawing = false;
    }
    private IEnumerator AnimateScaleY(Vector2 fromScale, float decreaseAmount, Image image)
    {
        yield return new WaitForSeconds(0.23f);

        Vector3 scale = image.rectTransform.localScale;
        scale.y = fromScale.y;
        image.rectTransform.localScale = scale;

        while (image.rectTransform.localScale.y > 0f)
        {
            yield return new WaitForSeconds(0.12f);

            Vector3 currentScale = image.rectTransform.localScale;
            currentScale.y -= decreaseAmount;

            // Đảm bảo không xuống âm
            if (currentScale.y < 0f) currentScale.y = 0f;

            image.rectTransform.localScale = currentScale;
        }
    }

    private void CaseLine(int indexRow, int indexColumn, int minRow, int maxRow, int minColumn, int maxColumn, int rowA, int rowB, EquationType equationType)
    {
        switch (equationType)
        {
            case EquationType.StraighType1:
                //Debug.Log($"indexRow: {indexRow}, indexColumn: {indexColumn}, minRow: {minRow}, maxRow: {maxRow}, minColumn: {minColumn}, maxColumn: {maxColumn}");
                float addMinRow = minColumn;
                float addIndexValue = 0;
                if (indexColumn == 1 && minColumn == 0)
                {
                    addIndexValue = 0;
                    addMinRow = 0;
                }
                if (indexColumn > 1)
                {
                    float baseValue = 0;
                    for (int i = 0; i < indexColumn; i++)
                    {
                        baseValue = 25 + 13.5f;
                    }
                    addIndexValue = baseValue * (indexColumn + 1);
                }

                float straighX = straightDefaultX + 115 * (addMinRow + 1) + addIndexValue;
                float straighY = straightDefaultY - 115 * rowA + content.anchoredPosition.y;
                lineImage.rectTransform.anchoredPosition = new Vector2(straighX, straighY);
                lineImage.rectTransform.sizeDelta = new Vector2(straightWidth * indexColumn, witdhDefault);
                break;
            case EquationType.StraighType2:

                break;
            case EquationType.MainDiagonal:

                break;
            case EquationType.SubDiagonal:

                break;
        }
    }
}
