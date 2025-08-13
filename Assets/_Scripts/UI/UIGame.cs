using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGame : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button journeyButton;
    [SerializeField] private Button gemButton;

    private void Start()
    {
        BindButton(newGameButton, GameMode.Journey);
        BindButton(journeyButton, GameMode.Journey);
        BindButton(gemButton, GameMode.Gem);
    }

    private void BindButton(Button button, GameMode mode)
    {
        button.onClick.AddListener(() => LoadSceneAndSetMode("GamePlay", mode));
    }

    private void LoadSceneAndSetMode(string sceneName, GameMode mode)
    {
        ScenesManager.instance.LoadScene(sceneName);
        GameManager.Instance.SetMode(mode);
    }
}
