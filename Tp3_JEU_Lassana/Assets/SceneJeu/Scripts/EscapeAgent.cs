using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class EscapeAgent : Agent
{
    [Header("Références")]
    public DoorController door;
    public Transform switchTransform;
    public Transform doorTransform;
    public Transform exitZone;

    [Header("Paramètres")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 100f;

    private Rigidbody rb;
    private Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    // Appelé au début de chaque épisode
    public override void OnEpisodeBegin()
    {
        // Reset position de l'agent
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset la porte
        door.SetDoorState(false);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // Position de l'agent (3 valeurs)
        sensor.AddObservation(transform.localPosition);

        // Position de l'interrupteur relative à l'agent (3 valeurs)
        sensor.AddObservation(switchTransform.localPosition - transform.localPosition);

        // Position de la porte relative à l'agent (3 valeurs)
        sensor.AddObservation(doorTransform.localPosition - transform.localPosition);

        // Est-ce que la porte est ouverte ? (1 valeur)
        sensor.AddObservation(door.IsOpen() ? 1f : 0f);

        // Total : 10 observations
    }

    // ce que l'agent peut faire
    public override void OnActionReceived(ActionBuffers actions)
{
    float moveX = actions.ContinuousActions[0];
    float moveZ = actions.ContinuousActions[1];

    Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
    rb.MovePosition(transform.position + move);

    AddReward(-0.001f);
}

public override void Heuristic(in ActionBuffers actionsOut)
{
    var actions = actionsOut.ContinuousActions;
    actions[0] = Input.GetAxis("Horizontal"); // gauche/droite
    actions[1] = Input.GetAxis("Vertical");   // avant/arrière

}

    void OnTriggerEnter(Collider other)
{
    Debug.Log("=== TRIGGER ENTER === : " + other.gameObject.name + " | tag: " + other.tag);

    if (other.CompareTag("Switch"))
    {
        Debug.Log("Switch trouvé ! Toggle porte...");
        if (door != null)
            door.Toggle();
        else
            Debug.LogError("Door est NULL !");
            
        AddReward(0.3f);
    }

    if (other.CompareTag("ExitZone"))
    {
        Debug.Log("ExitZone ! Porte ouverte: " + door.IsOpen());
        if (door.IsOpen())
        {
            AddReward(1.0f);
            EndEpisode();
        }
        else
        {
            AddReward(-0.5f);
        }
    }
}
}