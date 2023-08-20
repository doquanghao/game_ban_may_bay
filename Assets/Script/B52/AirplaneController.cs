using System.Collections;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    public Vector3 target; // Điểm đến
    public float speed = 5.0f; // Tốc độ di chuyển
    public float bombingDistance = 2.0f; // Khoảng cách để ném bom
    public GameObject bombPrefab; // Prefab của bom
    public int maxBombs = 24; // Số lượng bom tối đa có thể ném

    private bool hasBombed = false; // Đã ném bom hay chưa
    private void Update()
    {
        // Di chuyển máy bay đến điểm đến
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target, step);

        // Kiểm tra xem máy bay đã đến điểm đến chưa
        if (transform.position == target)
        {
            //Đến rồi thì hủy
            Destroy(gameObject);
        }

        if (transform.position.x >= -200f && !hasBombed)
        {
            StartCoroutine(DropBomb());
        }
    }

    IEnumerator DropBomb()
    {
        hasBombed = true;
        for (int i = 0; i < maxBombs; i++)
        {
            Instantiate(bombPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
