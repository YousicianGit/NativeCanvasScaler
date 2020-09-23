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
    void Update()
    {
        this.text.text = $@"Native: {Screen.width} x {Screen.height}
Unity: {fullscreenTransform.rect.width} x {fullscreenTransform.rect.height}
Scale: {canvas.scaleFactor}x";
    }
}
