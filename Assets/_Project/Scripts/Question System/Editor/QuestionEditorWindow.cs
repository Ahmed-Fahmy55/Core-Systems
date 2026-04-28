using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Zone8.Question.Runtime.Base; // Adjust if your namespace varies

namespace Bltzo.Question.Editor
{
    public class QuestionEditorWindow : EditorWindow
    {
        [SerializeField] private QuestionsListSo questionsList;
        [SerializeField] private QuestionBase questionPreview;

        private ReorderableList reorderableList;
        private Vector2 listScrollPosition;
        private Vector2 editorScrollPosition;

        [MenuItem("Tools/Question Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<QuestionEditorWindow>("Question Editor");
            window.minSize = new Vector2(700, 400);
            window.Show();
        }

        private void OnEnable()
        {
            if (questionsList != null)
            {
                SetupReorderableList();
            }
        }

        private void OnGUI()
        {
            DrawTopToolbar();

            if (questionsList == null)
            {
                EditorGUILayout.HelpBox("Please assign a QuestionsListSo asset to begin.", MessageType.Info);
                return;
            }

            // Ensure the internal list is never null
            if (questionsList.Questions == null)
            {
                questionsList.Questions = new List<QuestionBase>();
            }

            if (reorderableList == null) SetupReorderableList();

            EditorGUILayout.BeginHorizontal();

            // --- LEFT COLUMN: The Reorderable List ---
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition, "box");
            reorderableList.DoLayoutList();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // --- RIGHT COLUMN: The Question Editor ---
            EditorGUILayout.BeginVertical("box");
            editorScrollPosition = EditorGUILayout.BeginScrollView(editorScrollPosition);
            DrawQuestionEditor();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTopToolbar()
        {
            GUILayout.Space(8);
            GUILayout.Label("Question Database Editor", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            questionsList = (QuestionsListSo)EditorGUILayout.ObjectField("Target Questions List", questionsList, typeof(QuestionsListSo), false);
            if (EditorGUI.EndChangeCheck())
            {
                questionPreview = null;
                reorderableList = null;
                if (questionsList != null) SetupReorderableList();
            }
            GUILayout.Space(10);
        }

        private void DrawQuestionEditor()
        {
            if (questionPreview == null)
            {
                EditorGUILayout.HelpBox("Select a question from the list to modify its properties.", MessageType.Info);
                return;
            }

            SerializedObject so = new SerializedObject(questionPreview);
            so.Update();

            EditorGUILayout.LabelField($"Question Type: {questionPreview.GetType().Name}", EditorStyles.miniBoldLabel);
            GUILayout.Space(5);

            // 1. Question Text Area
            EditorGUILayout.LabelField("Question Text", EditorStyles.boldLabel);
            SerializedProperty textProp = so.FindProperty(nameof(QuestionBase.QuestionText));
            textProp.stringValue = EditorGUILayout.TextArea(textProp.stringValue, GUILayout.MinHeight(50));

            GUILayout.Space(10);

            // 2. Feedback Boxes (Big & Side-by-Side)
            EditorGUILayout.BeginHorizontal();

            // Correct Feedback
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Correct Feedback", EditorStyles.boldLabel);
            SerializedProperty correctProp = so.FindProperty(nameof(QuestionBase.CorrectFeedback));
            correctProp.stringValue = EditorGUILayout.TextArea(correctProp.stringValue, GUILayout.MinHeight(100));
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            // Wrong Feedback
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Wrong Feedback", EditorStyles.boldLabel);
            SerializedProperty wrongProp = so.FindProperty(nameof(QuestionBase.WrongFeedback));
            wrongProp.stringValue = EditorGUILayout.TextArea(wrongProp.stringValue, GUILayout.MinHeight(100));
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            // 3. Category Field with "New" Button
            EditorGUILayout.BeginHorizontal();
            SerializedProperty categoryProp = so.FindProperty(nameof(QuestionBase.Category));
            EditorGUILayout.PropertyField(categoryProp);
            if (GUILayout.Button("New", GUILayout.Width(50)))
            {
                CreateNewCategory(categoryProp);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Specific Parameters", EditorStyles.boldLabel);

            // 4. Iterate other properties (Skip system fields and handled ones)
            SerializedProperty prop = so.GetIterator();
            prop.NextVisible(true); // Skip Script
            while (prop.NextVisible(false))
            {
                if (prop.name == "m_Script" ||
                    prop.name == nameof(QuestionBase.QuestionText) ||
                    prop.name == nameof(QuestionBase.CorrectFeedback) ||
                    prop.name == nameof(QuestionBase.WrongFeedback) ||
                    prop.name == nameof(QuestionBase.Category)) continue;

                EditorGUILayout.PropertyField(prop, true);
            }

            if (so.hasModifiedProperties)
            {
                so.ApplyModifiedProperties();
                SyncAssetName();
            }
        }

        private void SetupReorderableList()
        {
            reorderableList = new ReorderableList(questionsList.Questions, typeof(QuestionBase), true, true, true, true);

            reorderableList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Question Inventory");

            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = questionsList.Questions[index];
                string name = element == null ? "Null" : (string.IsNullOrEmpty(element.QuestionText) ? element.name : element.QuestionText);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), name);
            };

            reorderableList.onSelectCallback = l =>
            {
                questionPreview = questionsList.Questions[l.index];
            };

            reorderableList.onAddDropdownCallback = (rect, l) => ShowAddQuestionMenu();

            reorderableList.onRemoveCallback = l =>
            {
                var element = questionsList.Questions[l.index];
                if (EditorUtility.DisplayDialog("Delete Question", $"Are you sure you want to permanently delete '{element.name}'?", "Delete", "Cancel"))
                {
                    Undo.RecordObject(questionsList, "Remove Question");
                    questionsList.Questions.RemoveAt(l.index);

                    if (questionPreview == element) questionPreview = null;

                    Undo.DestroyObjectImmediate(element);

                    EditorUtility.SetDirty(questionsList);
                    AssetDatabase.SaveAssets();
                }
            };
        }

        private void ShowAddQuestionMenu()
        {
            GenericMenu menu = new GenericMenu();
            foreach (var type in GetQuestionTypes())
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    QuestionBase newQuestion = (QuestionBase)ScriptableObject.CreateInstance(type);
                    newQuestion.name = type.Name;

                    Undo.RegisterCreatedObjectUndo(newQuestion, "Create Question Instance");
                    Undo.RecordObject(questionsList, "Add Question to List");

                    AssetDatabase.AddObjectToAsset(newQuestion, questionsList);
                    questionsList.Questions.Add(newQuestion);

                    EditorUtility.SetDirty(questionsList);
                    AssetDatabase.SaveAssets();

                    questionPreview = newQuestion;
                });
            }
            menu.ShowAsContext();
        }

        private void CreateNewCategory(SerializedProperty categoryProp)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save New Category", "NewCategory", "asset", "Select Category Destination");

            if (!string.IsNullOrEmpty(path))
            {
                CategorySo newCategory = ScriptableObject.CreateInstance<CategorySo>();
                newCategory.Name = Path.GetFileNameWithoutExtension(path);

                AssetDatabase.CreateAsset(newCategory, path);
                AssetDatabase.SaveAssets();

                categoryProp.objectReferenceValue = newCategory;
                categoryProp.serializedObject.ApplyModifiedProperties();
            }
        }

        private void SyncAssetName()
        {
            if (questionPreview == null) return;

            string targetName = string.IsNullOrWhiteSpace(questionPreview.QuestionText)
                ? questionPreview.GetType().Name
                : questionPreview.QuestionText;

            // Optional: Limit length for asset names
            if (targetName.Length > 40) targetName = targetName.Substring(0, 37) + "...";

            if (questionPreview.name != targetName)
            {
                questionPreview.name = targetName;
                EditorUtility.SetDirty(questionPreview);
            }
        }

        private static Type[] GetQuestionTypes()
        {
            return Assembly.GetAssembly(typeof(QuestionBase))
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(QuestionBase)))
                .ToArray();
        }
    }
}