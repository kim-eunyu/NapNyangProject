// MainQuestData.cs
using UnityEngine;
using System.Collections.Generic; // List를 쓰려면 이게 필요해용!

[CreateAssetMenu(fileName = "NewMainQuest", menuName = "Quests/Main Quest Data")]
public class MainQuestData : ScriptableObject
{
    public string questName; // 퀘스트 전체 제목 (예: "우중충냥의 숲")
    public List<QuestStep> steps; // 위에서 만든 퀘스트 단계들을 리스트로 담아용
}