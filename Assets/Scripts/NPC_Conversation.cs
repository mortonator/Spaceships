using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DialogueSystem;

[RequireComponent(typeof(DSDialogue))]
public class NPC_Conversation : iInteractable
{
    [SerializeField] string interactionText;
    public override string GetInteractionText() => interactionText;

    DSDialogueSO currentDialogue;
    DSDialogue dialogue;

    void Awake()
    {
        dialogue = GetComponent<DSDialogue>();
        currentDialogue = dialogue.GetStartingDialogue();
    }
    public override void Interact()
    {
        StartConversation();
    }

    void StartConversation()
    {
        Player_CharacterController.player.ui.Convo_Start(this);
        ShowText();
    }

    void ShowText()
    {
        Player_CharacterController.player.ui.Convo_ShowText(currentDialogue);
    }

    public void OnOptionChosen(int choiceIndex)
    {
        DSDialogueSO nextDialogue = currentDialogue.Choices[choiceIndex].NextChoice;

        if (nextDialogue == null)
        {
            Player_CharacterController.player.ui.Convo_End();
            return; // No more dialogues to show, do whatever you want, like setting the currentDialogue to the startingDialogue
        }

        currentDialogue = nextDialogue;

        ShowText();
    }
}
