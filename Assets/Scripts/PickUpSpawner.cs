using UnityEngine;
using UnityEngine.Serialization;

public class PickupSpawner : MonoBehaviour
{
    [FormerlySerializedAs("goinCoinPrefab")]
    [SerializeField] private GameObject goldCoinPrefab;
    [SerializeField] private GameObject heartPickupPrefab;

    public void DropItems()
    {
        int roll = Random.Range(1, 101); // 1..100

        if (roll <= 30) // 30% Heart
        {
            // Nếu full HP thì không rơi Heart
            if (PlayerHealth.Instance != null && PlayerHealth.Instance.CurrentHealth >= PlayerHealth.Instance.MaxHealth)
                return;

            if (heartPickupPrefab != null)
                Instantiate(heartPickupPrefab, transform.position, Quaternion.identity);
        }
        else if (roll <= 80) // 50% Gold (31..80)
        {
            if (goldCoinPrefab != null)
            {
                GameObject coin = Instantiate(goldCoinPrefab, transform.position, Quaternion.identity);
                if (coin.TryGetComponent(out Pickup pickup))
                {
                    pickup.SetAmount(Random.Range(1, 4)); // 1..3
                }
            }
        }
        // 20% còn lại không rơi gì
    }
}
