using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // ManagedReference(SerializeReference)인지 확인
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.LabelField(position, label.text, "Use [SubclassSelector] with [SerializeReference] only.");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // 1. 첫 번째 줄(라벨과 버튼이 들어갈 영역) 계산
        Rect firstLineRect = position;
        firstLineRect.height = EditorGUIUtility.singleLineHeight;

        // 2. PrefixLabel을 사용하여 "Element X" 라벨을 먼저 그립니다.
        // 이 함수는 라벨을 그린 후, 그 옆의 남은 공간(버튼이 들어갈 영역)을 반환합니다.
        Rect buttonRect = EditorGUI.PrefixLabel(firstLineRect, label);

        // 3. 현재 할당된 타입 이름을 가져와서 버튼을 그립니다.
        string typeName = GetTypeName(property);

        // 버튼 스타일을 조금 더 깔끔하게 하기 위해 GUIStyle 적용 (선택사항)
        if (EditorGUI.DropdownButton(buttonRect, new GUIContent(typeName), FocusType.Keyboard))
        {
            ShowTypeMenu(property);
        }

        // 4. 폴아웃(내용물)이 열려있다면 자식 프로퍼티들을 아래에 그립니다.
        if (property.isExpanded)
        {
            // 한 줄 아래부터 시작하도록 위치 조정
            Rect childrenRect = position;
            childrenRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // PropertyField를 그릴 때 label을 GUIContent.none으로 주어야 "Element X"가 중복으로 나오지 않습니다.
            EditorGUI.PropertyField(childrenRect, property, GUIContent.none, true);
        }

        EditorGUI.EndProperty();
    }

    private void ShowTypeMenu(SerializedProperty property)
    {
        GenericMenu menu = new GenericMenu();
        Type targetType = GetManagedReferenceType(property);

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => targetType.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

        menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(property.managedReferenceFullTypename), () =>
        {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        });

        foreach (var type in types)
        {
            string menuName = type.Name;
            menu.AddItem(new GUIContent(menuName), property.managedReferenceFullTypename.Contains(type.FullName), () =>
            {
                property.managedReferenceValue = Activator.CreateInstance(type);
                property.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.ShowAsContext();
    }

    private string GetTypeName(SerializedProperty property)
    {
        string fullPath = property.managedReferenceFullTypename;
        if (string.IsNullOrEmpty(fullPath)) return "None (Empty)";

        string typePart = fullPath.Split(' ').Last();
        return typePart.Split('.').Last();
    }

    private Type GetManagedReferenceType(SerializedProperty property)
    {
        var parts = property.managedReferenceFieldTypename.Split(' ');
        if (parts.Length < 2) return null;
        return Assembly.Load(parts[0]).GetType(parts[1]);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 접혀있을 때는 한 줄 높이만, 펼쳐져 있을 때는 전체 높이 반환
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}