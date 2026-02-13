using UnityEngine;

public class ChestTrigger : MonoBehaviour
{
    [Header("Chest Reference")]
    [SerializeField] private GameObject chestObject; // Ссылка на объект сундука
    
    private ChestInteraction chestInteraction;
    private bool isPlayerInTrigger = false;

    private void Start()
    {
        // Находим скрипт ChestInteraction на сундуке
        if (chestObject != null)
        {
            chestInteraction = chestObject.GetComponent<ChestInteraction>();
        }
        
        if (chestInteraction == null)
        {
            Debug.LogError("ChestInteraction не найден на объекте сундука!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            if (chestInteraction != null)
            {
                chestInteraction.SetPlayerInTrigger(true);
            }
            Debug.Log("Игрок вошел в зону сундука");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            if (chestInteraction != null)
            {
                chestInteraction.SetPlayerInTrigger(false);
            }
            Debug.Log("Игрок вышел из зоны сундука");
        }
    }

    // Визуализация триггера в редакторе
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Голубой с прозрачностью
        Collider collider = GetComponent<Collider>();
        
        if (collider != null && collider.isTrigger)
        {
            if (collider is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            }
            else if (collider is SphereCollider sphereCollider)
            {
                Gizmos.DrawSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Collider collider = GetComponent<Collider>();
        
        if (collider != null && collider.isTrigger)
        {
            if (collider is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
            else if (collider is SphereCollider sphereCollider)
            {
                Gizmos.DrawWireSphere(transform.position + sphereCollider.center, sphereCollider.radius);
            }
        }
        
        // Показываем связь с сундуком
        if (chestObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, chestObject.transform.position);
        }
    }
}