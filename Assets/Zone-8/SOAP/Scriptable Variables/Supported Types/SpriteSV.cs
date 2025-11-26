using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.SOAP.ScriptableVariable
{
    [CreateAssetMenu(menuName = "Zone8/SOAP/Scriptable Variable/Sprite")]
    public class SpriteSV : ScriptableVariable<Sprite>
    {
        [PreviewField(100, ObjectFieldAlignment.Left)]
        [ShowInInspector, ReadOnly]
        private Sprite SpritePreview => Value;
    }
}
