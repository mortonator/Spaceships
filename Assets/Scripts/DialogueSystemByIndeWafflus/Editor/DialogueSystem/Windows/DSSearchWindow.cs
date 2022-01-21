using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem
{
    public class DSSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        DSGraphView graphView;
        Texture2D indentationIcon;

        public void Initalise(DSGraphView _graphView)
        {
            graphView = _graphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Element")),
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
            new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
            {
                level = 2,
                userData = DSDialogueType.SingleChoice
            },
            new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
            {
                level = 2,
                userData = DSDialogueType.MultipleChoice
            },
            new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
            new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
            {
                level = 2,
                userData = new Group()
            }
        };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            switch (SearchTreeEntry.userData)
            {
                case DSDialogueType.SingleChoice:
                    DSSingleChoiceNode singleChoiceNode = (DSSingleChoiceNode)graphView.CreateNode("DialogueName", DSDialogueType.SingleChoice, graphView.GetLocalMousePosition(context.screenMousePosition, true));
                    graphView.AddElement(singleChoiceNode);
                    return true;

                case DSDialogueType.MultipleChoice:
                    DSMultipleChoiceNode multpleChoiceNode = (DSMultipleChoiceNode)graphView.CreateNode("DialogueName", DSDialogueType.MultipleChoice, graphView.GetLocalMousePosition(context.screenMousePosition, true));
                    graphView.AddElement(multpleChoiceNode);
                    return true;

                case Group _group:
                    graphView.CreateGroup("Dialogue Group", graphView.GetLocalMousePosition(context.screenMousePosition, true));
                    return true;

                default:
                    return false;
            }
        }
    }
}
