using System.Collections;
using UnityEngine;

public class AirplaneSpawner : MonoBehaviour
{
    public GameObject airplanePrefab; // Prefab của máy bay
    public GameObject target; // Prefab của máy bay
    public int maxAirplanes = 5; // Số lượng máy bay tối đa trong mỗi đợt

    private void Start()
    {
        StartCoroutine(SpawnAirplanes());
    }

    IEnumerator SpawnAirplanes()
    {
        while (true)
        {
            int airplanesToSpawn = Random.Range(1, maxAirplanes + 1); // Số lượng máy bay trong đợt
            for (int i = 0; i < airplanesToSpawn; i++)
            {
                SpawnAirplane();
            }

            yield return new WaitForSeconds(Random.Range(10f, 20f)); // Khoảng thời gian giữa các lần sinh máy bay
        }
    }

    void SpawnAirplane()
    {
        // Tạo máy bay mới tại vị trí ngẫu nhiên trong khu vực xuất hiện
        Vector3 spawnPosition = new Vector3(
            Random.Range(transform.position.x - 200, transform.position.x),
            Random.Range(transform.position.y, 20 + transform.position.y),
            Random.Range(transform.position.z - 200, transform.position.z + 200)
        );
        Vector3 targetPosition = new Vector3(
            Random.Range(target.transform.position.x, target.transform.position.x + 200),
            Random.Range(target.transform.position.y, 20 + target.transform.position.y),
            Random.Range(target.transform.position.z - 200, target.transform.position.z + 200)
        );
        Quaternion spawnRotation = airplanePrefab.transform.rotation; // Lấy rotation của prefab
        GameObject gameObject = Instantiate(airplanePrefab, spawnPosition, spawnRotation);
        gameObject.GetComponent<AirplaneController>().target = targetPosition;
    }
}
