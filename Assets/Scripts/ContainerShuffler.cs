using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerShuffler : MonoBehaviour
{
    public List<Transform> containers;

    private void Start()
    {
        ShuffleContainers();
    }

    public void ShuffleContainers()
    {
        if (containers == null || containers.Count == 0) return;

        List<Vector3> positions = new List<Vector3>();
        foreach (var container in containers)
        {
            positions.Add(container.position);
        }

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 temp = positions[i];
            int randomIndex = Random.Range(i, positions.Count);
            positions[i] = positions[randomIndex];
            positions[randomIndex] = temp;
        }

        for (int i = 0; i < containers.Count; i++)
        {
            containers[i].position = positions[i];
        }
    }
}
