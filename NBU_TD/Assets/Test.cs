using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class Test : MonoBehaviour
{
    [SerializeField]
    private Camera Camera = null;
    [SerializeField]
    private LayerMask LayerMask;
    private NavMeshAgent Agent;
    public GameObject prefab;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Ray ray = Camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask))
            {
                Instantiate(prefab, hit.point, Quaternion.identity);
            }
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Camera = Camera.main;
            Ray ray = Camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, LayerMask))
            {
                Agent.enabled = true;
                Agent.SetDestination(hit.point);
            }
        }

    }
}