using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class DSNode : Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<DSChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }
        protected DSGraphView graphView;
        public DSGroup group { get; set; }

        static Color defaultBackgroundColour = new Color(29f / 255, 29f / 255, 30f / 255);

        public virtual void Initialise(string nodeName, Vector2 position, DSGraphView _graphView)
        {
            ID = System.Guid.NewGuid().ToString();
            DialogueName = nodeName;
            Choices = new List<DSChoiceSaveData>();
            Text = "Dialogue text.";

            SetPosition(new Rect(position, Vector2.zero));
            graphView = _graphView;

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extenstion-container");
        }

        #region Overriden Methods
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }
        #endregion

        public virtual void Draw()
        {
            TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName, "", callback =>
            {
                TextField target = (TextField)callback.target;
                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName))
                        graphView.NameErrorsAmount++;
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName))
                        graphView.NameErrorsAmount--;
                }

                if (group == null)
                {
                    graphView.RemoveUngroupedNode(this);
                    DialogueName = target.value;
                    graphView.AddUngroupedNode(this);

                    return;
                }

                DSGroup currentGroup = group;
                graphView.RemoveGroupedNode(group, this);
                DialogueName = target.value;
                graphView.AddGroupedNode(currentGroup, this);
            });
            dialogueNameTextField.AddClasses("ds-node__textfield", "ds-node__filename-textfield", "ds-node__textfield_hidden");
            titleContainer.Insert(0, dialogueNameTextField);


            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);


            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Text");

            TextField textTextField = DSElementUtility.CreateTextArea(Text, null, callback=>
            {
                Text = callback.newValue;
            });
            textTextField.AddClasses("ds-node__textfield", "ds-node__quote-textfield");

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
        }

        #region Utility Methods
        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }
        void DisconnectInputPorts()=>DisconnectPorts(inputContainer);
        void DisconnectOutputPorts()=>DisconnectPorts(outputContainer);
        void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if(port.connected==false)
                    continue;

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();

            return inputPort.connected == false;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }
        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColour;
        }
#endregion
    }
}