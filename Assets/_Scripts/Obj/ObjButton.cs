using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class ObjButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Không tìm thấy component Button!");
        }
    }

    public void AddOnClickListener(UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    /// <summary>
    /// Xóa tất cả listener hiện có.
    /// </summary>
    public void ClearListeners()
    {
        if (button != null)
            button.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Thực hiện nhấn nút thông qua code.
    /// </summary>
    public void SimulateClick()
    {
        if (button != null)
            button.onClick.Invoke();
    }

    /// <summary>
    /// Kích hoạt hoặc vô hiệu hóa nút.
    /// </summary>
    public void SetInteractable(bool state)
    {
        if (button != null)
            button.interactable = state;
    }
}
