using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextUpdater : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform fullscreenTransform;
    public Text text;

    // Update is called once per frame
    private void Update()
    {
        this.text.text = $@"Screen: {Screen.width} x {Screen.height}
Canvas: {fullscreenTransform.rect.width} x {fullscreenTransform.rect.height}
Scale: {canvas.scaleFactor}x";
    }
}
