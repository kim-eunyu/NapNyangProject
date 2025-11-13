using UnityEngine;
using TMPro; // TextMeshPro를 쓰려면 이게 꼭 필요해용!

public class NicknameDisplay : MonoBehaviour
{
    [Header("닉네임을 표시할 텍스트")]
    public TMP_Text nicknameText; // (인스펙터에서 연결!)

    // 카메라를 저장할 변수
    private Camera mainCamera;

    void Start()
    {
        // 1. 게임이 시작되면 'MainCamera' 태그를 가진 카메라를 찾아서 저장!
        mainCamera = Camera.main;

        // 2. PlayerPrefs에서 "UserNickname" 키로 저장된 값을 불러옵니다.
        // (만약 저장된 값이 없으면, "Player"라는 기본 닉네임을 사용합니다)
        string savedNickname = PlayerPrefs.GetString("UserNickname", "Player");

        // 3. 텍스트 UI에 불러온 닉네임을 뙇! 표시합니다.
        if (nicknameText != null)
        {
            nicknameText.text = savedNickname;
        }
        else
        {
            Debug.LogError("NicknameDisplay 스크립트에 텍스트(TMP)가 연결 안 됐어용!");
        }
    }

    // LateUpdate는 모든 Update가 끝난 후, 주로 카메라 관련 처리에 사용돼용
    void LateUpdate()
    {
        // 4. (빌보드 기능) 캔버스가 항상 카메라를 바라보도록 만듭니다!
        if (mainCamera != null)
        {
            // 캔버스의 '앞면'이 카메라의 '앞면'과 같은 방향을 보도록 함
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}