using UnityEngine;
using UnityEngine.UI;

public class UINewGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button newGameButton; // Gán trong Inspector
    private void Start()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(() => ScenesManager.instance.LoadScene("GamePlay"));
    }
}
