using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySensorVisualCue : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.Euler(270, -transform.parent.rotation.y, 0);
    }
}
