using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.Question.Runtime.Base
{
    /// <summary>
    /// Represents a category for questions.
    /// </summary>

    [CreateAssetMenu(menuName = "Questions/Category")]
    public class CategorySo : ScriptableObject
    {
        [HorizontalGroup("Category")]
        public string Name;

        [HorizontalGroup("Category"), PreviewField(60), HideLabel]
        public Sprite Icon;

    }
}
