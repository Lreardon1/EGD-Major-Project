using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OneOnPlayerController : MonoBehaviour
{

    private WebCamDevice? webCamDevice = null;
    private WebCamTexture webCamTexture = null;
    public OneOnCombatManager manager;
    public TMP_Text whatSeenText;

    private bool cardTurn = false;

    public RawImage debugScreen;

    /// <summary>
    /// A kind of workaround for macOS issue: MacBook doesn't state it's webcam as frontal
    /// </summary>
    protected bool forceFrontalCamera = false;

    /// <summary>
    /// WebCam texture parameters to compensate rotations, flips etc.
    /// </summary>
    protected OpenCvSharp.Unity.TextureConversionParams TextureParameters { get; private set; }

    IEnumerator ApplyCard(OneOnTurnActor.Element element)
    {
        if (element != OneOnTurnActor.Element.None)
        {
            whatSeenText.text = "Applying " + (element) + " to current actor.";
            manager.PlayCard(new OneOnApplyElementCard(element));
            yield return new WaitForSeconds(3.0f);
            manager.RequestCardTurnOver(this);
        } else
        {
            manager.PlayCard(null);
            manager.RequestCardTurnOver(this);
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        double t = Time.realtimeSinceStartupAsDouble;
        if (webCamTexture != null && webCamTexture.didUpdateThisFrame)
        {
            // this must be called continuously
            ReadTextureConversionParameters();

            // process texture with whatever method sub-class might have in mind
            ProcessTexture(webCamTexture);
        }

        if (cardTurn)
        {
            whatSeenText.text = "The current Aruco ID seen is " + acceptedCardID
                + ((acceptedCardID <= (int)OneOnTurnActor.Element.None && acceptedCardID >= 0) ?
                ". \nThis is APPLY " + ((OneOnTurnActor.Element)acceptedCardID).ToString() : "") 
                + ".";
            if (Input.anyKey)
            {
                // TODO : more logic for more types here
                cardTurn = false;
                OneOnTurnActor.Element element = OneOnTurnActor.Element.None;
                if (acceptedCardID >= 0 && acceptedCardID < (int)OneOnTurnActor.Element.None)
                    element = (OneOnTurnActor.Element)acceptedCardID;
                StartCoroutine(ApplyCard(element));
            }
        } else if (manager.currentPlayMode != OneOnCombatManager.PlayMode.CardAction)
        {
            whatSeenText.text = "It is not your turn.";
        }
    }

    /// <summary>
    /// Camera device name, full list can be taken from WebCamTextures.devices enumerator
    /// </summary>
    public string DeviceName
    {
        get
        {
            return (webCamDevice != null) ? webCamDevice.Value.name : null;
        }
        set
        {
            // quick test
            if (value == DeviceName)
                return;

            if (null != webCamTexture && webCamTexture.isPlaying)
                webCamTexture.Stop();

            // get device index
            int cameraIndex = -1;
            for (int i = 0; i < WebCamTexture.devices.Length && -1 == cameraIndex; i++)
            {
                if (WebCamTexture.devices[i].name == value)
                    cameraIndex = i;
            }

            // set device up
            if (-1 != cameraIndex)
            {
                webCamDevice = WebCamTexture.devices[cameraIndex];
                webCamTexture = new WebCamTexture(webCamDevice.Value.name);

                // read device params and make conversion map
                ReadTextureConversionParameters();

                webCamTexture.Play();
            }
            else
            {
                throw new System.ArgumentException(string.Format("{0}: provided DeviceName is not correct device identifier", this.GetType().Name));
            }
        }
    }

    public void RequestCardAction(OneOnCombatManager cManager)
    {
        manager = cManager;
        cardTurn = true;
    }

    /// <summary>
    /// This method scans source device params (flip, rotation, front-camera status etc.) and
    /// prepares TextureConversionParameters that will compensate all that stuff for OpenCV
    /// </summary>
    private void ReadTextureConversionParameters()
    {
        OpenCvSharp.Unity.TextureConversionParams parameters = new OpenCvSharp.Unity.TextureConversionParams();

        // frontal camera - we must flip around Y axis to make it mirror-like
        parameters.FlipHorizontally = forceFrontalCamera || webCamDevice.Value.isFrontFacing;

        // TODO:
        // actually, code below should work, however, on our devices tests every device except iPad
        // returned "false", iPad said "true" but the texture wasn't actually flipped

        // compensate vertical flip
        //parameters.FlipVertically = webCamTexture.videoVerticallyMirrored;

        // deal with rotation
        if (0 != webCamTexture.videoRotationAngle)
            parameters.RotationAngle = webCamTexture.videoRotationAngle; // cw -> ccw

        // apply
        TextureParameters = parameters;

        //UnityEngine.Debug.Log (string.Format("front = {0}, vertMirrored = {1}, angle = {2}", webCamDevice.isFrontFacing, webCamTexture.videoVerticallyMirrored, webCamTexture.videoRotationAngle));
    }

    /// <summary>
    /// Default initializer for MonoBehavior sub-classes
    /// </summary>
    protected virtual void Awake()
    {
        if (WebCamTexture.devices.Length > 0)
            DeviceName = WebCamTexture.devices[WebCamTexture.devices.Length - 1].name;
    }

    void OnDestroy()
    {
        if (webCamTexture != null)
        {
            if (webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
            webCamTexture = null;
        }

        if (webCamDevice != null)
        {
            webCamDevice = null;
        }
    }

    protected int acceptedCardID = -1;
    protected int visibleCardID = -1;
    protected float timeOfLost = 0.0f;
    public float timeToLose;
    public bool unhandledChange = false;

    protected bool ProcessTexture(WebCamTexture input)
    {
        if (!cardTurn)
            return false;


        Mat camMat = OpenCvSharp.Unity.TextureToMat(input);
        int[] ids;
        Point2f[][] corners;
        Point2f[][] rejected;
        BaseImageParser.FindArucoCards(camMat, out ids, out corners, out rejected);

        OpenCvSharp.Aruco.CvAruco.DrawDetectedMarkers(camMat, corners, ids);
        debugScreen.texture = OpenCvSharp.Unity.MatToTexture(camMat);


        if (ids.Length == 0 && visibleCardID != -1)
        {
            timeOfLost = Time.time;
            visibleCardID = -1;
        } else if (ids.Length == 0 && timeOfLost + timeToLose <= Time.time)
        {
            acceptedCardID = -1;
            unhandledChange = true;
            // TODO : logic?
        }
        if (ids.Length >= 1 && ids[0] != acceptedCardID)
        {
            // new card
            acceptedCardID = visibleCardID = ids[0];
            timeOfLost = Time.time;
            unhandledChange = true;
        }

        return true;
    }
}
