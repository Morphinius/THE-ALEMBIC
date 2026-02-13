using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class CameraMove : MonoBehaviour
{
    public Transform target;
    public float smooth= 5.0f;
    public Vector3 offset = new Vector3(10, 14, -10);
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * smooth);

        if (Input.mouseScrollDelta.y > 0)
        {
            offset = new Vector3(3, 7, -3);
        }


        if (Input.mouseScrollDelta.y < 0)
        {
            offset = new Vector3(10, 14, -10);
        }
    } 
}