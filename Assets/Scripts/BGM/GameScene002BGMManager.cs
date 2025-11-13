using UnityEngine;
using System.Collections;

public class GameScene002BGMManager : MonoBehaviour
{
    [Header("BGM 플레이어 (Audio Source 2개 연결!)")]
    public AudioSource bgmSourceA;
    public AudioSource bgmSourceB;

    [Header("BGM 오디오 클립 (음악 파일 연결!)")]
    public AudioClip mainBGM;
    public AudioClip teleportBGM;

    [Header("페이드 설정")]
    public float fadeDuration = 2.0f;

    // --- (⭐새로 추가된 부분!) ---
    [Header("BGM 볼륨 설정")]
    [Tooltip("메인 BGM의 최대 볼륨 (0.0 ~ 1.0)")]
    [Range(0f, 1f)] // (⭐이걸 넣으면 인스펙터에서 슬라이더로 조절할 수 있어용!)
    public float mainBGMMaxVolume = 1.0f; // (기본값은 1.0)

    [Tooltip("텔레포트 BGM의 최대 볼륨 (0.0 ~ 1.0)")]
    [Range(0f, 1f)] // (⭐이것도 슬라이더 짠!)
    public float teleportBGMMaxVolume = 1.0f; // (기본값은 1.0)
    // --- (여기까지!) ---


    // 현재 실행 중인 페이드 코루틴
    private Coroutine fadeACoroutine;
    private Coroutine fadeBCoroutine;

    // --- 1. 씬이 시작될 때 ---
    void Start()
    {
        if (mainBGM != null) bgmSourceA.clip = mainBGM;
        if (teleportBGM != null) bgmSourceB.clip = teleportBGM;

        // 씬이 시작됐으니, 메인 BGM(A)을 페이드 인 시킵니다!
        SwitchToMainBGM(); // (이 함수가 똑똑해졌으니 그냥 호출!)
    }

    // --- 2. 텔레포트 존에서 이 함수를 호출! (⭐수정됨!) ---
    public void SwitchToTeleportBGM()
    {
        Debug.Log("텔레포트 BGM으로 교체!");

        // 메인(A) BGM은 끕니다 (페이드 아웃)
        StartFade(bgmSourceA, 0.0f, ref fadeACoroutine);

        // (⭐수정!) 텔레포트(B) BGM을 '1.0f'가 아니라, 으뉴님이 설정한 '최대 볼륨'까지 켭니다!
        StartFade(bgmSourceB, teleportBGMMaxVolume, ref fadeBCoroutine);
    }

    // --- 3. (보너스) 텔레포트 존에서 나올 때! (⭐수정됨!) ---
    public void SwitchToMainBGM()
    {
        Debug.Log("메인 BGM으로 복귀!");

        // 텔레포트(B) BGM은 끕니다 (페이드 아웃)
        StartFade(bgmSourceB, 0.0f, ref fadeBCoroutine);

        // (⭐수정!) 메인(A) BGM을 '1.0f'가 아니라, 으뉴님이 설정한 '최대 볼륨'까지 켭니다!
        StartFade(bgmSourceA, mainBGMMaxVolume, ref fadeACoroutine);
    }

    // --- (핵심 마법!) 페이드를 실행하는 코루틴 함수 ---
    // (이 아래쪽 코드는 수정할 필요 없이 똑같아용!)

    private void StartFade(AudioSource source, float targetVolume, ref Coroutine runningCoroutine)
    {
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }
        if (targetVolume > 0 && !source.isPlaying)
        {
            source.volume = 0;
            source.Play();
        }
        runningCoroutine = StartCoroutine(FadeAudio(source, targetVolume));
    }

    private IEnumerator FadeAudio(AudioSource audioSource, float targetVolume)
    {
        float currentTime = 0;
        float startVolume = audioSource.volume;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;
        if (targetVolume == 0.0f)
        {
            audioSource.Stop();
        }
    }
}