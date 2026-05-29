using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Material openMaterial;
    public Material closedMaterial;

    private bool isOpen = false;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        Debug.Log("Renderer trouvé : " + (rend != null));
        SetDoorState(false);
    }

    public void Toggle()
    {
        SetDoorState(!isOpen);
    }

    public void SetDoorState(bool open)
    {
        isOpen = open;
        Debug.Log("SetDoorState appelé : " + open);

        if (rend != null)
            rend.material = open ? openMaterial : closedMaterial;
        else
            Debug.LogError("Renderer est null !");

        GetComponent<Collider>().enabled = !open;
    }

    public bool IsOpen() => isOpen;
}