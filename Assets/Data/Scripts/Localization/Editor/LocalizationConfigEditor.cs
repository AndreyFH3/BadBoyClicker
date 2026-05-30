using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameLocalization;
using UnityEditor;
using UnityEngine;

namespace GameLocalizationEditor
{
    [CustomEditor(typeof(LocalizationConfig))]
    public class LocalizationConfigEditor : Editor
    {
        private const float KeyColumnWidth = 240f;
        private const float LanguageColumnWidth = 280f;
        private const float ButtonColumnWidth = 58f;

        private SerializedProperty _defaultLanguage;
        private SerializedProperty _currentLanguage;
        private SerializedProperty _entries;

        private Vector2 _scroll;
        private string _newKey = "";
        private string _newLanguage = "en";
        private string _search = "";
        private bool _showTools = true;

        private void OnEnable()
        {
            _defaultLanguage = serializedObject.FindProperty("_defaultLanguage");
            _currentLanguage = serializedObject.FindProperty("_currentLanguage");
            _entries = serializedObject.FindProperty("_entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Localization Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Edit localization as a table: each row is a key, each column is a language. Runtime data format stays unchanged.",
                MessageType.Info);

            DrawLanguageSettings();
            DrawTools();

            List<string> languages = CollectLanguages();
            EnsureAllEntriesHaveLanguages(languages);
            DrawIssues(languages);
            DrawTable(languages);

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Localization/Open Config")]
        private static void OpenLocalizationConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:LocalizationConfig");
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Localization", "No LocalizationConfig asset found.", "OK");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(path);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private void DrawLanguageSettings()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_defaultLanguage, new GUIContent("Default Language"));
                EditorGUILayout.PropertyField(_currentLanguage, new GUIContent("Current Language"));
            }
        }

        private void DrawTools()
        {
            _showTools = EditorGUILayout.Foldout(_showTools, "Tools", true);
            if (!_showTools)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _newLanguage = EditorGUILayout.TextField("New Language", _newLanguage);
                    if (GUILayout.Button("Add Language", GUILayout.Width(120f)))
                    {
                        AddLanguage(_newLanguage);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _newKey = EditorGUILayout.TextField("New Key", _newKey);
                    if (GUILayout.Button("Add Key", GUILayout.Width(120f)))
                    {
                        AddKey(_newKey, CollectLanguages());
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _search = EditorGUILayout.TextField("Search", _search);
                    if (GUILayout.Button("Clear", GUILayout.Width(70f)))
                    {
                        _search = "";
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Sort Keys A-Z"))
                    {
                        SortEntries();
                    }

                    if (GUILayout.Button("Remove Empty Rows"))
                    {
                        RemoveEmptyRows();
                    }

                    if (GUILayout.Button("Export TSV"))
                    {
                        ExportTsv();
                    }

                    if (GUILayout.Button("Import TSV"))
                    {
                        ImportTsv();
                    }
                }
            }
        }

        private void DrawIssues(IReadOnlyList<string> languages)
        {
            List<string> duplicateKeys = FindDuplicateKeys();
            int emptyKeys = CountEmptyKeys();
            int missingValues = CountMissingValues(languages);

            if (duplicateKeys.Count == 0 && emptyKeys == 0 && missingValues == 0)
            {
                EditorGUILayout.HelpBox("Validation passed: keys are unique and all language cells exist.", MessageType.None);
                return;
            }

            string message = "";
            if (emptyKeys > 0)
            {
                message += $"Empty keys: {emptyKeys}\n";
            }

            if (duplicateKeys.Count > 0)
            {
                message += $"Duplicate keys: {string.Join(", ", duplicateKeys)}\n";
            }

            if (missingValues > 0)
            {
                message += $"Empty translations: {missingValues}";
            }

            EditorGUILayout.HelpBox(message.Trim(), MessageType.Warning);
        }

        private void DrawTable(IReadOnlyList<string> languages)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(KeyColumnWidth));
                foreach (string language in languages)
                {
                    GUILayout.Label(language, EditorStyles.boldLabel, GUILayout.Width(LanguageColumnWidth));
                }

                GUILayout.Space(ButtonColumnWidth);
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(260f));
            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                SerializedProperty key = entry.FindPropertyRelative("_key");

                if (!MatchesSearch(entry, languages))
                {
                    continue;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    key.stringValue = EditorGUILayout.TextField(key.stringValue, GUILayout.Width(KeyColumnWidth));

                    foreach (string language in languages)
                    {
                        SerializedProperty value = GetOrCreateValue(entry, language);
                        SerializedProperty text = value.FindPropertyRelative("_text");
                        text.stringValue = EditorGUILayout.TextArea(text.stringValue, GUILayout.Width(LanguageColumnWidth), GUILayout.MinHeight(38f));
                    }

                    if (GUILayout.Button("X", GUILayout.Width(ButtonColumnWidth)))
                    {
                        _entries.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private List<string> CollectLanguages()
        {
            HashSet<string> languages = new(StringComparer.OrdinalIgnoreCase);
            AddLanguageIfValid(languages, _defaultLanguage.stringValue);
            AddLanguageIfValid(languages, _currentLanguage.stringValue);

            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty values = _entries.GetArrayElementAtIndex(i).FindPropertyRelative("_values");
                for (int j = 0; j < values.arraySize; j++)
                {
                    AddLanguageIfValid(languages, values.GetArrayElementAtIndex(j).FindPropertyRelative("_language").stringValue);
                }
            }

            return languages.OrderBy(language => language, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void AddLanguageIfValid(ISet<string> languages, string language)
        {
            if (!string.IsNullOrWhiteSpace(language))
            {
                languages.Add(language.Trim());
            }
        }

        private void AddLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return;
            }

            language = language.Trim();
            for (int i = 0; i < _entries.arraySize; i++)
            {
                GetOrCreateValue(_entries.GetArrayElementAtIndex(i), language);
            }

            if (string.IsNullOrWhiteSpace(_defaultLanguage.stringValue))
            {
                _defaultLanguage.stringValue = language;
            }

            if (string.IsNullOrWhiteSpace(_currentLanguage.stringValue))
            {
                _currentLanguage.stringValue = language;
            }
        }

        private void AddKey(string key, IReadOnlyList<string> languages)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            int index = _entries.arraySize;
            _entries.InsertArrayElementAtIndex(index);

            SerializedProperty entry = _entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("_key").stringValue = key.Trim();

            SerializedProperty values = entry.FindPropertyRelative("_values");
            values.ClearArray();

            foreach (string language in languages)
            {
                AddValue(values, language, "");
            }

            if (languages.Count == 0)
            {
                AddValue(values, "ru", "");
            }

            _newKey = "";
        }

        private void EnsureAllEntriesHaveLanguages(IReadOnlyList<string> languages)
        {
            foreach (string language in languages)
            {
                for (int i = 0; i < _entries.arraySize; i++)
                {
                    GetOrCreateValue(_entries.GetArrayElementAtIndex(i), language);
                }
            }
        }

        private SerializedProperty GetOrCreateValue(SerializedProperty entry, string language)
        {
            SerializedProperty values = entry.FindPropertyRelative("_values");
            for (int i = 0; i < values.arraySize; i++)
            {
                SerializedProperty value = values.GetArrayElementAtIndex(i);
                string currentLanguage = value.FindPropertyRelative("_language").stringValue;
                if (string.Equals(currentLanguage, language, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return AddValue(values, language, "");
        }

        private static SerializedProperty AddValue(SerializedProperty values, string language, string text)
        {
            int index = values.arraySize;
            values.InsertArrayElementAtIndex(index);

            SerializedProperty value = values.GetArrayElementAtIndex(index);
            value.FindPropertyRelative("_language").stringValue = language;
            value.FindPropertyRelative("_text").stringValue = text;
            return value;
        }

        private bool MatchesSearch(SerializedProperty entry, IReadOnlyList<string> languages)
        {
            if (string.IsNullOrWhiteSpace(_search))
            {
                return true;
            }

            string search = _search.Trim();
            if (entry.FindPropertyRelative("_key").stringValue.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return languages.Any(language =>
            {
                SerializedProperty value = GetOrCreateValue(entry, language);
                string text = value.FindPropertyRelative("_text").stringValue;
                return text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
            });
        }

        private List<string> FindDuplicateKeys()
        {
            Dictionary<string, int> counts = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _entries.arraySize; i++)
            {
                string key = _entries.GetArrayElementAtIndex(i).FindPropertyRelative("_key").stringValue;
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                counts.TryGetValue(key, out int count);
                counts[key] = count + 1;
            }

            return counts.Where(pair => pair.Value > 1).Select(pair => pair.Key).OrderBy(key => key).ToList();
        }

        private int CountEmptyKeys()
        {
            int count = 0;
            for (int i = 0; i < _entries.arraySize; i++)
            {
                if (string.IsNullOrWhiteSpace(_entries.GetArrayElementAtIndex(i).FindPropertyRelative("_key").stringValue))
                {
                    count++;
                }
            }

            return count;
        }

        private int CountMissingValues(IReadOnlyList<string> languages)
        {
            int count = 0;
            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                foreach (string language in languages)
                {
                    string text = GetOrCreateValue(entry, language).FindPropertyRelative("_text").stringValue;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void RemoveEmptyRows()
        {
            for (int i = _entries.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                string key = entry.FindPropertyRelative("_key").stringValue;
                SerializedProperty values = entry.FindPropertyRelative("_values");

                bool hasText = false;
                for (int j = 0; j < values.arraySize; j++)
                {
                    if (!string.IsNullOrWhiteSpace(values.GetArrayElementAtIndex(j).FindPropertyRelative("_text").stringValue))
                    {
                        hasText = true;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(key) && !hasText)
                {
                    _entries.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private void SortEntries()
        {
            List<EntrySnapshot> snapshots = ReadSnapshots();
            snapshots.Sort((left, right) => string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase));
            WriteSnapshots(snapshots);
        }

        private void ExportTsv()
        {
            string path = EditorUtility.SaveFilePanel("Export Localization TSV", Application.dataPath, "LocalizationConfig.tsv", "tsv");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            List<string> languages = CollectLanguages();
            List<string> lines = new() { "key\t" + string.Join("\t", languages.Select(EscapeTsv)) };

            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                List<string> cells = new() { EscapeTsv(entry.FindPropertyRelative("_key").stringValue) };

                foreach (string language in languages)
                {
                    cells.Add(EscapeTsv(GetOrCreateValue(entry, language).FindPropertyRelative("_text").stringValue));
                }

                lines.Add(string.Join("\t", cells));
            }

            File.WriteAllLines(path, lines);
            AssetDatabase.Refresh();
        }

        private void ImportTsv()
        {
            string path = EditorUtility.OpenFilePanel("Import Localization TSV", Application.dataPath, "tsv");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0)
            {
                return;
            }

            string[] header = lines[0].Split('\t');
            if (header.Length < 2 || !string.Equals(header[0], "key", StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Localization Import", "First TSV row must start with: key", "OK");
                return;
            }

            List<string> languages = header.Skip(1).Select(UnescapeTsv).Where(language => !string.IsNullOrWhiteSpace(language)).ToList();
            Dictionary<string, EntrySnapshot> imported = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split('\t');
                if (cells.Length == 0)
                {
                    continue;
                }

                string key = UnescapeTsv(cells[0]).Trim();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                EntrySnapshot snapshot = new() { Key = key };
                for (int j = 0; j < languages.Count; j++)
                {
                    string text = j + 1 < cells.Length ? UnescapeTsv(cells[j + 1]) : "";
                    snapshot.Values[languages[j]] = text;
                }

                imported[key] = snapshot;
            }

            WriteSnapshots(imported.Values.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase).ToList());
        }

        private List<EntrySnapshot> ReadSnapshots()
        {
            List<EntrySnapshot> snapshots = new();
            for (int i = 0; i < _entries.arraySize; i++)
            {
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                EntrySnapshot snapshot = new()
                {
                    Key = entry.FindPropertyRelative("_key").stringValue
                };

                SerializedProperty values = entry.FindPropertyRelative("_values");
                for (int j = 0; j < values.arraySize; j++)
                {
                    SerializedProperty value = values.GetArrayElementAtIndex(j);
                    string language = value.FindPropertyRelative("_language").stringValue;
                    if (!string.IsNullOrWhiteSpace(language))
                    {
                        snapshot.Values[language] = value.FindPropertyRelative("_text").stringValue;
                    }
                }

                snapshots.Add(snapshot);
            }

            return snapshots;
        }

        private void WriteSnapshots(IReadOnlyList<EntrySnapshot> snapshots)
        {
            _entries.ClearArray();
            for (int i = 0; i < snapshots.Count; i++)
            {
                _entries.InsertArrayElementAtIndex(i);
                SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_key").stringValue = snapshots[i].Key;

                SerializedProperty values = entry.FindPropertyRelative("_values");
                values.ClearArray();
                foreach (KeyValuePair<string, string> pair in snapshots[i].Values.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
                {
                    AddValue(values, pair.Key, pair.Value);
                }
            }
        }

        private static string EscapeTsv(string value)
        {
            return (value ?? "")
                .Replace("\\", "\\\\")
                .Replace("\t", "\\t")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private static string UnescapeTsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            System.Text.StringBuilder builder = new(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] != '\\' || i + 1 >= value.Length)
                {
                    builder.Append(value[i]);
                    continue;
                }

                i++;
                builder.Append(value[i] switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '\\' => '\\',
                    _ => value[i]
                });
            }

            return builder.ToString();
        }

        private class EntrySnapshot
        {
            public string Key;
            public readonly Dictionary<string, string> Values = new(StringComparer.OrdinalIgnoreCase);
        }
    }
}
