using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; 
using UnityEngine.SceneManagement; 

public class LobbyManager : MonoBehaviour
{
    [Header("캐릭터 애니메이터")]
    public Animator catAnimator; // (인스펙터에서 고양이 연결!)

    [Header("UI 캔버스 그룹")]
    public GameObject lobbyCanvasGroup;   
    public GameObject nicknameCanvasGroup; 

    [Header("카메라 타겟")]
    public Camera mainCamera;          
    public Transform cameraStartTarget;   
    public Transform characterZoomTarget; 
    
    [Header("줌인 연출 시간")]
    public float zoomDuration = 2.0f;   

    [Header("닉네임 UI")]
    public TMP_InputField nicknameInputField; 
    public Button confirmButton;                  

    void Start()
    {
        if (mainCamera != null && cameraStartTarget != null)
        {
            mainCamera.transform.position = cameraStartTarget.position;
            mainCamera.transform.rotation = cameraStartTarget.rotation;
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnClickConfirmNickname);
        }
        
        if (nicknameCanvasGroup != null)
        {
            nicknameCanvasGroup.SetActive(false);
        }
    }

    // --- 5. '게임시작' 버튼 함수 (⭐여기가 수정됐어용!) ---
    public void OnClickGameStart()
    {
        // --- 1. (⭐수정!) "1초 뒤에" 'FireCatTrigger' 함수를 실행하라고 "예약"
        Invoke("FireCatTrigger", 1.0f); 

        // --- (삭제!) ---
        // (여기 있던 'catAnimator.SetTrigger("StartGame");'는 밑으로 이사갔어용!)
        // ------------------

        // 2. 기존 로비 UI를 숨깁니다. (이건 바로 실행!)
        if (lobbyCanvasGroup != null)
        {
            lobbyCanvasGroup.SetActive(false);
        }
        
        // 3. (안전장치)
        if (mainCamera == null || cameraStartTarget == null || characterZoomTarget == null)
        {
            Debug.LogError("LobbyManager에 카메라 또는 타겟 오브젝트가 연결되지 않았어용!");
            return; 
        }

        // 4. 카메라 연출 코루틴을 실행! (이것도 바로 실행!)
        StartCoroutine(GameStartSequence());
    }

    // --- (⭐새로 추가!) 1초 뒤에 실제로 실행될 함수 ---
    private void FireCatTrigger()
    {
        if (catAnimator != null)
        {
            catAnimator.SetTrigger("StartGame"); // 1초 뒤에 "StartGame" 트리거 발동!
            Debug.Log("1초 지남! 고양이 트리거 발사!"); // (확인용 로그!)
        }
    }
    // ---------------------------------------------

    // --- 6. (빙글 회전) 카메라 연출 시퀀스 (코루틴) ---
    private IEnumerator GameStartSequence()
    {
        float timer = 0f;
        
        Vector3 startPos = cameraStartTarget.position;
        Quaternion startRot = cameraStartTarget.rotation;
        
        Vector3 endPos = characterZoomTarget.position;
        Quaternion endRot = characterZoomTarget.rotation;

        while (timer < zoomDuration)
        {
            float t = timer / zoomDuration;
            float smoothT = t * t * (3f - 2f * t); 

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, smoothT);

            Quaternion slerpRotation = Quaternion.Slerp(startRot, endRot, smoothT);
            float spinAngle = Mathf.Lerp(0f, 360f, smoothT);
            Quaternion extraSpin = Quaternion.Euler(0f, spinAngle, 0f); 

            mainCamera.transform.rotation = slerpRotation * extraSpin;
            
            timer += Time.deltaTime;
            yield return null; 
        }
        
        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;

        // --- 7. 닉네임 UI 등장 ---
        if (nicknameCanvasGroup != null)
        {
            nicknameCanvasGroup.SetActive(true);
            
            if (nicknameInputField != null)
            {
                nicknameInputField.ActivateInputField();
            }
        }
    }

    // --- 8. 닉네임 '확인' 버튼을 눌렀을 때 ---
    private void OnClickConfirmNickname()
    {
        string userNickname = "Player"; 

        if (nicknameInputField != null && !string.IsNullOrWhiteSpace(nicknameInputField.text))
        {
            userNickname = nicknameInputField.text;
        }

        PlayerPrefs.SetString("UserNickname", userNickname);
        PlayerPrefs.Save(); 

        Debug.Log("닉네임 '" + userNickname + "' (이)가 저장되었습니다!");

        if (nicknameCanvasGroup != null)
        {
            nicknameCanvasGroup.SetActive(false);
        }

        SceneManager.LoadScene("NapNyang_GameScene002"); 
    }
}