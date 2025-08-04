using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private ObjButton startButton;

    private void Start()
    {
        startButton.AddOnClickListener(OnGenerateValid);
    }

    private void OnGenerateValid()
    {
        GridManager.Instance.AnimationGenerate();
        GridManager.Instance.CloneRemainingTilesToBottom();
        Debug.Log("OnGenerateValid");
    }
}
