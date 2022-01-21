using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class DSMultipleChoiceNode : DSNode
    {
        public override void Initialise(string nodeName, Vector2 position, DSGraphView _graphView)
        {
            base.Initialise(nodeName, position, _graphView);

            DialogueType = DSDialogueType.MultipleChoice;

            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "Next Dialogue"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            Button addChoiceButton = DSElementUtility.CreateButton("Add Choice", () =>
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = "Next Dialogue"
                };
                Choices.Add(choiceData);

                Port choicePort = CreateChoicePort(choiceData);
                outputContainer.Add(choicePort);
            });
            addChoiceButton.AddToClassList("ds-node__button");
            mainContainer.Insert(1, addChoiceButton);

            foreach (DSChoiceSaveData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }

        #region Element Creation
        Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            DSChoiceSaveData choiceData = (DSChoiceSaveData)userData;

            Button deleteChoiceButton = DSElementUtility.CreateButton("X", ()=>
            {
                if (Choices.Count == 1) 
                    return;

                if (choicePort.connected)
                    graphView.DeleteElements(choicePort.connections);

                Choices.Remove(choiceData);
                graphView.RemoveElement(choicePort);
            });
            deleteChoiceButton.AddToClassList("ds-node__button");

            TextField choiceTextField = DSElementUtility.CreateTextField(choiceData.Text, null, callback=>
            {
                choiceData.Text = callback.newValue;
            });
            choiceTextField.AddClasses("ds-node__textfield", "ds-node__choice-textfield", "ds-node__textfield_hidden");

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
        #endregion
    }
}