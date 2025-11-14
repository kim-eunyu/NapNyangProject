using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light candleLight;

    [SerializeField] private float minIntensity = 0.8f; // 최소 밝기
    [SerializeField] private float maxIntensity = 1.2f; // 최대 밝기
    [SerializeField] private float flickerSpeed = 0.1f; // 얼마나 자주 바뀔지

    private float timer;

    void Start()
    {
        // 이 스크립트가 붙어있는 오브젝트의 Light 컴포넌트를 가져옴
        candleLight = GetComponent<Light>();
        timer = flickerSpeed;
    }

    void Update()
    {
        timer -= Time.deltaTime; // 타이머 감소

        // 타이머가 0이 되면
        if (timer <= 0)
        {
            // 최소~최대 밝기 사이에서 랜덤한 값으로 밝기를 변경
            candleLight.intensity = Random.Range(minIntensity, maxIntensity);

            // 타이머 초기화
            timer = flickerSpeed; 
        }
    }
}