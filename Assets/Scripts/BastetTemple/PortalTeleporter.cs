using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    // 인스펙터 창에서 이동할 '목적지'를 지정합니다.
    public Transform destinationPortal; 

    void OnTriggerEnter(Collider other)
    {
        // "Player" 태그를 가진 오브젝트가 들어왔다면
        if (other.CompareTag("Player"))
        {
            Debug.Log("포탈 탑승!");
            
            // 플레이어의 위치를 목적지 포탈의 위치로 순간이동!
            other.transform.position = destinationPortal.position;
                
            // 플레이어의 방향(Rotation)도 목적지가 바라보는 방향으로 설정
            // (이래야 무한 핑퐁을 막고, 도착해서 앞을 보게 됩니다)
            other.transform.rotation = destinationPortal.rotation; 
        }
    }
}