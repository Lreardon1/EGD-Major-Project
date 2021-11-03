using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CombatPopup : MonoBehaviour
{
    private Vector3 startingPos;
    private float startingTime;

    public TMP_Text text;
    public RawImage image;

    public AnimationCurve ascendCurve;
    public float ascendAmp;
    public AnimationCurve moveCurve;
    public float moveAmp;
    public AnimationCurve fadeCurve;
    public float timeTaken = 2;

    public void Init(string text, Texture2D popImage, Color col,
        AnimationCurve ascendCurve, float ascendAmp, AnimationCurve moveCurve, float moveAmp,
        AnimationCurve fadeCurve, float timeTaken, float textSize = -1)
    {
        // get anim curves
        this.ascendCurve = ascendCurve;
        this.ascendAmp = ascendAmp;
        this.moveCurve = moveCurve;
        this.moveAmp = moveAmp;
        this.fadeCurve = fadeCurve;
        this.timeTaken = timeTaken;

        if (textSize > 0)
            this.text.fontSize = textSize;

        Init(text, popImage, col);
    }

    public void Init(string text, Texture2D popImage, Color col)
    {
        // get start state
        startingPos = transform.position;
        startingTime = Time.time;

        // set popup data
        this.text.text = text;
        image.gameObject.SetActive(popImage != null);
        if (popImage != null)
        {
            print("ATTEMPTING with" + popImage + " with " + (float)popImage.height + " and " + (float)popImage.width);
            // TODO : resize image to fit
            this.image.rectTransform.sizeDelta =
                new Vector2(
                    this.image.rectTransform.sizeDelta.x,
                    this.image.rectTransform.sizeDelta.y * ((float)popImage.height / (float)popImage.width));

            this.image.texture = popImage;
            this.image.color = col;
        }
    }

    private void Update()
    {
        if (ascendCurve == null)
            return;

        float e = (Time.time - startingTime) / timeTaken;
        if (e > 1.0f) Destroy(gameObject);

        float y = ascendCurve.Evaluate(e) * ascendAmp;
        float x = moveCurve.Evaluate(e) * moveAmp;
        float f = fadeCurve.Evaluate(e);

        this.text.alpha = f;
        this.image.canvasRenderer.SetAlpha(f);

        transform.position = startingPos + (x * transform.right) + (y * transform.up);
    }
}
