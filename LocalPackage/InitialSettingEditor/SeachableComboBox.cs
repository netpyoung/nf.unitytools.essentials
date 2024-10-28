using System;
using UnityEditor;

namespace NF.UnityTools.Essentials.InitialSettingEditor
{
    public sealed class SeachableComboBox
    {
        private string[] _options = null;
        private int _selectedIndex = 0;
        private string _inputText = string.Empty;

        public SeachableComboBox(string[] options)
        {
            _options = options;
        }

        public void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _selectedIndex = EditorGUILayout.Popup("Select Option", _selectedIndex, _options);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateInputText();
            }

            EditorGUI.BeginChangeCheck();
            _inputText = EditorGUILayout.TextField("Input", _inputText);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateComboBoxSelection();
            }
        }

        private void UpdateComboBoxSelection()
        {
            int newIndex = Array.FindIndex(_options,
                option => option.Contains(_inputText, StringComparison.OrdinalIgnoreCase));

            if (newIndex != -1)
            {
                _selectedIndex = newIndex;
            }
        }

        private void UpdateInputText()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _options.Length)
            {
                _inputText = _options[_selectedIndex];
            }
        }
    }
}