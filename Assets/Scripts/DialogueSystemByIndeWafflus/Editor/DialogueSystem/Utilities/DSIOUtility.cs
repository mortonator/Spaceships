using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem
{
    public static class DSIOUtility
    {
        public const string RootDialoguePath = "Assets/Scripts/DialogueSystemByIndeWafflus";

        static DSGraphView graphView;
        static string graphFileName;
        static string containerFolderPath;

        static List<DSGroup> groups;
        static List<DSNode> nodes;

        static Dictionary<string, DSDialogueGroupSO> createdDialogueGroups;
        static Dictionary<string, DSDialogueSO> createdDialogue;

        static Dictionary<string, DSGroup> loadedGroups;
        static Dictionary<string, DSNode> loadedNodes;

        public static void Initialise(DSGraphView dsGraphView, string graphName)
        {
            graphView = dsGraphView;

            graphFileName = graphName;
            containerFolderPath = $"{RootDialoguePath}/DialogueSystem/Dialogues/{graphFileName}";

            groups = new List<DSGroup>();
            nodes = new List<DSNode>();

            createdDialogueGroups = new Dictionary<string, DSDialogueGroupSO>();
            createdDialogue = new Dictionary<string, DSDialogueSO>();

            loadedGroups = new Dictionary<string, DSGroup>();
            loadedNodes = new Dictionary<string, DSNode>();
        }

        #region Save Methods
        public static void Save()
        {
            CreateStaticFoldes();

            GetElementsFromGraphView();

            DSGraphSaveDataSO graphData = CreateAsset<DSGraphSaveDataSO>($"{RootDialoguePath}/Editor/DialogueSystem/Graphs", $"{graphFileName}Graph");
            graphData.Initialise(graphFileName);

            DSDialogueContainerSO dialogueContainer = CreateAsset<DSDialogueContainerSO>(containerFolderPath, graphFileName);
            dialogueContainer.Initialise(graphFileName);

            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);

            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }
        #endregion

        #region Load Methods
        public static void Load()
        {
            DSGraphSaveDataSO graphData = LoadAsset<DSGraphSaveDataSO>($"{RootDialoguePath}/Editor/DialogueSystem/Graphs", graphFileName);
            if (graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Couldn't load the file!",
                    "The file at the following path could not be found: \n\n" +
                    $"{RootDialoguePath}/Editor/DialogueSystem/Graphs/{graphFileName}\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Thanks!"
                    );

                return;
            }

            DSEditorWindow.UpdateFileName(graphData.FileName);

            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodeConnections();
        }

        static void LoadGroups(List<DSGroupSaveData> groups)
        {
            foreach (DSGroupSaveData groupData in groups)
            {
                DSGroup group = graphView.CreateGroup(groupData.Name, groupData.Position);

                group.ID = groupData.ID;

                loadedGroups.Add(group.ID, group);
            }
        }

        static void LoadNodes(List<DSNodeSaveData> nodes)
        {
            foreach (DSNodeSaveData nodeData in nodes)
            {
                List<DSChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
                DSNode node = graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);

                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;

                node.Draw();

                graphView.AddElement(node);
                loadedNodes.Add(node.ID, node);

                if (string.IsNullOrEmpty(nodeData.GroupID))
                    continue;

                DSGroup group = loadedGroups[nodeData.GroupID];
                node.group = group;
                group.AddElement(node);

            }
        }

        private static void LoadNodeConnections()
        {
            foreach (KeyValuePair<string, DSNode> loadedNode in loadedNodes)
            {
                foreach (Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    DSChoiceSaveData choiceData = (DSChoiceSaveData)choicePort.userData;

                    if (string.IsNullOrEmpty(choiceData.NodeID))
                        continue;

                    DSNode nextNode = loadedNodes[choiceData.NodeID];
                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();

                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                    graphView.AddElement(edge);

                    loadedNode.Value.RefreshPorts();
                }
            }
        }
        #endregion

        #region Groups
        static void SaveGroups(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();

            foreach (DSGroup group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);

                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames ,graphData);
        }

        static void SaveGroupToGraph(DSGroup group, DSGraphSaveDataSO graphData)
        {
            graphData.Groups.Add(
                new DSGroupSaveData()
                {
                    ID = group.ID,
                    Name = group.title,
                    Position = group.GetPosition().position
                }
            );
        }

        static void SaveGroupToScriptableObject(DSGroup group, DSDialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;

            CreateFolder($"{containerFolderPath}/Groups", groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            DSDialogueGroupSO dialogueGroup = CreateAsset<DSDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);
            dialogueGroup.Initialise(groupName);

            createdDialogueGroups.Add(group.ID, dialogueGroup);

            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<DSDialogueSO>());

            SaveAsset(dialogueGroup);
        }

        static void UpdateOldGroups(List<string> currentGroupNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

                foreach (string groupToRemove in groupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupNames);
        }
        #endregion

        #region Nodes
        static void SaveNodes(DSGraphSaveDataSO graphData, DSDialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (DSNode node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if (node.group != null)
                {
                    groupedNodeNames.AddItem(node.group.title, node.DialogueName);
                    continue;
                }

                ungroupedNodeNames.Add(node.DialogueName);
            }

            UpdateDialogueChoicesConnections();

            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }


        static void SaveNodeToGraph(DSNode node, DSGraphSaveDataSO graphData)
        {
            List<DSChoiceSaveData> choices = CloneNodeChoices(node.Choices);

            DSNodeSaveData nodeData = new DSNodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };

            graphData.Nodes.Add(nodeData);
        }

        static void SaveNodeToScriptableObject(DSNode node, DSDialogueContainerSO dialogueContainer)
        {
            DSDialogueSO dialogue;

            if (node.group != null)
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Groups/{node.group.title}/Dialogues", node.DialogueName);
                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<DSDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);
                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }

            dialogue.Initialise(
                node.DialogueName,
                node.Text,
                ConvertNodeChoicesToDialogueChoices(node.Choices),
                node.DialogueType,
                node.IsStartingNode()
            );

            createdDialogue.Add(node.ID, dialogue);

            SaveAsset(dialogue);
        }

        static List<DSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSDialogueChoiceData> dialogueChoices = new List<DSDialogueChoiceData>();

            foreach (DSChoiceSaveData nodeChoice in nodeChoices)
            {
                DSDialogueChoiceData choiceData = new DSDialogueChoiceData()
                {
                    Text = nodeChoice.Text
                };

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }
 
        static void UpdateDialogueChoicesConnections()
        {
            foreach (DSNode node in nodes)
            {
                DSDialogueSO dialogue = createdDialogue[node.ID];

                for (int choiceIndex = 0; choiceIndex < node.Choices.Count; choiceIndex++)
                {
                    DSChoiceSaveData nodeChoice = node.Choices[choiceIndex];

                    if (string.IsNullOrEmpty(nodeChoice.NodeID))
                        continue;

                    dialogue.Choices[choiceIndex].NextChoice = createdDialogue[nodeChoice.NodeID];

                    SaveAsset(dialogue);
                }
            }
        }

        static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if (graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach (KeyValuePair<string, List<string>> oldGroupedNodes in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if(currentGroupedNodeNames.ContainsKey(oldGroupedNodes.Key))
                    {
                        nodesToRemove = oldGroupedNodes.Value.Except(currentGroupedNodeNames[oldGroupedNodes.Key]).ToList();
                    }

                    foreach (string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNodes.Key}/Dialogues", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }
        static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, DSGraphSaveDataSO graphData)
        {
            if(graphData.OldUngroupedNodeNames!=null && graphData.OldUngroupedNodeNames.Count!=0)
            {
                List<string> nodesToRemove = graphData.OldGroupNames.Except(currentUngroupedNodeNames).ToList();

                foreach (string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
        }
        #endregion

        #region CreationMethods
        static void CreateStaticFoldes()
        {
            CreateFolder($"{RootDialoguePath}/Editor/DialogueSystem", "Graphs");

            CreateFolder(RootDialoguePath, "DialogueSystem");
            CreateFolder($"{RootDialoguePath}/DialogueSystem", "Dialogues");
            CreateFolder($"{RootDialoguePath}/DialogueSystem/Dialogues", graphFileName);

            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "Dialogues");
        }
        #endregion

        #region Fetch Methods
        private static void GetElementsFromGraphView()
        {
            Type groupType = typeof(DSGroup);

            graphView.graphElements.ForEach(graphElement =>
            {
                if (graphElement is DSNode node)
                {
                    nodes.Add(node);
                    return;
                }

                if (graphElement.GetType() == groupType)
                {
                    groups.Add((DSGroup)graphElement);
                    return;
                }
            });
        }
        #endregion

        #region Utility Methods
        public static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
                return;

            AssetDatabase.CreateFolder(path, folderName);
        }

        public static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();

                AssetDatabase.CreateAsset(asset, $"{path}/{assetName}.asset");
            }

            return asset;
        }

        public static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            return AssetDatabase.LoadAssetAtPath<T>($"{path}/{assetName}.asset");
        }

        public static void RemoveFolder(string fullPath)
        {
            FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullPath}/");
        }

        public static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
        }

        public static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static List<DSChoiceSaveData> CloneNodeChoices(List<DSChoiceSaveData> nodeChoices)
        {
            List<DSChoiceSaveData> choices = new List<DSChoiceSaveData>();
            foreach (DSChoiceSaveData choice in nodeChoices)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID
                };

                choices.Add(choiceData);
            }

            return choices;
        }

        #endregion
    }
}
