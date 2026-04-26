#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.Core.Editor
{
  [CustomPropertyDrawer(typeof(InputIconResult))]
  public class InputIconResultDrawer : PropertyDrawer
  {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      float line = EditorGUIUtility.singleLineHeight;
      float sp = EditorGUIUtility.standardVerticalSpacing;
      return line * 3f + sp * 2f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      var iconProperty = property.FindPropertyRelative("Icon");
      var sliceProperty = property.FindPropertyRelative("Slice");
      var squareProperty = property.FindPropertyRelative("Square");
      if (iconProperty == null || sliceProperty == null || squareProperty == null)
      {
        EditorGUI.LabelField(position, label.text, "Malformed InputIconResult");
        return;
      }

      EditorGUI.BeginProperty(position, label, property);
      float line = EditorGUIUtility.singleLineHeight;
      float sp = EditorGUIUtility.standardVerticalSpacing;
      float y = position.y;
      EditorGUI.PropertyField(new Rect(position.x, y, position.width, line), iconProperty);
      y += line + sp;
      EditorGUI.PropertyField(new Rect(position.x, y, position.width, line), sliceProperty);
      y += line + sp;
      EditorGUI.PropertyField(new Rect(position.x, y, position.width, line), squareProperty);
      EditorGUI.EndProperty();
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
      // Create a container for the fields
      var container = new VisualElement();

      // Get references to the SerializedProperties
      var iconProperty = property.FindPropertyRelative("Icon");
      var sliceProperty = property.FindPropertyRelative("Slice");
      var squareProperty = property.FindPropertyRelative("Square");

      // Create ObjectField for the Icon property
      var iconField = new ObjectField("Icon")
      {
        objectType = typeof(Sprite),
        bindingPath = iconProperty.propertyPath
      };

      // Bind the object field to the serialized property
      iconField.BindProperty(iconProperty);

      // Create Toggle for the Slice property
      var sliceField = new Toggle("Slice")
      {
        bindingPath = sliceProperty.propertyPath
      };

      // Bind the toggle to the serialized property
      sliceField.BindProperty(sliceProperty);

      // Create Toggle for the Square property
      var squareField = new Toggle("Square")
      {
        bindingPath = squareProperty.propertyPath
      };

      // Bind the toggle to the serialized property
      squareField.BindProperty(squareProperty);
      
      // Add fields to the container
      container.Add(iconField);
      container.Add(sliceField);
      container.Add(squareField);
      
      return container;
    }
  }
}
#endif
