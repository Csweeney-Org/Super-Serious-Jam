using UnityEngine;
using System.Collections;

public class RespawnManager : MonoBehaviour
{
    public GameObject[] prefabsItems;
    public Transform[] spawnPoints;

    public int nowItems;
    public int maxItems = 10;

    private bool isRespawning = false;

    private void Awake()
    {
        nowItems = 0;
        SpawnItems(maxItems);
    }

    private void Update()
    {
        if (nowItems < maxItems && !isRespawning)
        {
            int itemsToSpawn = maxItems - nowItems;
            StartCoroutine(RespawnWithDelay(itemsToSpawn));
        }

    }

    IEnumerator RespawnWithDelay(int amount)
    {
        isRespawning = true;

        float delay = Random.Range(1f, 15f);
        yield return new WaitForSeconds(delay);

        SpawnItems(amount);

        isRespawning = false; 
    }

    void SpawnItems(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            int instanceID = Random.Range(0, prefabsItems.Length);
            int spawnIndex = Random.Range(0, spawnPoints.Length);

            Instantiate(prefabsItems[instanceID], spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            nowItems++;
        }
    }

}
