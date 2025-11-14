using UnityEngine;

public class PetStoreNPCDialogue : MonoBehaviour
{
    [SerializeField] private GameObject dialogueWindow;

    private void OnMouseDown()
    {
        // ✨ 1. "DialogueWindow" 대신 "UIPopup" 태그를 찾는다용!
        GameObject[] allPopups = GameObject.FindGameObjectsWithTag("UIPopup");

        // ✨ 2. 찾은 모든 팝업창을 싹 다 끈다!
        foreach (GameObject popup in allPopups)
        {
            popup.SetActive(false);
        }

        // ✨ 3. 그 다음에 내 대화창을 켠다!
        if (dialogueWindow != null)
        {
            dialogueWindow.SetActive(true);
        }
    }
}