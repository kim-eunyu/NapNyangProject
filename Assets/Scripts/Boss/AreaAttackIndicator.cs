using System.Collections;
using UnityEngine;

public class AreaAttackIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    public Material indicatorMaterial;
    public Color warningColor = Color.red;
    public Color finalColor = new Color(1f, 0f, 0f, 0.8f);

    [Header("Particle Effects")]
    [Tooltip("광역 공격 준비 단계 파티클 이펙트")]
    public GameObject warningParticleEffect;
    [Tooltip("광역 공격 실행 단계 파티클 이펙트")]
    public GameObject explosionParticleEffect;
    [Tooltip("파티클 이펙트를 바닥에서 얼마나 위에 생성할지")]
    public float particleHeightOffset = 0.1f;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float warningDuration = 1f;
    public float fadeOutDuration = 0.3f;
    public bool pulseEffect = true;
    public float pulseSpeed = 3f;

    private GameObject indicatorObject;
    private Renderer indicatorRenderer;
    private Material materialInstance;
    private GameObject currentWarningParticle;
    private GameObject currentExplosionParticle;

    public static AreaAttackIndicator CreateIndicator(Vector3 center, float radius, float totalDuration = 1.8f)
    {
        // 빈 GameObject 생성
        GameObject indicatorParent = new GameObject("AreaAttackIndicator");
        indicatorParent.transform.position = center;

        // 원형 평면 생성
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.SetParent(indicatorParent.transform);
        cylinder.transform.localPosition = Vector3.up * 0.01f; // 바닥에서 살짝 위로
        cylinder.transform.localScale = new Vector3(radius * 2, 0.01f, radius * 2);

        // Collider 제거 (시각적 용도만)
        DestroyImmediate(cylinder.GetComponent<Collider>());

        // 컴포넌트 추가
        AreaAttackIndicator indicator = indicatorParent.AddComponent<AreaAttackIndicator>();
        indicator.indicatorObject = cylinder;
        indicator.indicatorRenderer = cylinder.GetComponent<Renderer>();

        // 기본 머티리얼 생성
        if (indicator.indicatorMaterial == null)
        {
            indicator.CreateDefaultMaterial();
        }

        indicator.materialInstance = new Material(indicator.indicatorMaterial);
        indicator.indicatorRenderer.material = indicator.materialInstance;

        // 애니메이션 시작
        indicator.StartCoroutine(indicator.PlayIndicatorAnimation(totalDuration));

        return indicator;
    }

    // 파티클 효과와 함께 인디케이터 생성 (오버로드 함수)
    public static AreaAttackIndicator CreateIndicatorWithParticles(Vector3 center, float radius,
        GameObject warningParticle = null, GameObject explosionParticle = null, float totalDuration = 1.8f)
    {
        AreaAttackIndicator indicator = CreateIndicator(center, radius, totalDuration);

        // 파티클 이펙트 설정
        indicator.warningParticleEffect = warningParticle;
        indicator.explosionParticleEffect = explosionParticle;

        return indicator;
    }

    void CreateDefaultMaterial()
    {
        indicatorMaterial = new Material(Shader.Find("Standard"));
        indicatorMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        indicatorMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        indicatorMaterial.SetInt("_ZWrite", 0);
        indicatorMaterial.DisableKeyword("_ALPHATEST_ON");
        indicatorMaterial.EnableKeyword("_ALPHABLEND_ON");
        indicatorMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        indicatorMaterial.renderQueue = 3000;
        indicatorMaterial.color = warningColor;
    }

    IEnumerator PlayIndicatorAnimation(float totalDuration)
    {
        float elapsedTime = 0f;
        Color startColor = warningColor;
        startColor.a = 0f;

        // 경고 단계 파티클 이펙트 생성
        if (warningParticleEffect != null)
        {
            Vector3 particlePosition = transform.position + Vector3.up * particleHeightOffset;
            currentWarningParticle = Instantiate(warningParticleEffect, particlePosition, transform.rotation);
        }

        // 1단계: 페이드 인
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeInDuration;

            Color currentColor = Color.Lerp(startColor, warningColor, progress);
            materialInstance.color = currentColor;

            yield return null;
        }

        // 2단계: 경고 단계 (펄스 효과)
        elapsedTime = 0f;
        while (elapsedTime < warningDuration)
        {
            elapsedTime += Time.deltaTime;

            if (pulseEffect)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.3f + 0.7f;
                Color pulseColor = warningColor;
                pulseColor.a *= pulse;
                materialInstance.color = pulseColor;
            }

            yield return null;
        }

        // 3단계: 최종 색상으로 변경 + 폭발 파티클
        materialInstance.color = finalColor;

        // 경고 파티클 제거
        if (currentWarningParticle != null)
        {
            Destroy(currentWarningParticle);
        }

        // 폭발 파티클 이펙트 생성
        if (explosionParticleEffect != null)
        {
            Vector3 particlePosition = transform.position + Vector3.up * particleHeightOffset;
            currentExplosionParticle = Instantiate(explosionParticleEffect, particlePosition, transform.rotation);
        }

        yield return new WaitForSeconds(0.2f);

        // 4단계: 페이드 아웃
        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeOutDuration;

            Color currentColor = Color.Lerp(finalColor, Color.clear, progress);
            materialInstance.color = currentColor;

            yield return null;
        }

        // 오브젝트 파괴
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (materialInstance != null)
        {
            DestroyImmediate(materialInstance);
        }

        // 파티클 정리
        if (currentWarningParticle != null)
        {
            Destroy(currentWarningParticle);
        }

        if (currentExplosionParticle != null)
        {
            Destroy(currentExplosionParticle);
        }
    }
}