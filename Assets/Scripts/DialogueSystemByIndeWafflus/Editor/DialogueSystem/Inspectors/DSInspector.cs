using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem
{
    [CustomEditor(typeof(DSDialogue))]
    public class DSInspector : Editor
    {
        // Dialogue Scriptable Objects
        SerializedProperty dialogueContainerProperty;
        SerializedProperty dialogueGroupProperty;
        SerializedProperty dialogueProperty;

        //Filters
        SerializedProperty groupedDialogueProperty;
        SerializedProperty startingDialoguesOnlyProperty;

        //Filters
        SerializedProperty selectedDialogueGroupIndexProperty;
        SerializedProperty selectedDialogueIndexProperty;

        void OnEnable()
        {
            dialogueContainerProperty = serializedObject.FindProperty("dialogueContainer");
            dialogueGroupProperty = serializedObject.FindProperty("dialogueGroup");
            dialogueProperty = serializedObject.FindProperty("dialogue");

            groupedDialogueProperty = serializedObject.FindProperty("groupedDialogues");
            startingDialoguesOnlyProperty = serializedObject.FindProperty("startingDialoguesOnly");

            selectedDialogueGroupIndexProperty = serializedObject.FindProperty("selectedDialogueGroupIndex");
            selectedDialogueIndexProperty = serializedObject.FindProperty("selectedDialogueIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDialogueContainerArea();
            DSDialogueContainerSO dialogueContainer = (DSDialogueContainerSO)dialogueContainerProperty.objectReferenceValue;

            if (dialogueContainer == null)
            {
                StopDrawing("Select a Dialogue Container to see the rest of the Inspector");
                return;
            }

            DrawFiltersArea();
            bool currentStartingDialoguesOnlyFilter = startingDialoguesOnlyProperty.boolValue;

            List<string> dialogueNames;
            string dialogueFolderPath = $"{DSIOUtility.RootDialoguePath}/DialogueSystem/Dialogues/{dialogueContainer.FileName}";
            string dialogueInfoMessage;

            if (groupedDialogueProperty.boolValue)
            {
                List<string> dialogueGroupNames = dialogueContainer.GetDialogueGroupNames();

                if (dialogueGroupNames.Count == 0)
                {
                    StopDrawing("There are no Dialogue Groups in this Dialogue Container.");
                    return;
                }

                DrawDialogueGroupArea(dialogueContainer, dialogueGroupNames);

                DSDialogueGroupSO dialogueGroup = (DSDialogueGroupSO)dialogueGroupProperty.objectReferenceValue;
                dialogueNames = dialogueContainer.GetGroupedDialogueNames(dialogueGroup, currentStartingDialoguesOnlyFilter);
                dialogueFolderPath += $"/Groups/{dialogueGroup.GroupName}/Dialogues";

                dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? " Starting" : "") + " Dialogues in this Dialogue Group.";
            }
            else
            {
                dialogueNames = dialogueContainer.GetUngroupedDialogueNames(currentStartingDialoguesOnlyFilter);
                dialogueFolderPath += "/Global/Dialogues";

                dialogueInfoMessage = "There are no" + (currentStartingDialoguesOnlyFilter ? " Starting" : "") + " Ungrouped Dialogues in this DialogueContianer.";
            }

            if (dialogueNames.Count == 0)
            {
                StopDrawing(dialogueInfoMessage);
                return;
            }

            DrawDialogueArea(dialogueNames, dialogueFolderPath);

            serializedObject.ApplyModifiedProperties();
        }

        void StopDrawing(string reason, MessageType messageType = MessageType.Info)
        {
            DSInspectorUtility.DrawHelpBox(reason, messageType);

            DSInspectorUtility.DrawSpace();

            DSInspectorUtility.DrawHelpBox("You need to select a Dialogue for this component to work properly at Runtime!", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }

        #region Draw Methods
        void DrawDialogueContainerArea()
        {
            DSInspectorUtility.DrawHeader("Dialogue Container");

            dialogueContainerProperty.DrawPropertyField();

            DSInspectorUtility.DrawSpace();
        }

        void DrawFiltersArea()
        {
            DSInspectorUtility.DrawHeader("Filters");

            groupedDialogueProperty.DrawPropertyField();
            startingDialoguesOnlyProperty.DrawPropertyField();

            DSInspectorUtility.DrawSpace();
        }

        void DrawDialogueGroupArea(DSDialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
        {
            DSInspectorUtility.DrawHeader("Dialogue Group");

            int oldSelectedDialogueGroupIndex = selectedDialogueGroupIndexProperty.intValue;

            DSDialogueGroupSO oldDialogueGroup = (DSDialogueGroupSO)dialogueGroupProperty.objectReferenceValue;
            bool isOldDialogueGroupNull = oldDialogueGroup == null;
            string oldDialogueGroupName = isOldDialogueGroupNull ? "" : oldDialogueGroup.GroupName;
            UpdateIndexOnNamesListUpdate(dialogueGroupNames, selectedDialogueGroupIndexProperty, oldSelectedDialogueGroupIndex, isOldDialogueGroupNull, oldDialogueGroupName);

            selectedDialogueGroupIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue Group", selectedDialogueGroupIndexProperty.intValue, dialogueGroupNames.ToArray());

            string selectedDialogueGroupName = dialogueGroupNames[selectedDialogueGroupIndexProperty.intValue];
            DSDialogueGroupSO selectedDialogueGroup = DSIOUtility.LoadAsset<DSDialogueGroupSO>($"{DSIOUtility.RootDialoguePath}/DialogueSystem/Dialogues/{dialogueContainer.FileName}/Groups/{selectedDialogueGroupName}", selectedDialogueGroupName);

            dialogueGroupProperty.objectReferenceValue = selectedDialogueGroup;
            DSInspectorUtility.DrawDisableFields(()=>dialogueGroupProperty.DrawPropertyField());

            DSInspectorUtility.DrawSpace();
        }

        void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
        {
            DSInspectorUtility.DrawHeader("Dialogue");

            int oldSelectedDialogueIndex = selectedDialogueIndexProperty.intValue;
            DSDialogueSO oldDialogue = (DSDialogueSO)dialogueProperty.objectReferenceValue;

            bool isOldDialogueNull = oldDialogue == null;
            string oldDialogueName = isOldDialogueNull ? "" : oldDialogue.DialogueName;
            UpdateIndexOnNamesListUpdate(dialogueNames, selectedDialogueIndexProperty, oldSelectedDialogueIndex, isOldDialogueNull, oldDialogueName);

            selectedDialogueIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue", selectedDialogueIndexProperty.intValue, dialogueNames.ToArray());

            string selectedDialogueName = dialogueNames[selectedDialogueIndexProperty.intValue];
            DSDialogueSO selectedDialogue = DSIOUtility.LoadAsset<DSDialogueSO>(dialogueFolderPath, selectedDialogueName);

            dialogueProperty.objectReferenceValue = selectedDialogue;
            DSInspectorUtility.DrawDisableFields(() => dialogueProperty.DrawPropertyField());
        }
        #endregion

        #region Index Methods
        private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty,  int oldSelectedPropertyIndex, bool isOldPropertyNull, string oldPropertyName)
        {
            if (isOldPropertyNull)
            {
                indexProperty.intValue = 0;
                return;
            }

            bool oldIndexIsOutOfBoundsOfNamesListCount = oldSelectedPropertyIndex > optionNames.Count - 1;
            bool oldNameIsDifferentThanSelectedName = oldPropertyName != optionNames[oldSelectedPropertyIndex];

            if (oldIndexIsOutOfBoundsOfNamesListCount || oldNameIsDifferentThanSelectedName)
            {
                if (optionNames.Contains(oldPropertyName))
                    indexProperty.intValue = optionNames.IndexOf(oldPropertyName);
                else
                    indexProperty.intValue = 0;
            }
        }
        #endregion
    }
}