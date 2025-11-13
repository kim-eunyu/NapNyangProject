// QuestStep.cs
using UnityEngine;

[System.Serializable] // 인스펙터 창에서 보이게 해줘용!
public class QuestStep
{
    public string stepDescription; // 단계별 설명 (예: "9개의 촛불 찾아서 불 붙이기")

    // 여기에 나중에 이 단계를 완료할 조건을 넣을 수 있어용. 
    // (예: public ObjectiveType objectiveType;)
}