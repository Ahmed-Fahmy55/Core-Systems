using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Zone8.Tweening
{
    public static class TweeningExtensions
    {

        /// <summary>
        /// Animates the scale of a TextMeshProUGUI element to the specified target scale over the given duration.
        /// </summary>
        /// <param name="text">The TextMeshProUGUI element to animate.</param>
        /// <param name="targetScale">The target scale to reach.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        /// <returns>A Tween representing the animation.</returns>
        public static Tween DOScale(this TextMeshProUGUI text, Vector3 targetScale, float duration)
        {
            // Ensure the text object is valid
            if (text == null)
            {

                Debug.LogWarning("TextMeshProUGUI is null.");
                return null;
            }

            // Animate the RectTransform's localScale to the target scale
            return text.rectTransform.DOScale(targetScale, duration);
        }

        /// <summary>
        /// Animates the font size of a TextMeshProUGUI element to the specified target size over the given duration.
        /// </summary>
        /// <param name="text">The TextMeshProUGUI element to animate.</param>
        /// <param name="targetFontSize">The target font size to reach.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        /// <returns>A Tween representing the animation.</returns>
        public static Tween DOFontSize(this TextMeshProUGUI text, float targetFontSize, float duration)
        {
            // Ensure the text object is valid
            if (text == null)
            {
                Debug.LogWarning("TextMeshProUGUI is null.");
                return null;
            }

            // Animate the fontSize to the target value
            return DOTween.To(() => text.fontSize, x => text.fontSize = x, targetFontSize, duration);
        }

        /// <summary>
        /// Animates the text to reveal characters one by one over the specified duration.
        /// </summary>
        /// <param name="textComponent">The TextMeshProUGUI component to animate.</param>
        /// <param name="toText">The complete text to display.</param>
        /// <param name="duration">The total duration of the animation in seconds.</param>
        /// <returns>A Tween representing the animation.</returns>
        public static Tween DoText(this TextMeshProUGUI textComponent, string toText, float duration)
        {
            // Ensure the text component is valid
            if (textComponent == null)
            {
                Debug.LogWarning("TextMeshProUGUI component is null.");
                return null;
            }

            textComponent.maxVisibleCharacters = 0;
            // Set the full text
            textComponent.text = toText;

            // Calculate the delay between each character reveal
            int characterCount = textComponent.text.Length;
            float delayPerCharacter = duration / characterCount;

            // Create a sequence to animate each character
            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < characterCount; i++)
            {
                sequence.Append(DOTween.To(() => textComponent.maxVisibleCharacters,
                    x => textComponent.maxVisibleCharacters = x, i, delayPerCharacter));
            }
            return sequence;
        }

        // ... existing methods ...

        /// <summary>
        /// Animates the value of a Slider to the specified target value over the given duration.
        /// </summary>
        /// <param name="slider">The Slider to animate.</param>
        /// <param name="targetValue">The target value to reach.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        /// <returns>A Tween representing the animation.</returns>
        public static Tween DOFill(this Slider slider, float targetValue, float duration)
        {
            if (slider == null)
            {
                Debug.LogWarning("Slider is null.");
                return null;
            }

            return DOTween.To(() => slider.value, x => slider.value = x, targetValue, duration);
        }
    }
}
