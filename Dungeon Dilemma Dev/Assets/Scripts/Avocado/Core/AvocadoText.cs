using System;
using TMPro;
using UnityEngine;

namespace Avocado {
    /// <summary>
    /// Avocado Text wrapper class, wraps the logic of changing text and containing TMP_Text ref
    /// </summary>
    public class AvocadoText {
        /// <summary>
        /// The actual <c>TMP_Text</c> object associated with this <c>AvocadoText</c> object
        /// </summary>
        public TMP_Text RuntimeElement;
        public string Content => this.RuntimeElement.text;

        public AvocadoText(TMP_Text el) {
            this.RuntimeElement = el;
        }

        /// <summary>
        /// Edit content of the <c>RuntimeElement</c>
        /// </summary>
        /// <param name="newContent">The new text content to be put as the value of <c>RuntimeElement</c>'s text</param>
        public void Edit(string newContent) {
            RuntimeElement.text = newContent;
        }

        /// <summary>
        /// Sets the color of the <c>RuntimeElement</c>
        /// </summary>
        /// <param name="newColor"> The new color to be put as the value of <c>RuntimeElement</c>'s color</param>
        public void SetColor(Color newColor)
        {
            RuntimeElement.color = newColor;
        }

        /// <summary>
        /// Enables and disables the text game object.
        /// </summary>
        /// <param name="isVisible"> Enabled when true, false otherwise. </param>
        public void ToggleVisibility(bool isVisible)
        {
            RuntimeElement.gameObject.SetActive(isVisible);
        }
    }
}