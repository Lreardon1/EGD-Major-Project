using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageParserManager : MonoBehaviour
{
    protected MatImageParser matParser;

    public RawImage im1;
    public RawImage im2;
    public RawImage im3;
    public RawImage im4;

    public RawImage im5;
    public Texture2D strayTestTexture1;
    public Texture2D strayTestTexture2;
    public float testStrayThresh = 128.0f;
    public int dilateAmount = 3;
    public int colShift = 4;
    public int rowShift = 0;
    public float sigma = 1;
    public int guassSize = 3;
    [Range(0, 1)]
    public float greyOut;

    [Space(10)]
    [Header("Mat Params")]
    public Texture2D matBaseImage;

    private WebCamDevice? webCamDevice = null;
    private WebCamTexture webCamTexture = null;

    [Space(10)]
    [Header("Card Params")]
    public Texture2D baseCardImage;

    /// <summary>
    /// A kind of workaround for macOS issue: MacBook doesn't state it's webcam as frontal
    /// </summary>
    protected bool forceFrontalCamera = false;

    /// <summary>
    /// WebCam texture parameters to compensate rotations, flips etc.
    /// </summary>
    protected OpenCvSharp.Unity.TextureConversionParams TextureParameters { get; private set; }


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
        Debug.Log("Time to complete: " + (Time.realtimeSinceStartupAsDouble - t) + ". Delta Time: " + Time.deltaTime);
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
        CardImageParser.InitCardTemplate(OpenCvSharp.Unity.TextureToMat(baseCardImage));

        matParser = new MatImageParser();
        matParser.Initialize(matBaseImage, this);


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
    
    protected bool ProcessTexture(WebCamTexture input)
    {

        Mat camMat = OpenCvSharp.Unity.TextureToMat(input);
        im1.color = Color.white;
        if (matParser.UpdateParse(camMat))
        {

            im1.color = Color.red;
            // = OpenCvSharp.Unity.MatToTexture(matParser.replaneImage);
            //im1.GetComponent<RectTransform>().sizeDelta = new Vector2(im1.texture.width, im1.texture.height);


            //im2.GetComponent<RectTransform>().sizeDelta = new Vector2(im2.texture.width, im2.texture.height);

            //string[] array = new string[matParser.structuredCards.Count];
            //matParser.structuredCards.Keys.CopyTo(array, 0);

            //im3.texture = OpenCvSharp.Unity.MatToTexture(matParser.structuredCards[array[0]].card.debugMat);
            //im4.texture = OpenCvSharp.Unity.MatToTexture(matParser.structuredCards[array[1]].card.debugMat);
            //im3.GetComponent<RectTransform>().sizeDelta = new Vector2(im3.texture.width, im3.texture.height);
        }
       // im2.texture = OpenCvSharp.Unity.MatToTexture(matParser.debugAruco);

        return true;
    }

    public Mat GetTestStray1()
    {
        return OpenCvSharp.Unity.TextureToMat(strayTestTexture1);
    }

    public Mat GetTestStray2()
    {
        return OpenCvSharp.Unity.TextureToMat(strayTestTexture2);
    }

    public void GiveDebugStray(Mat im)
    {
        im5.texture = OpenCvSharp.Unity.MatToTexture(im);
    }
}
