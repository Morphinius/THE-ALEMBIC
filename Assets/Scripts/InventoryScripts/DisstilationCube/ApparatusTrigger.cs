using UnityEngine;

public class ApparatusTrigger : MonoBehaviour
{
    [Header("Apparatus Reference")]
    [SerializeField] private GameObject apparatusObject; // Ссылка на объект аппарата
    
    private ApparatusInteraction apparatusInteraction;

    private void Start()
    {
        Debug.Log("ApparatusTrigger: Start");
        
        // Находим скрипт ApparatusInteraction на аппарате
        if (apparatusObject != null)
        {
            apparatusInteraction = apparatusObject.GetComponent<ApparatusInteraction>();
            if (apparatusInteraction != null)
            {
                Debug.Log("ApparatusTrigger: ApparatusInteraction найден");
            }
            else
            {
                Debug.LogError("ApparatusTrigger: ApparatusInteraction не найден на объекте аппарата!");
            }
        }
        else
        {
            Debug.LogError("ApparatusTrigger: ApparatusObject не назначен!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ApparatusTrigger: OnTriggerEnter с {other.name}, tag: {other.tag}, layer: {other.gameObject.layer}");
        
        if (other.CompareTag("Player"))
        {
            if (apparatusInteraction != null)
            {
                apparatusInteraction.SetPlayerInTrigger(true);
                Debug.Log("ApparatusTrigger: Игрок вошел в зону аппарата - SetPlayerInTrigger(true) вызван");
            }
            else
            {
                Debug.LogError("ApparatusTrigger: apparatusInteraction is null!");
            }
        }
        else
        {
            Debug.Log($"ApparatusTrigger: Объект не игрок. Тег: {other.tag}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"ApparatusTrigger: OnTriggerExit с {other.name}, tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            if (apparatusInteraction != null)
            {
                apparatusInteraction.SetPlayerInTrigger(false);
                Debug.Log("ApparatusTrigger: Игрок вышел из зоны аппарата - SetPlayerInTrigger(false) вызван");
            }
        }
    }

    // Визуализация триггера в редакторе
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
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
        Gizmos.color = new Color(1, 0.5f, 0, 1f);
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
        
        if (apparatusObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, apparatusObject.transform.position);
        }
    }
}