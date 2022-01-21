using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class DSGraphView : GraphView
    {
        DSSearchWindow searchWindow;
        DSEditorWindow editorWindow;

        MiniMap miniMap;

        SerializableDictionary<string, DSGroupErrorData> groups;
        SerializableDictionary<string, DSNodeErrorData> ungroupedNodes;
        SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>> groupedNodes;

        const float minimumZoomScale = 0.25f;
        const float maxmumZoomScale = 1.5f;

        int nameErrorsAmount;
        public int NameErrorsAmount 
        { 
            get 
            { 
                return nameErrorsAmount;
            }
            set
            {
                nameErrorsAmount = value;

                if (nameErrorsAmount == 0)
                {
                    editorWindow.EnableSaving();
                    return;
                }

                editorWindow.DisableSaving();
            }
        }

        public DSGraphView(DSEditorWindow _editorWindow)
        {
            editorWindow = _editorWindow;

            groups = new SerializableDictionary<string, DSGroupErrorData>();
            ungroupedNodes = new SerializableDictionary<string, DSNodeErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>>();

            AddManipulators();
            AddSearchWindow();
            AddMiniMap();
            AddGridBackground();

            OnElementDeleted();
            OnGroupElementAdded();
            OnGroupElementRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();
        }

        #region Overriden Methods
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                    return;

                if (startPort.node == port.node)
                    return;

                if (startPort.direction == port.direction)
                    return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        #endregion

        #region Manipulators
        void AddManipulators()
        {
            this.AddManipulator(new ContentZoomer()
            {
                minScale = minimumZoomScale,
                maxScale = maxmumZoomScale
            });
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Single Choice Node", DSDialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Multiple Choice Node", DSDialogueType.MultipleChoice));

            this.AddManipulator(CreateGroupContextualMenu("Add Multiple Choice Node"));
        }
        IManipulator CreateNodeContextualMenu(string actionTitle, DSDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("Dialogue Name", dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );
            return contextualMenuManipulator;
        }
        IManipulator CreateGroupContextualMenu(string actionTitle)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("Dialogue Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );
            return contextualMenuManipulator;
        }
        #endregion

        #region Elements Creation
        public DSGroup CreateGroup(string title, Vector2 position)
        {
            DSGroup group = new DSGroup(title, position);

            AddGroup(group);
            AddElement(group);

            foreach (GraphElement element in selection)
            {
                if ((element is DSNode) == false)
                    continue;

                DSNode node = (DSNode)element;

                group.AddElement(node);
            }

            return group;
        }

        public DSNode CreateNode(string nodeName, DSDialogueType dialogueType, Vector2 position, bool shouldDraw=true)
        {
            Type nodeType = Type.GetType($"DialogueSystem.DS{dialogueType}Node");

            DSNode node = (DSNode)Activator.CreateInstance(nodeType);
            node.Initialise(nodeName, position, this);

            if (shouldDraw)
                node.Draw();

            AddUngroupedNode(node);

            return node;
        }
        #endregion

        #region Callbacks
        void OnElementDeleted()
        {
            deleteSelection = (operatingSystem, askUser) =>
              {
                  Type groupType = typeof(DSGroup);
                  Type edgeType = typeof(Edge);

                  List<DSGroup> groupsToDelete = new List<DSGroup>();
                  List<DSNode> nodesToDelete = new List<DSNode>();
                  List<Edge> edgesToDelete = new List<Edge>();

                  foreach (GraphElement element in selection)
                  {
                      if (element is DSNode node)
                      {
                          nodesToDelete.Add(node);
                          continue;
                      }

                      if (element.GetType() != edgeType)
                      {
                          edgesToDelete.Add((Edge)element);
                          continue;
                      }

                      if (element.GetType() != groupType)
                          continue;

                      DSGroup group = (DSGroup)element;
                      groupsToDelete.Add(group);
                  }

                  foreach (DSGroup groupToDelete in groupsToDelete)
                  {
                      List<DSNode> groupNodes = new List<DSNode>();

                      foreach (GraphElement groupElement in groupToDelete.containedElements)
                      {
                          if ((groupElement is DSNode) == false)
                              continue;

                          groupNodes.Add((DSNode)groupElement);
                      }

                      groupToDelete.RemoveElements(groupNodes);
                      RemoveGroup(groupToDelete);
                      RemoveElement(groupToDelete);
                  }

                  DeleteElements(edgesToDelete);

                  foreach (DSNode nodeToDelete in nodesToDelete)
                  {
                      if (nodeToDelete.group != null)
                          nodeToDelete.group.RemoveElement(nodeToDelete);

                      RemoveUngroupedNode(nodeToDelete);
                      nodeToDelete.DisconnectAllPorts();
                      RemoveElement(nodeToDelete);
                  }
              };
        }

        void OnGroupElementAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                DSGroup dsGroup = (DSGroup)group;
                foreach (GraphElement element in elements)
                {
                    if ((element is DSNode) == false)
                        continue;

                    DSNode node = (DSNode)element;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(dsGroup, node);
                }
            };
        }
        void OnGroupElementRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
              {
                  DSGroup dsGroup = (DSGroup)group;
                  foreach (GraphElement element in elements)
                  {
                      if (element is DSNode node)
                      {
                          RemoveGroupedNode(dsGroup, node);
                          AddUngroupedNode(node);
                      }
                  }
              };
        }

        void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DSGroup dSGroup = (DSGroup)group;

                dSGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(dSGroup.title))
                {
                    if (!string.IsNullOrEmpty(dSGroup.oldTitle))
                        NameErrorsAmount++;
                }
                else
                {
                    if (string.IsNullOrEmpty(dSGroup.oldTitle))
                        NameErrorsAmount--;
                }

                RemoveGroup(dSGroup);
                dSGroup.oldTitle = dSGroup.title;
                AddGroup(dSGroup);
            };
        }

        void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DSNode nextNode = (DSNode)edge.input.node;
                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                            continue;

                        Edge edge = (Edge)element;
                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;
                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }
        #endregion

        #region Repeated Elements
        public void AddUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();
            if (ungroupedNodes.ContainsKey(nodeName) == false)
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                nodeErrorData.nodes.Add(node);

                ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].nodes;
            ungroupedNodesList.Add(node);

            Color errorColor = ungroupedNodes[nodeName].errorData.colour;
            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                NameErrorsAmount++;

                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }
        public void RemoveUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();
            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].nodes;
            ungroupedNodesList.Remove(node);

            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                NameErrorsAmount--;

                ungroupedNodesList[0].ResetStyle();
                return;
            }

            if (ungroupedNodesList.Count == 0)
                ungroupedNodes.Remove(nodeName);
        }

        public void AddGroupedNode(DSGroup group, DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();
            node.group = group;

            if (groupedNodes.ContainsKey(group) == false)
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DSNodeErrorData>());
            }

            if (groupedNodes[group].ContainsKey(nodeName) == false)
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();
                nodeErrorData.nodes.Add(node);

                groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].nodes;
            groupedNodesList.Add(node);

            Color errorColor = groupedNodes[group][nodeName].errorData.colour;
            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                NameErrorsAmount++;

                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }
        public void RemoveGroupedNode(DSGroup group, DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();
            node.group = null;

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].nodes;
            groupedNodesList.Remove(node);

            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                NameErrorsAmount--;

                groupedNodesList[0].ResetStyle();
                return;
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                    groupedNodes.Remove(group);
            }
        }

        void AddGroup(DSGroup group)
        {
            string groupTitle = group.title.ToLower();
            if (groups.ContainsKey(groupTitle) == false)
            {
                DSGroupErrorData groupErrorData = new DSGroupErrorData();
                groupErrorData.groups.Add(group);

                groups.Add(groupTitle, groupErrorData);

                return;
            }

            List<DSGroup> groupList = groups[groupTitle].groups;
            groupList.Add(group);

            Color errorColour = groups[groupTitle].errorData.colour;
            group.SetErrorStyle(errorColour);

            if (groupList.Count == 2)
                groupList[0].SetErrorStyle(errorColour);
        }
        void RemoveGroup(DSGroup group)
        {
            string groupOldTitle = group.oldTitle.ToLower();
            List<DSGroup> groupsList = groups[groupOldTitle].groups;
            groupsList.Remove(group);

            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                groupsList[0].ResetStyle();
                return;
            }

            if (groupsList.Count == 0)
                ungroupedNodes.Remove(groupOldTitle);

        }
        #endregion

        #region Elements Addition
        void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentSize();

            Insert(0, gridBackground);
        }

        void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 180));

            Add(miniMap);

            miniMap.visible = false;
        }

        void AddStyles()
        {
            this.AddStyleSheets("DSNodeStyles", "DSGraphViewStyles");
        }

        void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
                searchWindow.Initalise(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
        }
        #endregion

        #region Utilities
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            if (isSearchWindow)
                mousePosition -= editorWindow.position.position;

            return contentViewContainer.WorldToLocal(mousePosition);

            // Vector2 worldMousePosition = mousePosition;
            // Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            // return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            NameErrorsAmount = 0;
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }
        #endregion
    }
}