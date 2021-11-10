using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CVControllerBackLoader : MonoBehaviour
{
    [Header("Controller")]
    public CombatManager cm;
    public GameObject cvPanel;
    public GameObject regularPanel;
    [Header("Controller Images")]
    public RawImage planeImage;
    public RawImage goodSeeImage;
    public RawImage stickerImage1;
    public RawImage stickerImage2;
    public RawImage stickerImage3;
    public Image progressIndicator;
    [Header("Controller Text")]
    public TMP_Text playText;
    public TMP_Text cardText;

    public GameObject cvManagerPrefab;

    IEnumerator StartCV()
    {
        GameObject go = Instantiate(cvManagerPrefab);
        CardParserManager cmp = go.GetComponent<CardParserManager>();
        yield return new WaitForEndOfFrame();
        cmp.ActivateCVForCombat(this);
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (CombatManager.IsInCVMode && CardParserManager.instance != null)
        {
            if (WebCamTexture.devices.Length > 0)
                DeviceName = WebCamTexture.devices[0].name;
            CardParserManager.instance.ActivateCVForCombat(this);
            cvPanel.SetActive(true);
            regularPanel.SetActive(false);
        }
    }

    protected OpenCvSharp.Unity.TextureConversionParams TextureParameters { get; private set; }
    protected bool forceFrontalCamera = false;
    WebCamTexture webCamTexture;
    WebCamDevice? webCamDevice;

    // Update is called once per frame
    void Update()
    {
        if (webCamTexture == null) return;

        if (!webCamTexture.isPlaying)
        {
            print("Starting play again");
            // webCamTexture.Stop();
            webCamTexture.Play();
        }
        print("Update count: " + webCamTexture.updateCount);
        if (webCamTexture != null && webCamTexture.didUpdateThisFrame)
        {
            // this must be called continuously
            ReadTextureConversionParameters();
            if (CombatManager.IsInCVMode)
            {
                CardParserManager.instance.HandleNewImage(webCamTexture);
            }
        }
    }    

    /* HANDLE CAMERA THINGS */
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
                webCamTexture = new WebCamTexture(webCamDevice.Value.name, 720, 480, 20);
                DontDestroyOnLoad(webCamTexture);

                // read device params and make conversion map
                ReadTextureConversionParameters();

                webCamTexture.Play();
            }
            else
            {
                throw new System.ArgumentException(string.Format("{0}: provided DeviceName is not correct device identifier", this.GetType().Name));
            }
        }

        /*
        get
        {
            return (webCamDevice != null) ? webCamDevice.Value.name : null;
        }
        set
        {
            print("MODIFYING DEVICE NAME");
            // quick test
            if (value == DeviceName)
                return;

            if (null != webCamTexture && webCamTexture.isPlaying)
                webCamTexture.Stop();
            webCamTexture = null;
            webCamDevice = null;

            if (value == null) return;

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
                //webCamTexture = new WebCamTexture(webCamDevice.Value.name, 1920, 1080, 15);
                webCamTexture = new WebCamTexture(webCamDevice.Value.name);

                // read device params and make conversion map
                ReadTextureConversionParameters();

                webCamTexture.Play();
                print(webCamTexture.deviceName);
                print(webCamTexture.dimension);
                print(webCamDevice.Value.availableResolutions);
                print("Made new webcam texture: " + webCamTexture);
            }
            else
            {
                throw new System.ArgumentException(string.Format("{0}: provided DeviceName is not correct device identifier", this.GetType().Name));
            }
        }*/
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

        // TO-DO:
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

    void OnDestroy()
    {
        print("DESTROYING");
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
}
