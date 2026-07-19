using UnityEngine;
using System.Collections.Generic;

public class EnemyCullingManager : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float activateDistance = 30f;
    [SerializeField] private float checkInterval = 0.5f;

    [Header("Список врагов (перетащить вручную)")]
    [SerializeField] private List<GameObject> allEnemies = new List<GameObject>();

    [Header("Отладка")]
    [SerializeField] private bool showDebugLogs = false;

    private Transform player;
    private float nextCheckTime;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("[EnemyCullingManager] Player not found!");
        }

        if (allEnemies.Count == 0)
        {
            Debug.LogWarning("[EnemyCullingManager] Enemy list is empty! Add enemies in inspector.");
        }
    }

    void Update()
    {
        if (player == null) return;
        if (Time.time < nextCheckTime) return;

        nextCheckTime = Time.time + checkInterval;

        foreach (GameObject enemy in allEnemies)
        {
            if (enemy == null) continue;

            float distance = Vector3.Distance(enemy.transform.position, player.position);
            bool shouldBeActive = distance < activateDistance;

            if (enemy.activeSelf != shouldBeActive)
            {
                if (showDebugLogs)
                    Debug.Log($"[{enemy.name}] Distance={distance:F1}, Active={shouldBeActive}");

                enemy.SetActive(shouldBeActive);
            }
        }
    }

    // Добавить врага в список (можно вызывать из других скриптов)
    public void AddEnemy(GameObject enemy)
    {
        if (!allEnemies.Contains(enemy))
        {
            allEnemies.Add(enemy);
            if (showDebugLogs)
                Debug.Log($"[EnemyCullingManager] Added enemy: {enemy.name}");
        }
    }

    // Удалить врага из списка (при смерти)
    public void RemoveEnemy(GameObject enemy)
    {
        if (allEnemies.Contains(enemy))
        {
            allEnemies.Remove(enemy);
            if (showDebugLogs)
                Debug.Log($"[EnemyCullingManager] Removed enemy: {enemy.name}");
        }
    }

    // Показать всех врагов в консоли (для отладки)
    public void ShowAllEnemies()
    {
        Debug.Log($"=== Total enemies in list: {allEnemies.Count} ===");
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
                Debug.Log($"Enemy: {enemy.name}, Active: {enemy.activeSelf}, Pos: {enemy.transform.position}");
        }
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, activateDistance);
        }
    }
}