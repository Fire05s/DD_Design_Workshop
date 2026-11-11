using UnityEngine.UI;
using UnityEngine;

namespace Avocado
{
    /// <summary>
    /// Avocado wrapper class for Unity image object
    /// </summary>
    public class AvocadoImage
    {
        /// <summary>
        /// <c>Image</c> object associated with this <c>AvocadoImage</c> object
        /// </summary>
        public Image RuntimeElement { get; private set; }
        /// <summary>
        /// <c>Color</c> of the <c>Image</c>
        /// </summary>
        public Color ImageTintColor { get; private set; }
        /// <summary>
        /// State of the image
        /// </summary>
        public bool IsVisible { get; private set; }
        private Color _cachedColor;
        private Sprite _imageCache;

        public AvocadoImage(Image el)
        {
            RuntimeElement = el;
            ImageTintColor = RuntimeElement.color;
        }
        /// <summary>
        /// Toggle the invisibility state of the Image
        /// </summary>
        /// <param name="isVisible">Bool representing visibility</param>
        public void ToggleVisibility(bool isVisible)
        {
            IsVisible = isVisible;

            var color = RuntimeElement.color;
            color.a = isVisible ? 1f : 0f;
            RuntimeElement.color = color;
        }
        /// <summary>
        /// Change the sprite of the <c>Image</c> object
        /// </summary>
        /// <param name="sprite">Sprite object</param>
        public void Edit(Sprite sprite)
        {
            RuntimeElement.sprite = sprite;
        }
        /// <summary>
        /// Change the color of the <c>Image</c> object
        /// </summary>
        /// <param name="newColor">Color object</param>
        public void ChangeColor(Color newColor)
        {
            if (!IsVisible) {return;}
            ImageTintColor = newColor;
            RuntimeElement.color = ImageTintColor;
        }

        /// <summary>
        /// Enables/disables the image component.
        /// </summary>
        /// <param name="active"> True to set enable, false to disable. </param>
        public void SetActive(bool active)
        {
            if (RuntimeElement == null) return;

            RuntimeElement.gameObject.SetActive(active);
        }

        /// <summary>
        /// Sets the fill amount of the image component.
        /// </summary>
        /// <param name="fill"> Fill amount. </param>
        public void SetFillAmount(float fill)
        {
            if (RuntimeElement == null) return;
            RuntimeElement.fillAmount = fill;
        }
    }
}