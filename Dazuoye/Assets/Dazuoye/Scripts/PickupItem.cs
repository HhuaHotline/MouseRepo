using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("垃圾属性")]
    public string itemName = "普通垃圾";
    public int value = 1;           // 增加的数量
    public float weight = 1f;       // 增加的负重

    private bool isPlayerNearby = false;
    private PlayerInventory playerInventory;

    void Update()
    {
        // 如果玩家在附近，且按下了 E 键
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (playerInventory != null)
            {
                // 让玩家背包增加物品
                playerInventory.AddItem(this);

                // 【修改点1】：调用 UI 时只传一个参数
                UIManager.Instance.ShowInteractPrompt(false);

                // 销毁地上的垃圾
                Destroy(gameObject);
            }
        }
    }

    // 玩家进入范围
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerInventory = other.GetComponent<PlayerInventory>();

            // 【修改点2】：调用 UI 时只传一个参数
            UIManager.Instance.ShowInteractPrompt(true);
        }
    }

    // 玩家离开范围
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            playerInventory = null;

            // 【修改点3】：调用 UI 时只传一个参数
            UIManager.Instance.ShowInteractPrompt(false);
        }
    }
}