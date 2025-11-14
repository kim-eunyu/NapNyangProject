using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LowMentalEffect : MonoBehaviour
{
    [Header("정신력 비네팅 설정")]
    [Tooltip("정신력 효과용 Global Volume을 연결해주세요.")]
    [SerializeField] private Volume postProcessVolume;

    [Tooltip("비네팅이 최대로 강해지는 정도 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float maxIntensity = 0.5f;

    private Vignette vignette;

    void Awake()
    {
        if (postProcessVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0f; // 시작할 때 확실히 끄기
        }
        else
        {
            Debug.LogError("LowMentalEffect: Volume에 Vignette가 없습니다!");
        }
    }

    // --- [수정] ---
    // 0.0 (0%) ~ 1.0 (100%) 사이의 값을 받아서 비네팅 강도를 조절하는 함수
    public void UpdateEffect(float percentage)
    {
        if (vignette != null)
        {
            // (받은 퍼센트 값 * 최대 강도) 로 현재 강도를 설정해요
            vignette.intensity.value = percentage * maxIntensity;
        }
    }
    // --- [수정 끝] ---
}