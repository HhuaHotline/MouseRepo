using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TextMeshProUGUI interactText;

    void Awake() { Instance = this; }

    // 注意：去掉了 itemName 参数
    public void ShowInteractPrompt(bool show)
    {
        if (show)
        {
            // 加上了方括号，看起来像按键
            interactText.text = "按 [E] 拾取";
            interactText.gameObject.SetActive(true);
        }
        else
        {
            interactText.gameObject.SetActive(false);
        }
    }
}