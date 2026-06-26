using Unity.VisualScripting;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public int maxPunches;
    public int currentPunches;
    RespawnManager respawnManager;

    private void Awake()
    {
        respawnManager = FindFirstObjectByType<RespawnManager>();
        currentPunches = 0;
    }

    private void Update()
    {
        if (currentPunches >= maxPunches)
        {
            respawnManager.nowItems--;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            currentPunches++;
            if (currentPunches >= maxPunches)
            {
                respawnManager.nowItems--;
                Destroy(gameObject);
            }
        }
    }
}
