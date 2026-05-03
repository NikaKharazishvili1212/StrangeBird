using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nikspector
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string Name;
        public ButtonAttribute() => Name = "";
        public ButtonAttribute(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.All)]
    public class TabAttribute : Attribute
    {
        public string Name;
        public TabAttribute(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FoldoutAttribute : Attribute
    {
        public string Name;
        public FoldoutAttribute(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class FoldoutEndAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class HelpBoxAttribute : Attribute
    {
        public string Text;
        public int FontSize;
        public MessageType MessageType;

        public HelpBoxAttribute(string text, int fontSize = 15, MessageType messageType = MessageType.Info)
        {
            Text = text;
            FontSize = fontSize;
            MessageType = messageType;
        }
    }

#if UNITY_EDITOR
    public class NikspectorData : ScriptableObject
    {
        public List<ButtonInfo> Buttons = new();
        public Foldout RootFoldout = new Foldout(true);

        [Serializable]
        public class ButtonInfo
        {
            public string Name;
            public string Tab;
            public MemberInfo Member;
        }

        [Serializable]
        public class FieldDisplayInfo
        {
            public string Name;
            public FieldInfo Field;
            public SerializedProperty Property;
            public FieldType Type;
            public HelpBoxInfo HelpBox;
        }

        [Serializable]
        public class HelpBoxInfo
        {
            public string Text;
            public int FontSize;
            public MessageType MessageType;
            public bool AlreadyDrawn;

            public HelpBoxInfo(string text, int fontSize, MessageType messageType)
            {
                Text = text;
                FontSize = fontSize;
                MessageType = messageType;
                AlreadyDrawn = false;
            }
        }

        public enum FieldType
        {
            Normal,
            Const,
            Readonly,
            Static
        }

        [Serializable]
        public class Foldout
        {
            public string Name;
            public bool Expanded;

            [SerializeReference]
            public List<Foldout> Subfoldouts = new List<Foldout>();

            public Foldout GetSubfoldout(string path)
            {
                if (path == "") return this;
                else if (!path.Contains('/')) return Subfoldouts.Find(r => r.Name == path);
                else return Subfoldouts.Find(r => r.Name == path.Split('/').First())?.GetSubfoldout(path.Substring(path.IndexOf('/') + 1));
            }

            public bool IsSubfoldoutContentVisible(string path)
            {
                if (string.IsNullOrEmpty(path)) return true;
                else if (!path.Contains('/')) return Expanded && Subfoldouts.Find(r => r.Name == path)?.Expanded == true;
                else return Expanded && Subfoldouts.Find(r => r.Name == path.Split('/').First())?.IsSubfoldoutContentVisible(path.Substring(path.IndexOf('/') + 1)) == true;
            }

            public Foldout(string name) => Name = name;
            public Foldout(bool expanded) => Expanded = expanded;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class NikspectorMonoBehaviourEditor : Editor
    {
        private NikspectorData _data;
        private Dictionary<string, List<NikspectorData.FieldDisplayInfo>> _fieldsByTab = new();
        private List<NikspectorData.FieldDisplayInfo> _rootFields = new();
        private string _selectedTab = "";
        private string[] _tabKeys;

        private void OnEnable()
        {
            SetupData();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the script field at the top
            GUI.enabled = false;
            var scriptProp = serializedObject.FindProperty("m_Script");
            if (scriptProp != null)
                EditorGUILayout.PropertyField(scriptProp);
            GUI.enabled = true;

            GUILayout.Space(3);
            DrawTabs();
            DrawFields();
            DrawButtons();

            serializedObject.ApplyModifiedProperties();
        }

        private void SetupData()
        {
            _data = ScriptableObject.CreateInstance<NikspectorData>();
            _fieldsByTab.Clear();
            _rootFields.Clear();

            SetupButtons();
            SetupFields();
            SetupFoldouts();

            if (_fieldsByTab.Any())
            {
                _selectedTab = _fieldsByTab.Keys.First();
                _tabKeys = _fieldsByTab.Keys.ToArray();
            }
        }

        private void SetupButtons()
        {
            var members = target.GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute<ButtonAttribute>() != null);

            foreach (var member in members)
            {
                var buttonAttr = member.GetCustomAttribute<ButtonAttribute>();
                var tabAttr = member.GetCustomAttribute<TabAttribute>();

                _data.Buttons.Add(new NikspectorData.ButtonInfo
                {
                    Name = string.IsNullOrEmpty(buttonAttr.Name) ? FormatName(member.Name) : buttonAttr.Name,
                    Tab = tabAttr?.Name ?? "",
                    Member = member
                });
            }
        }

        private void SetupFields()
        {
            var allFields = target.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .OrderBy(f => f.MetadataToken)
                .ToList();

            var propertyMap = new Dictionary<string, SerializedProperty>();
            var prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do propertyMap[prop.name] = prop.Copy();
                while (prop.NextVisible(false));
            }

            string currentTab = "";

            foreach (var field in allFields)
            {
                var tabAttr = field.GetCustomAttribute<TabAttribute>();

                if (tabAttr != null) currentTab = tabAttr.Name;

                if (field.GetCustomAttribute<ButtonAttribute>() != null) continue;

                NikspectorData.FieldType fieldType;
                if (field.IsLiteral) fieldType = NikspectorData.FieldType.Const;
                else if (field.IsInitOnly && !field.IsStatic) fieldType = NikspectorData.FieldType.Readonly;
                else if (propertyMap.ContainsKey(field.Name)) fieldType = NikspectorData.FieldType.Normal;
                else if (field.IsStatic) fieldType = NikspectorData.FieldType.Static;
                else continue;

                var helpBoxAttr = field.GetCustomAttribute<HelpBoxAttribute>();
                NikspectorData.HelpBoxInfo helpBoxInfo = null;
                if (helpBoxAttr != null)
                {
                    helpBoxInfo = new NikspectorData.HelpBoxInfo(
                        helpBoxAttr.Text,
                        helpBoxAttr.FontSize,
                        helpBoxAttr.MessageType
                    );
                }

                var fieldInfo = new NikspectorData.FieldDisplayInfo
                {
                    Name = FormatName(field.Name),
                    Field = field,
                    Property = fieldType == NikspectorData.FieldType.Normal ? propertyMap[field.Name] : null,
                    Type = fieldType,
                    HelpBox = helpBoxInfo
                };

                if (!string.IsNullOrEmpty(currentTab))
                {
                    if (!_fieldsByTab.ContainsKey(currentTab)) _fieldsByTab[currentTab] = new();
                    _fieldsByTab[currentTab].Add(fieldInfo);
                }
                else _rootFields.Add(fieldInfo);
            }
        }

        private void SetupFoldouts()
        {
            var foldoutAttributes = new List<FoldoutAttribute>();

            void FindFoldoutAttributes(Type type)
            {
                foldoutAttributes.AddRange(type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Select(r => r.GetCustomAttribute<FoldoutAttribute>())
                    .OfType<FoldoutAttribute>());

                if (type == typeof(MonoBehaviour)) return;
                if (type == typeof(ScriptableObject)) return;
                if (type == null) return;
                if (type.BaseType == null) return;

                FindFoldoutAttributes(type.BaseType);
            }

            void SetupFoldout(NikspectorData.Foldout foldout, IEnumerable<string> allSubfoldoutPaths)
            {
                if (foldout.Subfoldouts == null) foldout.Subfoldouts = new List<NikspectorData.Foldout>();

                foldout.Subfoldouts.RemoveAll(r => r == null);

                var names = allSubfoldoutPaths.Select(r => r.Split('/').First()).Distinct().ToList();

                foreach (var name in names)
                    if (foldout.Subfoldouts.Find(r => r.Name == name) == null)
                        foldout.Subfoldouts.Add(new NikspectorData.Foldout(name));

                foreach (var subfoldout in foldout.Subfoldouts.ToList())
                    if (!names.Contains(subfoldout.Name))
                        foldout.Subfoldouts.Remove(subfoldout);

                foldout.Subfoldouts = foldout.Subfoldouts.OrderBy(r => names.IndexOf(r.Name)).ToList();

                foreach (var subfoldout in foldout.Subfoldouts)
                    SetupFoldout(subfoldout, allSubfoldoutPaths
                        .Where(r => r.StartsWith(subfoldout.Name + "/"))
                        .Select(r => r.Substring(subfoldout.Name.Length + 1))
                        .ToList());
            }

            FindFoldoutAttributes(target.GetType());
            SetupFoldout(_data.RootFoldout, foldoutAttributes.Select(r => r.Name));
        }

        private void DrawTabs()
        {
            if (_tabKeys == null || _tabKeys.Length == 0) return;

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            foreach (var tabName in _tabKeys)
            {
                var prevColor = GUI.backgroundColor;
                if (_selectedTab == tabName) GUI.backgroundColor = Color.white * 1.5f;
                if (GUILayout.Button(tabName, GUILayout.Height(25))) _selectedTab = tabName;
                GUI.backgroundColor = prevColor;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        private void DrawFields()
        {
            // Reset all HelpBox AlreadyDrawn flags before drawing
            if (!string.IsNullOrEmpty(_selectedTab) && _fieldsByTab.ContainsKey(_selectedTab))
                ResetHelpBoxFlags(_fieldsByTab[_selectedTab]);
            else
                ResetHelpBoxFlags(_rootFields);

            if (!string.IsNullOrEmpty(_selectedTab) && _fieldsByTab.ContainsKey(_selectedTab))
                DrawFieldList(_fieldsByTab[_selectedTab]);
            else
                DrawFieldList(_rootFields);
        }

        private void ResetHelpBoxFlags(List<NikspectorData.FieldDisplayInfo> fields)
        {
            foreach (var field in fields)
            {
                if (field.HelpBox != null)
                    field.HelpBox.AlreadyDrawn = false;
            }
        }

        private void DrawFieldList(List<NikspectorData.FieldDisplayInfo> fields)
        {
            string drawingFoldoutPath = "";
            string lastHelpBoxText = null;

            void UpdateIndentLevel(string path)
            {
                var prev = EditorGUI.indentLevel;
                EditorGUI.indentLevel = path.Split('/').Where(r => r != "").Count();
                if (prev > EditorGUI.indentLevel) GUILayout.Space(6);
            }

            foreach (var field in fields)
            {
                var foldoutAttribute = field.Field.GetCustomAttribute<FoldoutAttribute>();
                var endFoldoutAttribute = field.Field.GetCustomAttribute<FoldoutEndAttribute>();

                var newFoldoutPath = drawingFoldoutPath;
                if (endFoldoutAttribute != null) newFoldoutPath = "";
                if (foldoutAttribute != null) newFoldoutPath = foldoutAttribute.Name;

                var drawingPathSplit = drawingFoldoutPath.Split('/').Where(r => r != "").ToArray();
                var newPathSplit = newFoldoutPath.Split('/').Where(r => r != "").ToArray();
                var sharedLength = 0;

                for (; sharedLength < newPathSplit.Length && sharedLength < drawingPathSplit.Length; sharedLength++)
                    if (drawingPathSplit[sharedLength] != newPathSplit[sharedLength]) break;

                drawingFoldoutPath = string.Join("/", drawingPathSplit.Take(sharedLength));

                for (int i = sharedLength; i < newPathSplit.Length; i++)
                {
                    if (!_data.RootFoldout.IsSubfoldoutContentVisible(drawingFoldoutPath)) break;

                    var prevPath = drawingFoldoutPath;
                    drawingFoldoutPath += (drawingFoldoutPath == "" ? "" : "/") + newPathSplit[i];
                    drawingFoldoutPath = drawingFoldoutPath.Trim('/');

                    UpdateIndentLevel(prevPath);

                    var foldout = _data.RootFoldout.GetSubfoldout(drawingFoldoutPath);
                    if (foldout != null)
                    {
                        var prevColor = GUI.color;
                        GUI.color = Color.white * 1.5f;

                        var newExpanded = EditorGUILayout.Foldout(foldout.Expanded, foldout.Name, true);

                        GUI.color = prevColor;

                        if (newExpanded != foldout.Expanded)
                        {
                            Undo.RecordObject(_data, "Toggle Foldout");
                            foldout.Expanded = newExpanded;
                        }
                    }
                }

                if (!_data.RootFoldout.IsSubfoldoutContentVisible(drawingFoldoutPath)) continue;

                UpdateIndentLevel(drawingFoldoutPath);

                // Draw HelpBox if it hasn't been drawn yet for this field group
                if (field.HelpBox != null && !field.HelpBox.AlreadyDrawn)
                {
                    // Check if we need to draw this help box (it should only be drawn once per unique text)
                    if (field.HelpBox.Text != lastHelpBoxText)
                    {
                        DrawHelpBox(field.HelpBox);
                        lastHelpBoxText = field.HelpBox.Text;

                        // Mark all fields with the same help box text as already drawn
                        foreach (var f in fields)
                        {
                            if (f.HelpBox != null && f.HelpBox.Text == field.HelpBox.Text)
                                f.HelpBox.AlreadyDrawn = true;
                        }
                    }
                }
                else if (field.HelpBox == null)
                {
                    // Reset lastHelpBoxText when we encounter a field without a help box
                    lastHelpBoxText = null;
                }

                DrawField(field);
            }

            EditorGUI.indentLevel = 0;
        }

        private void DrawHelpBox(NikspectorData.HelpBoxInfo helpBox)
        {
            // Create frame style for yellow border
            var frameStyle = new GUIStyle();
            frameStyle.normal.background = MakeTex(2, 2, Color.yellow);
            frameStyle.padding = new RectOffset(1, 1, 1, 1); // 1px border thickness
            frameStyle.margin = new RectOffset(3, 3, 3, 3);

            // Create help box style with dark background
            var helpBoxStyle = new GUIStyle();
            helpBoxStyle.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.15f, 1f));
            helpBoxStyle.padding = new RectOffset(5, 5, 2, 2);
            helpBoxStyle.wordWrap = true;
            helpBoxStyle.alignment = TextAnchor.MiddleLeft;
            helpBoxStyle.fontSize = helpBox.FontSize;
            helpBoxStyle.normal.textColor = GetTextColor(helpBox.MessageType);

            // Draw the frame (yellow border)
            EditorGUILayout.BeginVertical(frameStyle);

            // Draw the help box with text inside
            GUILayout.Label(helpBox.Text, helpBoxStyle);

            EditorGUILayout.EndVertical();

            // Add spacing after the help box
            GUILayout.Space(5);
        }

        private Color GetTextColor(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Warning => new Color(1f, 0.9f, 0.4f), // Yellow for warnings
                MessageType.Error => new Color(1f, 0.4f, 0.4f),   // Red for errors
                _ => new Color(0.9f, 0.9f, 0.9f)                  // Light gray for info
            };
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void DrawField(NikspectorData.FieldDisplayInfo fieldInfo)
        {
            switch (fieldInfo.Type)
            {
                case NikspectorData.FieldType.Normal:
                    var labell = new GUIContent(fieldInfo.Property.displayName);
                    EditorGUILayout.PropertyField(fieldInfo.Property, labell, true);
                    break;
                case NikspectorData.FieldType.Const:
                    EditorGUI.BeginDisabledGroup(true);
                    DrawFieldValue($"(const) {fieldInfo.Name}", fieldInfo.Field.GetValue(null));
                    EditorGUI.EndDisabledGroup();
                    break;
                case NikspectorData.FieldType.Readonly:
                    EditorGUI.BeginDisabledGroup(true);
                    DrawFieldValue($"(readonly) {fieldInfo.Name}", fieldInfo.Field.GetValue(target));
                    EditorGUI.EndDisabledGroup();
                    break;
                case NikspectorData.FieldType.Static:
                    object currentValue = fieldInfo.Field.GetValue(null);
                    Type fieldType = fieldInfo.Field.FieldType;
                    string label = $"(static) {fieldInfo.Name}";

                    if (fieldType == typeof(int)) fieldInfo.Field.SetValue(null, EditorGUILayout.IntField(label, (int)currentValue));
                    else if (fieldType == typeof(float)) fieldInfo.Field.SetValue(null, EditorGUILayout.FloatField(label, (float)currentValue));
                    else if (fieldType == typeof(string)) fieldInfo.Field.SetValue(null, EditorGUILayout.TextField(label, (string)currentValue));
                    else if (fieldType == typeof(bool)) fieldInfo.Field.SetValue(null, EditorGUILayout.Toggle(label, (bool)currentValue));
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType)) fieldInfo.Field.SetValue(null, EditorGUILayout.ObjectField(label, (UnityEngine.Object)currentValue, fieldType, true));
                    else DrawFieldValue(label, currentValue);
                    break;
            }
        }

        private void DrawFieldValue(string label, object value)
        {
            EditorGUILayout.LabelField(label, value?.ToString() ?? "null");
        }

        private void DrawButtons()
        {
            if (!_data.Buttons.Any()) return;

            GUILayout.Space(10);

            foreach (var button in _data.Buttons)
            {
                if (!string.IsNullOrEmpty(button.Tab) && button.Tab != _selectedTab) continue;

                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.white * 1f;

                if (GUILayout.Button(button.Name, GUILayout.Height(30)))
                {
                    foreach (var t in targets)
                    {
                        Undo.RecordObject(t, button.Name);
                        var method = button.Member as MethodInfo;
                        method.Invoke(method.IsStatic ? null : t, null);
                    }
                }

                GUI.backgroundColor = prevColor;
            }
        }

        private string FormatName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            var result = System.Text.RegularExpressions.Regex.Replace(name, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"(\p{Ll})(\P{Ll})", "$1 $2");

            return char.ToUpper(result[0]) + result.Substring(1);
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true)]
    public class NikspectorScriptableObjectEditor : NikspectorMonoBehaviourEditor { }
#endif
}