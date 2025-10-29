// 예시 스크립트 (PortalTeleporter.cs)
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    public Transform destinationPortal; // B포탈을 여기에 할당

    void OnTriggerEnter(Collider other)
    {
        // "Player" 태그를 가진 오브젝트가 들어왔다면
        if (other.CompareTag("Player"))
        {
            Debug.Log("포탈 탑승!");
            // 플레이어의 위치를 목적지 포탈의 위치로 순간이동!
            other.transform.position = destinationPortal.position;

            // (선택 사항) 플레이어가 B포탈에서 바로 A로 다시 돌아오는 걸 막기 위해
            // 약간의 딜레이를 주거나, 플레이어의 방향도 틀어주는 게 좋습니다.
            other.transform.rotation = destinationPortal.rotation; 
        }
    }
}