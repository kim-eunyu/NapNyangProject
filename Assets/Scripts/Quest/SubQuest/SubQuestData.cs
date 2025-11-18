// 파일 이름: SubQuestData.cs
using UnityEngine;
using System.Collections.Generic;

// [System.Serializable]은 인스펙터 창에서 'SubQuestObjective' 클래스를
// 편집할 수 있게 해줘용.
[System.Serializable]
public class SubQuestObjective
{
    public MonsterType monsterType; // 예: Slime
    public int requiredAmount;    // 예: 3
}

[CreateAssetMenu(fileName = "NewSubQuest", menuName = "Quests/Sub Quest Data")]
public class SubQuestData : ScriptableObject
{
    public string questName; // 예: "몬스터 사냥꾼"
    
    [TextArea(3, 10)]
    public string description; // 예: "숲의 모든 몬스터를 3마리씩 처치하자."

    // "몬스터 3마리씩" 같은 '병렬' 목표들을 리스트로 관리해용.
    public List<SubQuestObjective> objectives; 

    // --- [!!! 바로 여기! 이 변수를 '추가'했어용 !!!] ---
    [Header("Completion")]
    [TextArea(3, 5)]
    public string completionMessage = "<b>퀘스트 완료!</b>\n\n보상을 받으러 돌아가자.";
    // --- [!!! 추가된 부분 끝 !!!] ---
}