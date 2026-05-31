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

    [Header("Curriculum")]
    public bool randomizeAgentPos = false;
    public bool randomizeSwitch = false;
    public float roomRadius = 4f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private bool switchActivated = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
    }

    // Début de chaque épisode — reset la scène
    public override void OnEpisodeBegin()
    {
        switchActivated = false;
        door.SetDoorState(false);

        // Position agent fixe ou aléatoire selon le niveau de curriculum
        if (randomizeAgentPos)
            transform.localPosition = new Vector3(
                Random.Range(-roomRadius, roomRadius), 0.5f,
                Random.Range(-roomRadius, roomRadius));
        else
            transform.position = startPosition;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Position interrupteur fixe ou aléatoire selon le niveau de curriculum
        if (randomizeSwitch)
            switchTransform.localPosition = new Vector3(
                Random.Range(-roomRadius, roomRadius), 0.5f,
                Random.Range(-roomRadius, roomRadius));
    }

    // Observations transmises au réseau neuronal pour prendre des décisions
    public override void CollectObservations(VectorSensor sensor)
    {
        // Position locale de l'agent dans la salle (3 valeurs)
        sensor.AddObservation(transform.localPosition);

        // Direction et distance vers l'interrupteur (3 valeurs)
        // Permet à l'agent de savoir où se trouve l'interrupteur
        sensor.AddObservation(switchTransform.localPosition - transform.localPosition);

        // Direction et distance vers la porte (3 valeurs)
        // Permet à l'agent de savoir où se trouve la sortie
        sensor.AddObservation(doorTransform.localPosition - transform.localPosition);

        // État de la porte : 1 = ouverte, 0 = fermée (1 valeur)
        // indique à l'agent s'il doit chercher l'interrupteur ou la sortie
        sensor.AddObservation(door.IsOpen() ? 1f : 0f);

        // Total : 10 observations
    }

    // Actions exécutées à chaque step selon les décisions du réseau neuronal
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);

        if (!door.IsOpen())
        {
            float distToSwitch = Vector3.Distance(transform.position, switchTransform.position);
            AddReward(-distToSwitch * 0.0001f);
        }
        else
        {
            float distToDoor = Vector3.Distance(transform.position, exitZone.position);
            AddReward(-distToDoor * 0.0002f); // légèrement plus fort que switch
        }

        AddReward(-0.001f);
    }

    // Mode heuristique — contrôle manuel WASD pour tester la scène
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }

    void OnTriggerEnter(Collider other)
    {
        // L'agent active l'interrupteur pour la première fois
        //  une seule activation par épisode
        if (other.CompareTag("Switch") && !switchActivated)
        {
            switchActivated = true;
            door.SetDoorState(true);
            // Récompense positive pour avoir trouvé et activé l'interrupteur
            AddReward(1.0f);
        }

        // L'agent atteint l'ExitZone avec la porte ouverte
        if (other.CompareTag("ExitZone") && door.IsOpen())
        {
            // Grande récompense finale pour compléter l'objectif
            AddReward(10.0f);
            EndEpisode();
        }
    }
}