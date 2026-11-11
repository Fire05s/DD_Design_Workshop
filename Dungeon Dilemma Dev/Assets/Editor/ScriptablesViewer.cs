using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ScriptablesViewer : EditorWindow
{
    [SerializeField] private int m_SelectedIndex = -1;
    private VisualElement m_RightPane;

    private Editor m_newEditor;

    [MenuItem("Window/Custom Tools/Scriptables Viewer")]
    public static void ShowWindow()
    {
        // This method is called when the user selects the menu item in the Editor.
        EditorWindow window = GetWindow<ScriptablesViewer>();
        window.titleContent = new GUIContent("Scriptable Object Viewer");

        // Limit size of the window.
        window.minSize = new Vector2(450, 200);
        window.maxSize = new Vector2(1920, 720);
    }

    public void CreateGUI()
    {
        // Get a list of all sprites in the project.
        var allObjectGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Scriptables" });
        var allObjects = new List<ScriptableObject>();
        foreach (var guid in allObjectGuids)
        {
            allObjects.Add(AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        // Create a two-pane view with the left pane being fixed.
        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        // Add the panel to the visual tree by adding it as a child to the root element.
        rootVisualElement.Add(splitView);

        // A TwoPaneSplitView always needs two child elements.
        var leftPane = new ListView();
        splitView.Add(leftPane);
        m_RightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        splitView.Add(m_RightPane);

        // Initialize the list view with all sprites' names.
        leftPane.makeItem = () => new Label();
        leftPane.bindItem = (item, index) => { (item as Label).text = allObjects[index].name; };
        leftPane.itemsSource = allObjects;

        // React to the user's selection.
        leftPane.selectionChanged += OnObjectSelectionChange;

        // Restore the selection index from before the hot reload.
        leftPane.selectedIndex = m_SelectedIndex;

        // Store the selection index when the selection changes.
        leftPane.selectionChanged += (items) => { m_SelectedIndex = leftPane.selectedIndex; };
    }

    private void OnObjectSelectionChange(IEnumerable<object> selectedItems)
    {
        // Clear all previous content from the pane.
        m_RightPane.Clear();

        var enumerator = selectedItems.GetEnumerator();
        if (enumerator.MoveNext()) {
            var selectedObject = enumerator.Current as ScriptableObject;
            if (selectedObject != null)
            {
                // m_newEditor = Editor.CreateEditor(selectedObject);
                // m_newEditor.OnInspectorGUI();
                SerializedObject soEditor = new UnityEditor.SerializedObject(selectedObject);
                var container = new VisualElement();

                BindingExtensions.Bind(container, soEditor);
                // var it = soEditor.GetIterator();
                // while (it.NextVisible(false))
                // {
                //     SerializedProperty serializedProperty = it.Copy();
                //     container.Bind(serializedProperty);

                //     container.Add(serializedProperty);
                // }

                m_RightPane.Add(container);
            }
        }
    }
//   private VisualElement CreateEditorFromNodeData(ScriptableObject new_object)
//         {
//             SerializedObject soEditor = new UnityEditor.SerializedObject(new_object);
//             var container = new VisualElement();
          
//             var it = soEditor.GetIterator();
//             if (!it.NextVisible(true))
//                 return container;
          
//             //Descends through serialized property children & allows us to edit them.
//             do
//             {
//                 var propertyField = new EditorGUILayout.PropertyField(it.Copy())
//                     { name = "PropertyField:" + it.propertyPath };

//                 //Bind the property so we can edit the values.
//                 propertyField.Bind(soEditor);
                  
//                 //This ignores the label name field, it's ugly.
//                 if (it.propertyPath == "m_Script" && soEditor.targetObject != null)
//                 {
//                     propertyField.SetEnabled(false);
//                     propertyField.visible = false;
//                 }
                  
//                 container.Add(propertyField);
//             }
//             while (it.NextVisible(false));

//             return container;
//         }
}