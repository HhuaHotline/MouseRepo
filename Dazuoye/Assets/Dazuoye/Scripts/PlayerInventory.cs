using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int totalGarbageCount = 0;   // 当前捡到的总数
    public float currentWeight = 0f;    // 当前负重

    public void AddItem(PickupItem item)
    {
        totalGarbageCount += item.value;
        currentWeight += item.weight;

        // 在控制台打印出来，方便你测试
        Debug.Log($"捡到了：{item.itemName}！当前总数：{totalGarbageCount}，负重：{currentWeight}");
    }
}