using System.Collections.Generic;
using UnityEngine;

using XInputDotNetPure;

public class ProximityEnemySensor : MonoBehaviour
{
    private List<GameObject> nearbyEnemies = new List<GameObject>();
    private Collider sensorCollider;

    void Start()
    {
        sensorCollider = GetComponentInParent<Collider>();
    }

    void Update()
    {
        if(Time.deltaTime == 0.0f || !enabled)
        {
            setVibration(0, 0);
            return;
        }

        GameObject leftNearestEnemy = null;
        GameObject rightNearestEnemy = null;
        float leftNearestDistance = 0.0f;
        float rightNearestDistance = 0.0f;

        // remove dead and destroyed enemies from the list
        nearbyEnemies.RemoveAll(enemy => isEnemyDead(enemy));

        // find closest enemy on each side
        foreach(GameObject enemy in nearbyEnemies)
        {
            float distance = Vector3.Distance(enemy.transform.position, sensorCollider.transform.position);
            if(enemy.transform.position.x < sensorCollider.transform.position.x){
                if(distance < leftNearestDistance || leftNearestEnemy == null){
                    leftNearestDistance = distance;
                    leftNearestEnemy = enemy;
                }
            }else{
                if(distance < rightNearestDistance || rightNearestEnemy == null){
                    rightNearestDistance = distance;
                    rightNearestEnemy = enemy;
                }
            }

        }

        // scale vibration strength based on collider size
        float size = sensorCollider.bounds.size.x / 2;

        float leftStrength = 0.0f;
        if(leftNearestEnemy != null)
        {
            leftStrength = 1.0f - leftNearestDistance / size;
        }

        float rightStrength = 0.0f;
        if(rightNearestEnemy != null)
        {
            rightStrength = 1.0f - rightNearestDistance / size;
        }


        setVibration(leftStrength, rightStrength);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isEnemy(other.gameObject))
        {
            nearbyEnemies.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(isEnemy(other.gameObject))
        {
            nearbyEnemies.Remove(other.gameObject);
        }
    }

    private bool isEnemy(GameObject obj)
    {
        CompleteProject.EnemyAttack attackComponent = (CompleteProject.EnemyAttack)obj.GetComponentInChildren<CompleteProject.EnemyAttack>();
        return attackComponent != null;
    }

    private bool isEnemyDead(GameObject obj)
    {
        if(!obj) return true;

        CompleteProject.EnemyHealth healthComponent = (CompleteProject.EnemyHealth)obj.GetComponentInChildren<CompleteProject.EnemyHealth>();
        return healthComponent.isDead;
    }

    void OnApplicationQuit()
    {
        setVibration(0, 0);
    }

    void setVibration(float left, float right)
    {
        GamePad.SetVibration(0, left, right);
    }
}
