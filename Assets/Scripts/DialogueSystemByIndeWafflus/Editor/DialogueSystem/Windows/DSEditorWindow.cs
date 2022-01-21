using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;

namespace DialogueSystem
{
    public class DSEditorWindow : EditorWindow
    {
        readonly string defaultFileName = "DialoguesFileName";

        static TextField fileNameTextField;

        DSGraphView graphView;
        Button saveButton;
        Button miniMapButton;

        [MenuItem("Dialogue/Dialogue Graph")]
        public static void Open()
        {
            DSEditorWindow wnd = GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        void OnEnable()
        {
            AddGraphView();
            AddToolbar();

            AddStyles();
        }

        #region Elements Addition 
        void AddGraphView()
        {
            graphView = new DSGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = DSElementUtility.CreateTextField(defaultFileName, "File Name:", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = DSElementUtility.CreateButton("Save", () => Save());

            Button loadButton = DSElementUtility.CreateButton("Load", () => Load());
            Button clearButton = DSElementUtility.CreateButton("Clear", () => Clear());
            Button resetButton = DSElementUtility.CreateButton("Reset", () => ResetGraph());
            
            miniMapButton = DSElementUtility.CreateButton("MiniMap", () => ToggleMiniMap());

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(miniMapButton);

            toolbar.AddStyleSheets("DSToolbarStyles");

            rootVisualElement.Add(toolbar);
        }

        void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DSVariables");
        }
        #endregion

        #region Toolbar Actions
        void Save()
        {
            if(string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid file name.",
                    "Please ensure the file name you've typed in is valid.",
                    "Roger!"
                    );

                return;
            }

            DSIOUtility.Initialise(graphView, fileNameTextField.value);
            DSIOUtility.Save();
        }
        void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", $"{DSIOUtility.RootDialoguePath}/Editor/DialogueSystem/Graphs", "asset");

            if (string.IsNullOrEmpty(filePath))
                return;

            Clear();

            DSIOUtility.Initialise(graphView, Path.GetFileNameWithoutExtension(filePath));
            DSIOUtility.Load();
        }

        void Clear()
        {
            graphView.ClearGraph();
        }
        void ResetGraph()
        {
            Clear();
            UpdateFileName(defaultFileName);
        }

        void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();

            miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
        }
        #endregion

        #region Utility Methods
        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

        public void EnableSaving() => saveButton.SetEnabled(true);
        public void DisableSaving() => saveButton.SetEnabled(false);
        #endregion
    }
}