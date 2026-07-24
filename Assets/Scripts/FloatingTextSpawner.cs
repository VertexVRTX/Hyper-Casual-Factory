using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance { get; private set; }

    [SerializeField] private FloatingText floatingTextPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnText(Vector3 worldPosition, int amount, bool isBonus = false)
    {
        if (floatingTextPrefab == null) return;

        Quaternion cameraRotation = Camera.main != null ? Camera.main.transform.rotation : Quaternion.identity;

        FloatingText instance = Instantiate(floatingTextPrefab, worldPosition + Vector3.up * 0.5f, cameraRotation);
        instance.Init(amount, isBonus);
    }
}
