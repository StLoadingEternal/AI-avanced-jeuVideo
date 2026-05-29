using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public DoorController door;
    public Material activeMaterial;   // vert quand activé
    public Material inactiveMaterial; // jaune par défaut

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material = inactiveMaterial;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Agent"))
        {
            door.Toggle();
            // Change couleur interrupteur
            rend.material = door.IsOpen() ? activeMaterial : inactiveMaterial;
        }
    }
}