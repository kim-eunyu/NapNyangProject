using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LowHealthEffect : MonoBehaviour
{
    public Volume postProcessVolume;

    // --- [추가] --- Inspector에서 조절할 변수들
    [Header("효과 설정")]
    [Tooltip("비네팅이 최대로 강해지는 정도 (0~1)")]
    [Range(0f, 1f)] // 0과 1사이 값만 넣도록 슬라이더를 만들어줘요
    [SerializeField] private float maxIntensity = 0.6f;

    [Tooltip("효과가 깜빡이는 속도")]
    [SerializeField] private float pulseSpeed = 1.5f;
    // --- [추가 끝] ---

    private Vignette vignette;
    private Coroutine pulseRoutine;

    void Awake()
    {
        if (postProcessVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0f; // 시작할 때 확실히 끄기
        }
    }

    public void StartEffect()
    {
        if (pulseRoutine == null)
        {
            pulseRoutine = StartCoroutine(Pulse());
        }
    }

    public void StopEffect()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
            vignette.intensity.value = 0f;
        }
    }

    private IEnumerator Pulse()
    {
        // --- [삭제] --- 함수 안에 있던 변수 선언은 위로 옮겼으니 지워요
        // float maxIntensity = 0.6f;
        // float pulseSpeed = 1.5f;
        // --- [삭제 끝] ---

        while (true)
        {
            // 이제 Inspector에서 설정한 값을 사용해요
            float targetIntensity = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f * maxIntensity;
            vignette.intensity.value = targetIntensity;
            yield return null;
        }
    }
}