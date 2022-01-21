using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    public class DSDialogue : MonoBehaviour
    {
        // Dialogue ScriptableObjects
        [SerializeField] DSDialogueContainerSO dialogueContainer;
        [SerializeField] DSDialogueGroupSO dialogueGroup;
        [SerializeField] DSDialogueSO dialogue;

        // Filters
        [SerializeField] bool groupedDialogues;
        [SerializeField] bool startingDialoguesOnly;

        // Indexes
        [SerializeField] int selectedDialogueGroupIndex;
        [SerializeField] int selectedDialogueIndex;

        public DSDialogueSO GetStartingDialogue() => dialogue;
    }
}
