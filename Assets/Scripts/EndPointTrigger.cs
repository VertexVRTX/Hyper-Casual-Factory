using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPointTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Box box = other.GetComponent<Box>();
        if (box != null && !box.IsHandled)
        {
            box.IsHandled = true;
            GameManager.Instance.OnBoxMissed();
            GameManager.Instance.Conveyor.ReleaseBox(box);
        }
    }
}
