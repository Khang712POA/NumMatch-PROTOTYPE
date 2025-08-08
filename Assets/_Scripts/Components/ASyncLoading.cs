using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ASyncLoading : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private RectTransform iconHandle;
    [SerializeField] private string nextScreen = "GameHome";
    private void Start()
    {
        this.LoadLevelBtn(nextScreen);
    }
    private void LoadLevelBtn(string levelToLoad)
    {
        StartCoroutine(AnimateLoadingText());
        StartCoroutine(LoadLevelASync(levelToLoad));
    }

    IEnumerator LoadLevelASync(string levelToLoad)
    {
        // Bắt đầu load scene nhưng chưa cho phép chuyển
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        float duration = 2f; // thời gian slider chạy
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Tính % slider
            float progress = Mathf.Clamp01(elapsed / duration);
            loadingSlider.value = progress;

            // Xoay icon mỗi frame
            float zRotation = (elapsed / duration) * 360f;
            iconHandle.localRotation = Quaternion.Euler(0f, 0f, zRotation);

            yield return null;
        }

        // Đảm bảo slider đầy
        loadingSlider.value = 1f;
        iconHandle.localRotation = Quaternion.Euler(0f, 0f, 360f);

        // Cho phép chuyển scene
        loadOperation.allowSceneActivation = true;
    }

    IEnumerator AnimateLoadingText()
    {
        float duration = 3f; // thời gian 3 giây
        float elapsed = 0f;

        // Bắt đầu từ góc 0
        float startRotation = 0f;
        float endRotation = 360f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Xoay trục Z
            float zRotation = Mathf.Lerp(startRotation, endRotation, t);
            iconHandle.localRotation = Quaternion.Euler(0f, 0f, zRotation);

            yield return null;
        }

        // Đảm bảo dừng đúng 360 độ
        iconHandle.localRotation = Quaternion.Euler(0f, 0f, endRotation);
    }

}
