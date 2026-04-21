using TMPro;
using UnityEngine;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{
    [RequireComponent(typeof(TMP_Text))]
    public class SVIntTextUpdater : SVTextUpdater<int>
    {
        protected override bool IsVariableHasValue()
        {
            return true; // Int always has a value, so we return true here.
        }
    }
}