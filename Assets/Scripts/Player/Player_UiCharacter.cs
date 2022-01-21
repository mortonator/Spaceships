using DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player_UiCharacter : MonoBehaviour
{
    [SerializeField] GameObject gameScreen;
    [SerializeField] GameObject pauseScreen;
    [Space]
    [SerializeField] GameObject hudScreen;
    [SerializeField] GameObject dialogueScreen;
    [Space]
    [SerializeField] InputHandler playerInput;
    [SerializeField] UnityEngine.EventSystems.EventSystem eventSystem;
    [Space]
    [SerializeField] TMP_Text interactText;
    [SerializeField] GameObject continuePauseButton;
    [Space]
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject[] choicesButton;
    [SerializeField] TextMeshProUGUI[] choicesText;

    bool isPaused;

    public void SetInteractText(string text)
    {
        interactText.text = text;
    }

    public void Pause(UnityEngine.InputSystem.InputAction.CallbackContext c)
    {
        if(c.started)
        {
            isPaused = !isPaused;

            gameScreen.SetActive(!isPaused);
            pauseScreen.SetActive(!isPaused);

            playerInput.SetEnable(!isPaused);

            if (isPaused)
                eventSystem.SetSelectedGameObject(continuePauseButton);
        }
    }

    #region Dialogue
    NPC_Conversation conversation;
    public void Convo_Start(NPC_Conversation _conversation)
    {
        conversation = _conversation;

        hudScreen.SetActive(false);
        dialogueScreen.SetActive(true);

        playerInput.SetEnable(false);
    }
    public void Convo_End()
    {
        conversation = null;

        hudScreen.SetActive(true);
        dialogueScreen.SetActive(false);

        playerInput.SetEnable(true);
    }

    public void Convo_ShowText(DSDialogueSO currentDialogue)
    {
        dialogueText.text = currentDialogue.Text;

        int count = currentDialogue.Choices.Count;
        eventSystem.SetSelectedGameObject(choicesButton[count - 1]);

        for (int choiceIndex = 1; choiceIndex <= count; choiceIndex++)
        {
            choicesButton[count - choiceIndex].SetActive(true); // Places the last at the bottom
            choicesText[count - choiceIndex].text = currentDialogue.Choices[count - choiceIndex].Text;
        }

        for (int choiceIndex = count; choiceIndex < 10; choiceIndex++) 
        {
            choicesButton[choiceIndex].SetActive(false);
        }
    }
    public void Convo_Response(int index)
    {
        conversation.OnOptionChosen(index);
    }
    #endregion
}
