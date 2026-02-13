using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Control : MonoBehaviour
{
    [SerializeField]
    private GameObject particles;

    private Vector3 mousePose;

    [SerializeField] public GameObject Chest1;
    [SerializeField] public GameObject DistillCube;
    public Vector3 objectPos;

    private void Start()
    {
        particles.SetActive(false);

        Chest1 = GameObject.FindGameObjectWithTag("Chest");
        objectPos = Chest1.transform.position;

        DistillCube = GameObject.FindGameObjectWithTag("DistillCube");
        objectPos = DistillCube.transform.position;
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit placeInfo;
            if (Physics.Raycast(ray, out placeInfo))
            {
                if (placeInfo.collider.CompareTag("Ground"))
                {
                    mousePose = new Vector3(placeInfo.point.x, transform.position.y, placeInfo.point.z);
                    particles.SetActive(true);
                    particles.transform.position = new Vector3(mousePose.x, 0.1f, mousePose.z);
                }

                if (placeInfo.collider.CompareTag("Chest"))
                {
                    mousePose = new Vector3(placeInfo.point.x, transform.position.y, placeInfo.point.z);
                    particles.SetActive(true);
                    particles.transform.position = new Vector3(Chest1.transform.position.x, 2f, Chest1.transform.position.z);
                }

                if (placeInfo.collider.CompareTag("DistillCube"))
                {
                    mousePose = new Vector3(placeInfo.point.x, transform.position.y, placeInfo.point.z);
                    particles.SetActive(true);
                    particles.transform.position = new Vector3(DistillCube.transform.position.x, 2f, DistillCube.transform.position.z);
                }
            }
        }
    }
}
