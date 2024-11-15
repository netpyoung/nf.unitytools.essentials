using System;
using System.Linq;
using UnityEditor;

namespace NF.UnityTools.Essentials.InitialSettingEditor
{
    public sealed class SeachableComboBox
    {
        private string[] _currOptions = Array.Empty<string>();
        private string[] _InputOptions = Array.Empty<string>();
        private int _selectedIndex = 0;
        private string _inputText = string.Empty;

        public SeachableComboBox(string[] options)
        {
            _currOptions = options;
            _InputOptions = options;
        }

        public string OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _selectedIndex = EditorGUILayout.Popup("Select Option", _selectedIndex, _currOptions);
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
            
            if (_selectedIndex >= 0 && _selectedIndex < _currOptions.Length)
            {
                return _currOptions[_selectedIndex];
            }
            return string.Empty;
        }

        private void UpdateComboBoxSelection()
        {
            _currOptions = _InputOptions.Where(x => x.Contains(_inputText)).ToArray();
            int newIndex = Array.FindIndex(_currOptions,
                option => option.Contains(_inputText, StringComparison.OrdinalIgnoreCase));

            if (newIndex != -1)
            {
                _selectedIndex = newIndex;
            }
        }

        private void UpdateInputText()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _currOptions.Length)
            {
                _inputText = _currOptions[_selectedIndex];
            }
        }
    }
}