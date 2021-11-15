using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using OpenCvSharp.Aruco;
using OpenCvSharp.XFeatures2D;
using System.Linq;
using System;
using UnityEngine.Events;
using UnityEditor;


// TODO HERE : update to function as best as possible with all these different modes, shouldn't be terrible but won't be super nice.
public class CardParser : MonoBehaviour
{
    public enum ParseMode
    {
        DeckMode,
        HandMode,
        AllMode,
        StickerlessMode
    }

    public CardParserManager cardParserManager;
    [Space(10)]
    public Texture2D staticTestImage;
    
    // BOUNDING BOXES
    [Space(10)]
    [Header("Bounding Boxes")]
    [SerializeField, HideInInspector]
    private BoundingBox bottomRightBoundingBox;
    [SerializeField, HideInInspector]
    private BoundingBox upperLeftBoundingBox;
    [SerializeField, HideInInspector]
    private BoundingBox stickerBoundingBox1;
    [SerializeField, HideInInspector]
    private BoundingBox stickerBoundingBox2;
    [SerializeField, HideInInspector]
    private BoundingBox stickerBoundingBox3;
    [SerializeField, HideInInspector]
    private BoundingBox elementColorBoundingBox;

    [Space(10)]
    [Header("Match and Planing Scalars")]
    public int cornerReplaneOffset = 25;
    public float matchThresh = 0.8f;
    public float tagLineIntersectThresh = 4.0f;
    private int defaultCardWidth = 0;
    private int defaultCardHeight = 0;
    private int defaultStickerWidth;
    private int defaultStickerHeight;
    private int defaultCardPlusBorderWidth = 0;
    private int defaultCardPlusBorderHeight = 0;

    private ParseMode mode = ParseMode.AllMode;

    public void UpdateMode(ParseMode handMode)
    {
        // TODO : currently just do something simple for only elements
        // mode = handMode;
        Debug.Log("UNIMPLEMENTED FUNCTION UPDATEMODE, FIX JAY FIX!");
    }

    [Space(10)]
    [Header("Template Cards")]
    public int borderAmount = 10;
    public float defaultCardResizeAmount = 0.5f;
    public ScriptableCardImage[] cardTemplates;
    private Dictionary<int, List<TemplateCardData>> templateCardDict = new Dictionary<int, List<TemplateCardData>>();
    private Dictionary<CardType, CardTypeTemplateData> cardTypeDict = new Dictionary<CardType, CardTypeTemplateData>();
    private Dictionary<CardElement, CardElementTemplateData> cardElementDict = new Dictionary<CardElement, CardElementTemplateData>();
    public ScriptableCardSticker[] stickerTemplates;
    private Dictionary<string, StickerTemplateData> cardStickerDict = new Dictionary<string, StickerTemplateData>();
    private WebCamDevice? webCamDevice = null;
    private WebCamTexture webCamTexture = null;
    
    

    /// <summary>
    /// A kind of workaround for macOS issue: MacBook doesn't state it's webcam as frontal
    /// </summary>
    protected bool forceFrontalCamera = false;

    /// <summary>
    /// WebCam texture parameters to compensate rotations, flips etc.
    /// </summary>
    protected OpenCvSharp.Unity.TextureConversionParams TextureParameters { get; private set; }
    

    private void Awake()
    {
        //MakeBoundingBoxFromEditorStr(boundBoxText);
        BakeCardTemplateData();
        

    }


    private int ConvertToIntMask(CardElement element)
    {
        return (int)element;
    }

    private int ConvertToIntMask(CardType type)
    {
        return (int)type;
    }

    private Mat ExtractCardType(Mat cardMat)
    {
        return bottomRightBoundingBox.CropByBox(cardMat);
    }
    private Scalar ExtractCardElement(Mat cardMat)
    {
        Mat elementBox = elementColorBoundingBox.CropByBox(cardMat);
        return elementBox.Mean();
    }

    /**
     * Bake the template card data for quicker use in detection and parsing.
     * Uses a pretty large amount of memory for great gains in performance.
     * Called on START using provided scriptable objects of cards.
     */
    private void BakeCardTemplateData()
    {
        // BAKE ON CARDS
        templateCardDict.Add(0, new List<TemplateCardData>());
        
        foreach (ScriptableCardImage card in cardTemplates)
        {
            float resizeAmount = 1.0f / defaultCardResizeAmount;
            defaultCardWidth = Mathf.RoundToInt(card.cardTexture.width / resizeAmount);
            defaultCardHeight = Mathf.RoundToInt(card.cardTexture.height / resizeAmount);
            defaultCardPlusBorderWidth = defaultCardWidth + (borderAmount * 2);
            defaultCardPlusBorderHeight = defaultCardHeight + (borderAmount * 2);

            // extract keypoints
            int cardType = ConvertToIntMask(card.cardElement);
            int cardElement = ConvertToIntMask(card.cardType);

            using (Mat cardMat = OpenCvSharp.Unity.TextureToMat(card.cardTexture).Resize(new Size(defaultCardWidth, defaultCardHeight)))
            {

                // extract card type and elements if new, BEFORE WE MAKE A BORDER FOR THE IMAGE!!!
                if (!cardTypeDict.ContainsKey(card.cardType))
                {
                    CardTypeTemplateData typeData = new CardTypeTemplateData(ExtractCardType(cardMat));
                    cardTypeDict.Add(card.cardType, typeData);
                }
                if (!cardElementDict.ContainsKey(card.cardElement))
                {
                    CardElementTemplateData elementData = new CardElementTemplateData(ExtractCardElement(cardMat));
                    cardElementDict.Add(card.cardElement, elementData);
                }
                
                // create the image we'll bake for self and keypoints
                Cv2.CopyMakeBorder(cardMat, cardMat, borderAmount, borderAmount, borderAmount, borderAmount, BorderTypes.Constant, Scalar.White);

                // get and store keypoints
                GetKeypoints(cardMat, out KeyPoint[] kp, out Mat des);

                // make data to store
                TemplateCardData cardKeypointData = new TemplateCardData(MakeHSHistrogram(cardMat), kp, des, card.cardID, card.cardName, card.cardType, card.cardElement);

                // if the current card element and/or card type doesn't have a list, make one
                if (!templateCardDict.ContainsKey(cardType | cardElement))
                    templateCardDict.Add(cardType | cardElement, new List<TemplateCardData>());
                if (!templateCardDict.ContainsKey(cardElement))
                    templateCardDict.Add(cardElement, new List<TemplateCardData>());
                if (!templateCardDict.ContainsKey(cardType))
                    templateCardDict.Add(cardType, new List<TemplateCardData>());

                // Add the !REFERENCE! of this data to 4 lists: one for both known, 2 for either type or element known, 1 for totally clueless.
                templateCardDict.TryGetValue(cardType | cardElement, out List<TemplateCardData> cards);
                cards.Add(cardKeypointData);
                templateCardDict.TryGetValue(cardType, out cards);
                cards.Add(cardKeypointData);
                templateCardDict.TryGetValue(cardElement, out cards);
                cards.Add(cardKeypointData);
                templateCardDict.TryGetValue(0, out cards);
                cards.Add(cardKeypointData); // this is the unknown category: 0.
            }
        }

        // TODO : Get sticker data
        foreach (ScriptableCardSticker sticker in stickerTemplates)
        {
            Mat si = OpenCvSharp.Unity.TextureToMat(sticker.stickerTexture);

            defaultStickerWidth = si.Width;
            defaultStickerHeight = si.Height;

            using (Mat bin = new Mat()) {
                Cv2.CvtColor(si, bin, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(bin, bin, 200, 255, ThresholdTypes.Otsu);
                Scalar meanIconColor = Cv2.Mean(si, bin);
                Cv2.BitwiseNot(bin, bin);
                Scalar meanBackColor = Cv2.Mean(si, bin);
                Cv2.BitwiseNot(bin, bin);
                print("For " + sticker.stickerName + ", Icon color: " + meanIconColor + " back color: " + meanBackColor);
                StickerTemplateData stickerData = new StickerTemplateData(bin, meanBackColor, meanIconColor);
                cardStickerDict.Add(sticker.stickerName, stickerData);
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



    private GameObject previousCard;

    public bool ProcessTexture(WebCamTexture input)
    {
        // if (!shouldUpdate) return false;

        if (!Input.GetKey(KeyCode.D))
        {
            using (Mat cardScene = OpenCvSharp.Unity.TextureToMat(input))
            {
                CustomCard card = ParseCard(cardScene, null);
                if (card == null)
                {
                    UpdateCardDetected(null, -1);
                    return true;
                }
                List<GameObject> possibleCards = cardParserManager.GetCardsOfName(card.cardName);
                print("COUNT: " + possibleCards.Count);
                GameObject bestCard = possibleCards[0];
                AttemptToGetStickerMods(cardScene, card, lastGoodReplane, possibleCards);
                UpdateCardDetected(bestCard, bestCard != null ? card.cardID : -1);

            }
        }
        return true;
    }




    /*************************************************************************************************
    /*************************************************************************************************
    /*************************************************************************************************
    /*************************************************************************************************
    /*************************************************************************************************
    /*************************************************************************************************
    /*************************************************************************************************
    /*************************************************************************************************/

    private object AttemptToGetStickerMods(Mat cardScene, CustomCard card, Mat cardMat, List<GameObject> possibleCards)
    {
        // get rhombus bounds
        DetectContours(cardMat, out Point2f[][] contours, out HierarchyIndex[] h);

        // get possible sticker bounds
        Point2f[] possibleSticker1 = GetStickerBoundByContour(contours, stickerBoundingBox1.GetCenter(), stickerBoundingBox1.GetArea(cardMat));
        Point2f[] possibleSticker2 = GetStickerBoundByContour(contours, stickerBoundingBox2.GetCenter(), stickerBoundingBox2.GetArea(cardMat));
        Point2f[] possibleSticker3 = GetStickerBoundByContour(contours, stickerBoundingBox3.GetCenter(), stickerBoundingBox3.GetArea(cardMat));

        object finalSticker1 = null;
        object finalSticker2 = null;
        object finalSticker3 = null;

        if (possibleSticker1 != null)
            finalSticker1 = ParseStickerByContour(possibleSticker1, cardScene, card, cardMat, 0);
        else
            finalSticker1 = ParseStickerByCrop(cardScene, card, cardMat, stickerBoundingBox1, 0);

        if (possibleSticker2 != null)
            finalSticker2 = ParseStickerByContour(possibleSticker2, cardScene, card, cardMat, 1);
        else
            finalSticker2 = ParseStickerByCrop(cardScene, card, cardMat, stickerBoundingBox2, 1);

        if (possibleSticker3 != null)
            finalSticker3 = ParseStickerByContour(possibleSticker3, cardScene, card, cardMat, 2);
        else
            finalSticker3 = ParseStickerByCrop(cardScene, card, cardMat, stickerBoundingBox1, 2);

        // TODO
        return new Tuple<object, object, object>(finalSticker1, finalSticker2, finalSticker3);
    }

    private Point2f[] GetStickerBoundByContour(Point2f[][] contours, Point2f containedPoint, float expectedArea)
    {
        float bestRatio = 1.5f;
        Point2f[] bestContour = null;
        foreach (Point2f[] contour in contours)
        {
            bool inside = Cv2.PointPolygonTest(contour, containedPoint, false) > 0.0;

            float cArea = (float)Cv2.ContourArea(contour);
            float areaRatio = CompareAreaRatio(expectedArea, cArea);
            if (bestRatio > areaRatio)
            {
                bestRatio = areaRatio;
                bestContour = contour;
            }
        }

        return bestContour;
    }

    private object ParseStickerByContour(Point2f[] stickerContour, Mat cardScene, CustomCard card, Mat cardMat, int ID)
    {
        Point2f[] corners = new Point2f[]
        {
            new Point2f(0,0),
            new Point2f(defaultStickerWidth, 0),
            new Point2f(defaultStickerWidth, defaultStickerHeight),
            new Point2f(0, defaultStickerHeight)
        };
        Mat stickerTransMat = Cv2.GetPerspectiveTransform(stickerContour, corners);
        Mat stickerMat = new Mat();
        Cv2.WarpPerspective(cardMat, stickerMat, stickerTransMat, new Size(defaultStickerWidth, defaultStickerHeight));
        cardParserManager.UpdateStickerDebugs(ID, stickerMat);
        return null;
    }

    private object ParseStickerByCrop(Mat cardScene, CustomCard card, Mat cardMat, BoundingBox stickerBoundingBox1, int ID)
    {
        return null;
    }

    [HideInInspector]
    public UnityEvent<GameObject, int> StableUpdateEvent = new UnityEvent<GameObject, int>();
    [HideInInspector]
    public UnityEvent<GameObject, int> ToNullUpdateEvent = new UnityEvent<GameObject, int>();
    [HideInInspector]
    public UnityEvent<GameObject, int> ToNewUpdateEvent = new UnityEvent<GameObject, int>();

    private float timeSinceLastUpdate = -1.0f;
    public float timeRequiredForNull;
    public float timeRequiredForNew;
    public float timeRequiredForNewFromNull;

    private void UpdateCardDetected(GameObject card, int id)
    {
        print("Updating for card: " + (card != null ? card.GetComponent<Card>().name : "NULL") + " with previous " + 
            (previousCard != null ? previousCard.GetComponent<Card>().cardName : " NULL"));

        // if card is the same as last, don't update
        if (card == previousCard)
        {
            timeSinceLastUpdate = Time.time;
            StableUpdateEvent.Invoke(card, id);
        }

        // update to null
        if (card == null && timeSinceLastUpdate + timeRequiredForNull <= Time.time)
        {
            previousCard = card;
            timeSinceLastUpdate = Time.time;
            ToNullUpdateEvent.Invoke(card, id);
        }

        // update to different card
        if (card != null && previousCard == null && timeSinceLastUpdate + timeRequiredForNewFromNull <= Time.time)
        {
            previousCard = card;
            timeSinceLastUpdate = Time.time;
            ToNewUpdateEvent.Invoke(card, id);
        } else if (card != null && timeSinceLastUpdate + timeRequiredForNew <= Time.time)
        {
            previousCard = card;
            timeSinceLastUpdate = Time.time;
            ToNewUpdateEvent.Invoke(card, id);
        }
    }

    public class CustomCard
    {
        public float certainty;
        public int cardID;
        public BoundingBox bb;
        public CardType cardType;
        public CardElement cardElement;
        public string cardName;
        public Mat homoMat;
        public CustomCard(float certainty, BoundingBox inSceneBB, CardType type, CardElement element, int cardID, string cardName, Mat replaneMat)
        {
            this.certainty = certainty;
            this.cardID = cardID;
            this.bb = inSceneBB;
            this.cardType = type;
            this.cardElement = element;
            this.cardName = cardName;
            this.homoMat = replaneMat;
        }

        public static bool Equiv(CustomCard card1, CustomCard card2)
        {
            if (card1 == card2) return true;
            if (card1 == null || card2 == null) return false;

            return card1.cardID == card2.cardID;
        }
    }



















    private Point[][] FilterContoursByCornerCount(Point[][] contours, int lengthReq)
    {
        List<Point[]> newContours = new List<Point[]>();
        for (int i = 0; i < contours.Length; ++i)
        {
            if (contours[i].Length == lengthReq)
                newContours.Add(contours[i]);
        }
        print(newContours.Count);
        return newContours.ToArray();
    }

    private float ComputeApparentRatio(Point2f[] c)
    {
        float leftRight = (float)(c[0].DistanceTo(c[1]) + c[1].DistanceTo(c[2]));
        float bottomTop = (float)(c[2].DistanceTo(c[3]) + c[2].DistanceTo(c[3]));

        return Mathf.Max(leftRight / bottomTop, bottomTop / leftRight);
    }

    // WARNING: ASSUMES ALL CONTOURS ARE ALREADY FILTERED S.T. EACH HAS 4 CORNERS
    private Point2f[][] Sort4CornerContoursByRatio(Point2f[][] contours)
    {
        List<Point2f[]> newContours = new List<Point2f[]>(contours);
        newContours.Sort(delegate (Point2f[] c1, Point2f[] c2) 
            { return ComputeApparentRatio(c1).CompareTo(ComputeApparentRatio(c2)); });
        
        return newContours.ToArray();
    }



    /**
     * Make an RGB/BGR histogram, useful for image comparisons.
    */
    public Mat MakeRGBHistrogram(Mat im)
    {
        Cv2.CvtColor(im, im, ColorConversionCodes.BGR2HSV);
        int[] histSize = new int[] { 30, 30, 30 };
        float[] colorRange = new float[] { 0, 256 };
        float[][] ranges = new float[][] { colorRange, colorRange, colorRange };
        Mat hist = new Mat();
        // we compute the histogram from the 0-th and 1-st channels
        int[] channels = new int[] { 0, 1, 2 };

        Cv2.CalcHist(new Mat[] { im }, channels, null, hist, 3, histSize, ranges);
        return hist;
    }


    /**
     * Make HSV histogram using H and S values, 
     * this is much more lighting independent!!!
     */
    public Mat MakeHSHistrogram(Mat im)
    {
        Mat hsvIM = new Mat();

        Cv2.CvtColor(im, hsvIM, ColorConversionCodes.BGR2HSV);
        // Quantize the hue to 30 levels
        // and the saturation to 32 levels
        int hbins = 30, sbins = 32;
        int[] histSize = { hbins, sbins };
        // hue varies from 0 to 179, see cvtColor
        float[] hranges = { 0, 180 };
        // saturation varies from 0 (black-gray-white) to
        // 255 (pure spectrum color)
        float[] sranges = { 0, 256 };
        float[][] ranges = { hranges, sranges };

        // we compute the histogram from the 0-th and 1-st channels
        int[] channels = { 0, 1 };

        Mat hist = new Mat();
        Cv2.CalcHist(new Mat[] { hsvIM }, channels, null, hist, 2, histSize, ranges);
        return hist;
    }

    /**
     * Make HSV histogram using H and S values, 
     * this is much more lighting independent!!!
     */
    public Mat MakeHSHistrogram(Mat im, Mat mask)
    {
        Mat hsvIM = new Mat();

        Cv2.CvtColor(im, hsvIM, ColorConversionCodes.BGR2HSV);
        // Quantize the hue to 30 levels
        // and the saturation to 32 levels
        int hbins = 30, sbins = 32;
        int[] histSize = { hbins, sbins };
        // hue varies from 0 to 179, see cvtColor
        float[] hranges = { 0, 180 };
        // saturation varies from 0 (black-gray-white) to
        // 255 (pure spectrum color)
        float[] sranges = { 0, 256 };
        float[][] ranges = { hranges, sranges };

        // we compute the histogram from the 0-th and 1-st channels
        int[] channels = { 0, 1 };

        Mat hist = new Mat();
        Cv2.CalcHist(new Mat[] { hsvIM }, channels, mask, hist, 2, histSize, ranges);
        return hist;
    }


    private float GetHistogramMatch(Mat im1, Mat template)
    {
        // hist = cv2.calcHist([image], [0, 1, 2], None, [8, 8, 8],
        // [0, 256, 0, 256, 0, 256])
        Mat im1Hist = MakeHSHistrogram(im1);
        Mat tempHist = MakeHSHistrogram(template);
        im1Hist = im1Hist / (im1Hist.Norm() + 0.00001f);
        tempHist = tempHist / (tempHist.Norm() + 0.00001f);

        float comp = (float)Cv2.CompareHist(im1Hist, tempHist, HistCompMethods.Correl);
        im1Hist.Release();
        tempHist.Release();

        return comp;
    }

    private float GetDiffAndHistMatchScore(Mat im1, Mat template, float histWeight)
    {
        using (Mat blur1 = new Mat())
        using (Mat templateBlur = new Mat())
        using (Mat diff = new Mat()) 
        {
            Cv2.CvtColor(im1, blur1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(template, templateBlur, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(blur1, blur1, new Size(3, 3), 0);
            Cv2.GaussianBlur(templateBlur, templateBlur, new Size(3, 3), 0);
            Cv2.Threshold(blur1, blur1, 100, 255, ThresholdTypes.Otsu);
            Cv2.Threshold(templateBlur, templateBlur, 100, 255, ThresholdTypes.Otsu);
            Cv2.Absdiff(templateBlur, blur1, diff);

            Scalar channelSums = Cv2.Sum(diff);
            double sum = channelSums.Val0 + channelSums.Val1 + channelSums.Val2 + channelSums.Val3;

            float diffOp = 1.0f - ((float)sum / (template.Size().Height * template.Size().Width * 255));
            if (diffOp > 0.9) {
                //diffImages[0].texture = OpenCvSharp.Unity.MatToTexture(diff);
                print(diffOp);
            }
            float histOp = GetHistogramMatch(im1, template);

            return (histWeight * histOp) + ((1f - histWeight) * diffOp);
        }
    }

    private float GetShapeScore(Mat im, out int type)
    {
        //Debug.Log("ERROR: Shape score outdated from changes, multiple shapes in type possible.");
        type = -1;
        return 0.0f;
        using (Mat greyBR = new Mat())
        using (Mat greyIM = new Mat()) 
        {
            // TODO : perform this on all of the shapes, and store them s.t. you don't recall everytime 
            // TODO : jerry rigging is fine but should not be getting done at runtime.
            // (might also be more stable if you guarenteed success in editor rather than runtime)
            //Cv2.CvtColor(brMatch, greyBR, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(greyBR, greyBR, new Size(5, 5), 2);
            Cv2.AdaptiveThreshold(greyBR, greyBR, 125, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 5, 12);
            Point[][] templateShape;
            HierarchyIndex[] h;
            Cv2.FindContours(greyBR, out templateShape, out h, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            if (templateShape.Length != 2)
                print("WELL SHIT, that's a problem now!   " + templateShape.Length);

            // get all grey shapes and test them
            Cv2.CvtColor(im, greyIM, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(greyIM, greyIM, new Size(3, 3), 2);
            Cv2.AdaptiveThreshold(greyIM, greyIM, 125, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 5, 12);
            Point[][] imShapes;
            Cv2.FindContours(greyIM, out imShapes, out h, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);

            float bestMatch = 10.0f;
            foreach (Point[] shape in imShapes)
            {
                float match = (float)Cv2.MatchShapes(templateShape[1], shape, ShapeMatchModes.I1);
                bestMatch = Mathf.Min(bestMatch, match);
            }
            type = -1;
            return bestMatch;
        }
    }
    
    private float AttemptToMatchByShape(Mat scene, Mat template, Point2f[] rect)
    {
        Size s = template.Size();
        // destination rect
        Point2f[] destRect = new Point2f[] {
            new Point2f(0, 0), new Point2f(s.Width, 0),
            new Point2f(s.Width, s.Height), new Point2f(0, s.Height) };
        // revolving rect
        Point2f[] newRect = new Point2f[] {
            new Point2f(rect[0].X, rect[0].Y), new Point2f(rect[1].X, rect[1].Y),
            new Point2f(rect[2].X, rect[2].Y), new Point2f(rect[3].X, rect[3].Y) };
        // remapping to have a better time replaning
        int top = Mathf.FloorToInt(Mathf.Min(newRect[0].Y, newRect[1].Y, newRect[2].Y, newRect[3].Y));
        int bottom = Mathf.CeilToInt(Mathf.Max(newRect[0].Y, newRect[1].Y, newRect[2].Y, newRect[3].Y));
        int left = Mathf.FloorToInt(Mathf.Min(newRect[0].X, newRect[1].X, newRect[2].X, newRect[3].X));
        int right = Mathf.CeilToInt(Mathf.Max(newRect[0].X, newRect[1].X, newRect[2].X, newRect[3].X));

        for (int i = 0; i < newRect.Length; ++i)
        {
            newRect[i].Y -= top;
            newRect[i].X -= left;
        }

        using (Mat subCard = scene.SubMat(top, bottom, left, right))
        using (Mat perspMat = Cv2.GetPerspectiveTransform(newRect, destRect))
        using (Mat warped = new Mat())
        {
            Cv2.WarpPerspective(subCard, warped, perspMat, template.Size());
            int shapeType; // TODO : replace with enum
            return 1.0f - GetShapeScore(warped, out shapeType);
        }
    }

    private float AttemptToMatchCardByTemplate(Mat card, Mat template, Point2f[] rect, out int rotCount)
    {
        Size s = template.Size();
        // destination rect
        Point2f[] destRect = new Point2f[] {
            new Point2f(0, 0), new Point2f(s.Width, 0),
            new Point2f(s.Width, s.Height), new Point2f(0, s.Height) };
        // revolving rect
        Point2f[] newRect = new Point2f[] {
            new Point2f(rect[0].X, rect[0].Y), new Point2f(rect[1].X, rect[1].Y),
            new Point2f(rect[2].X, rect[2].Y), new Point2f(rect[3].X, rect[3].Y) };
        // remapping to have a better time replaning
        int top = Mathf.FloorToInt(Mathf.Min(newRect[0].Y, newRect[1].Y, newRect[2].Y, newRect[3].Y));
        int bottom = Mathf.CeilToInt(Mathf.Max(newRect[0].Y, newRect[1].Y, newRect[2].Y, newRect[3].Y));
        int left = Mathf.FloorToInt(Mathf.Min(newRect[0].X, newRect[1].X, newRect[2].X, newRect[3].X));
        int right = Mathf.CeilToInt(Mathf.Max(newRect[0].X, newRect[1].X, newRect[2].X, newRect[3].X));

        Mat subCard = card.SubMat(top, bottom, left, right);
        
        for (int i = 0; i < newRect.Length; ++i)
        {
            newRect[i].Y -= top;
            newRect[i].X -= left;
        }

        int currentRot = 0;
        int bestRot = 0;
        float bestMatch = 0.0f;
        while (currentRot < 4) {

            using (Mat perspMat = Cv2.GetPerspectiveTransform(newRect, destRect))
            using (Mat warped = new Mat()) 
            {
                Cv2.WarpPerspective(subCard, warped, perspMat, template.Size());
                float diffScore = GetDiffAndHistMatchScore(warped, template, 0.1f);
                if (diffScore > bestMatch)
                {
                    //print("Got in with" + diffScore);
                    bestMatch = diffScore;
                    bestRot = currentRot;
                }
                // ROTATE
                newRect = RotateWinding(newRect, 1);
                currentRot++;
            }
        }
        subCard.Release();
        rotCount = bestRot;
        return bestMatch;
    }
    
    // this is trying to find the tag, it was not to be.... even tho it would have nailed the image
    private Point2f[] GetBestParallelMatchToCorner(Point2f[] corner, Point2f[][] shapes)
    {
        foreach (Point2f[] shape in shapes)
        {
            for (int i = 0; i < 3; ++i)
            {
                float[] line = GetLineFromPoints(corner[i], corner[(i + 1) % 4]);
                List<int> foundPoints = new List<int>();
                for (int pt = 0; pt < shape.Length; ++pt)
                {
                    if (GetDistanceFromLine(line, shape[pt]) < tagLineIntersectThresh) {
                        foundPoints.Add(pt);
                    }
                }
                if (foundPoints.Count != 2)
                    continue;

                line = GetLineFromPoints(corner[(i + 2) % 4], corner[(i + 3) % 4]);
                for (int pt = 0; pt < shape.Length; ++pt)
                {
                    if (GetDistanceFromLine(line, shape[pt]) < tagLineIntersectThresh && !foundPoints.Contains(pt))
                    {
                        foundPoints.Add(pt);
                    }
                }
                if (foundPoints.Count == 4)
                {
                    return shape;
                }
            }
        }
        return null;
    }

    /***********************************************************************************************************************
     * *********************************************************************************************************************
     * *********************************************************************************************************************/
     // Line functions

    private float GetIntersectDot(Point2f[] line1, Point2f[] line2)
    {
        Point2f vec1 = line1[0] - line1[1];
        Point2f vec2 = line2[0] - line2[1];

        float norm1 = (float)vec1.DistanceTo(new Point2f(0, 0));
        vec1.X /= norm1;
        vec1.Y /= norm1;
        float norm2 = (float)vec2.DistanceTo(new Point2f(0, 0));
        vec2.X /= norm2;
        vec2.Y /= norm2;

        return ((float)vec1.DotProduct(vec2));
    }



    private float[] GetLineFromPoints(Point2f first, Point2f second)
    {
        float[] abc = new float[3];
        abc[0] = (first.Y - second.Y) + 0.000001f;
        abc[1] = (second.X - first.X) + 0.000001f;
        abc[2] = -(abc[0] * first.X) - (abc[1] * first.Y);
        return abc;
    }

    private Point2f GetIntersectionPoint(float[] line1, float[] line2)
    {
        Point2f intersect = new Point2f();
        float a = line1[0] / -line1[1];
        float b = line2[0] / -line2[1];
        float c = line1[2] / -line1[1];
        float d = line2[2] / -line2[1];
        
        // if denom == 0, they are either parallel or axis aligned
        // this happens because vertical lines are not actually functions?
        if (Mathf.Abs(line1[0]) < 0.001f && Mathf.Abs(line2[1]) < 0.001f)
        {
            intersect.Y = line2[2] / line2[0];
            intersect.X = line1[2] / line1[1];
            return intersect;
        }
        else if (Mathf.Abs(line1[1]) < 0.001f && Mathf.Abs(line2[0]) < 0.001f)
        {
            intersect.Y = line1[2] / line1[0];
            intersect.X = line2[2] / line2[1];
            return intersect;
        }
        else if (Mathf.Abs((a - b)) < 0.001f)
        {
            Debug.LogError("WHAT YOU DOING? A PARALLEL LINES HAVE GOT NOTHING");
            return new Point2f(Mathf.Infinity, Mathf.Infinity);
        }

        intersect.X =  (d - c) / (a - b);
        intersect.Y = a * ((d - c) / (a - b)) + c;
        return intersect;
    }


    private float GetDistanceFromLine(float[] line, Point2f pt)
    {
        return Mathf.Abs((line[0] * pt.X) + (line[1] * pt.Y) + line[2]) 
            / Mathf.Sqrt((line[0] * line[0]) + (line[1] * line[1]));
    }




    /*************************************************************************************************************
     * ***********************************************************************************************************
     * ***********************************************************************************************************/
     // CONTOUR AND POINT FUNCTIONS:

    private Point2f[][] SimplifyContours(Point[][] contours, int sizeReq = -1, float ratio = 0.04f)
    {
        List<Point2f[]> contourList = new List<Point2f[]>();

        foreach (Point[] cont in contours)
        {
            Point[] simp = Cv2.ApproxPolyDP(cont, ratio * Cv2.ArcLength(cont, true), true);
            if (sizeReq <= -1 || simp.Length == sizeReq)
            {
                Point2f[] simpF = new Point2f[simp.Length];
                for (int i = 0; i < simp.Length; ++i)
                    simpF[i] = simp[i];
                print(simpF);
                contourList.Add(simpF);
            }
        }
        return contourList.ToArray();
    }

    private Point[][] SimplifyContoursPP(Point[][] contours, int sizeReq = -1, float ratio = 0.04f)
    {
        List<Point[]> contourList = new List<Point[]>();

        foreach (Point[] cont in contours)
        {
            Point[] simp = Cv2.ApproxPolyDP(cont, ratio * Cv2.ArcLength(cont, true), true);
            if (sizeReq <= -1 || simp.Length == sizeReq)
            {
                Point[] simpF = new Point[simp.Length];
                for (int i = 0; i < simp.Length; ++i)
                    simpF[i] = simp[i];
                print(simpF);
                contourList.Add(simpF);
            }
        }
        return contourList.ToArray();
    }
    
    private void DetectContours(Mat scene, out Point2f[][] contours, out HierarchyIndex[] hierarchy)
    {
        using (Mat greyCard = new Mat())
        using (Mat edgeCard = new Mat())
        {
            hierarchy = null;

            Dictionary arucoDict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
            DetectorParameters arucoParams = DetectorParameters.Create();
            arucoParams.CornerRefinementMinAccuracy = 0.000001;
            arucoParams.CornerRefinementMaxIterations = 500;
            arucoParams.DoCornerRefinement = true;
            arucoParams.AdaptiveThreshWinSizeMin = 3; // TODO
            arucoParams.AdaptiveThreshWinSizeMax = 23;
            arucoParams.AdaptiveThreshWinSizeStep = 5;

            Point2f[][] corners;
            int[] ids;
            Mat testMat = new Mat();
            Cv2.BitwiseNot(scene, testMat);
            CvAruco.DetectMarkers(testMat, arucoDict, out corners, out ids, arucoParams, out contours);
            contours = contours.Concat(corners).ToArray();
        }
    }

    private Point[][] ConvertPoint2fToPoint(Point2f[][] cvt)
    {
        Point[][] newCvt = new Point[cvt.Length][];
        for (int i = 0; i < cvt.Length; ++i)
        {
            newCvt[i] = new Point[cvt[i].Length];
            for (int k = 0; k < cvt[i].Length; ++k)
            {
                newCvt[i][k] = cvt[i][k];
            }
        }
        return newCvt;
    }

    /*************************************************************************************************************
     * ***********************************************************************************************************
     * ***********************************************************************************************************/


    private int GetCardIDFromShapes(Mat im)
    {
        using (Mat grey = new Mat())
        {
            Cv2.CvtColor(im, grey, ColorConversionCodes.BGR2GRAY);
            //Cv2.GaussianBlur(grey, grey, new Size(3, 3), 0);
            Cv2.MinMaxLoc(grey, out double minVal, out double maxVal);

            Cv2.Threshold(grey, grey, minVal + 30, 255, ThresholdTypes.BinaryInv);
            //Cv2.Erode(grey, grey, new Mat());
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(grey, out contours, out hierarchy, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
            Point2f[][] simpCnts = SimplifyContours(contours, -1, 0.1f);
            Mat colorMat = new Mat();
            im.CopyTo(colorMat);
            Cv2.DrawContours(colorMat, ConvertPoint2fToPoint(simpCnts), -1, Scalar.Blue, 2);

            int val = 0;
            foreach (Point2f[] cnt in simpCnts)
            {
                val += cnt.Length;
            }
            return val;
        }
    }


    private CardCorner[] FindBestLowerRightCardCorner(Mat scene, ref Point2f[][] contours)
    {
        List<CardCorner> rectList = new List<CardCorner>();
        foreach (Point2f[] rect in contours)
        {
            CardCorner bestCorner = new CardCorner();
            bestCorner.matchVal = 0;

            foreach (CardType typeKey in cardTypeDict.Keys) {

                cardTypeDict.TryGetValue(typeKey, out CardTypeTemplateData data);
                int rotCount = 0;
                float matchByShape = AttemptToMatchByShape(scene, data.typeImage, rect);
                float matchToDiff = AttemptToMatchCardByTemplate(scene, data.typeImage, rect, out rotCount);

                if ((matchByShape * 0.1f) + (matchToDiff * 0.9f) > bestCorner.matchVal) {
                    // if we have no upper left partner, we aren't a lower right.
                    Point2f[] rotRect = RotateWinding(rect, rotCount);
                    if (FindBestUpperLeftCardCorner(scene, rotRect, ref contours) == null)
                        continue;

                    bestCorner = new CardCorner()
                    {
                        corners = rect,
                        matchVal = (matchByShape * 0.1f) + (matchToDiff * 0.9f),
                        neededRot = rotCount,
                        mostLikelyType = typeKey
                    };
                }
            }

            if (bestCorner.matchVal > matchThresh)
            {
                rectList.Add(bestCorner);
            }
        }


        rectList.Sort((k1, k2) => k2.matchVal.CompareTo(k1.matchVal));
        return rectList.ToArray();
    }

    // helper function for replaned square box, allows you to get the upper left corner
    private int FindMinSum(Point2f[] shape)
    {
        float minSum = float.PositiveInfinity;
        int best = -1;
        for (int i = 0; i < shape.Length; ++i)
        {
            if (shape[i].X + shape[i].Y < minSum)
            {
                best = i;
                minSum = shape[i].X + shape[i].Y;
            }
        }
        return best;
    }

    // RETURNS A WINDING ORDER OR -1 for the canidate.
    private int TryToReorientUpperLeft(Point2f[] canid, Point2f[] warpedOG, Mat persp)
    {
        Point2f[] warpedCanid = Cv2.PerspectiveTransform(canid, persp);

        // TODO : an alternative way could be to check if each is a line along axis
            // best alt is to check for 90ish degree angles
        Point2f[] line1 = new Point2f[] { warpedCanid[0], warpedCanid[1] };
        Point2f[] line2 = new Point2f[] { warpedCanid[2], warpedCanid[3] };
        float intersectDot = GetIntersectDot(line1, line2);
        if (intersectDot > -0.97f && intersectDot < 0.97f)
            return -1;

        line1 = new Point2f[] { warpedCanid[1], warpedCanid[2] };
        line2 = new Point2f[] { warpedCanid[3], warpedCanid[0] };
        intersectDot = GetIntersectDot(line1, line2);
        if (intersectDot > -0.97f && intersectDot < 0.97f)
            return -1;

        int minCorner = FindMinSum(warpedCanid);
        return (4 - minCorner) % 4;
    }

    private Point2f[] RotateWinding(Point2f[] rect, int rotCount)
    {
        Point2f[] newRect = new Point2f[rect.Length];
        rect.CopyTo(newRect, 0);

        while (rotCount != 0)
        {
            // ROTATE
            Point2f nowFirst = newRect[3];
            newRect[3] = newRect[2];
            newRect[2] = newRect[1];
            newRect[1] = newRect[0];
            newRect[0] = nowFirst;
            rotCount--;
        }
        return newRect;
    }

    private float CompareAreaRatio(float expected, float area)
    {
        float ratio = expected / area;
        ratio = (ratio < 1.0f) ? 1.0f / ratio : ratio;
        return ratio;
    }

    // Get the most likely upper left corner from lower right corner
    private CardCorner FindBestUpperLeftCardCorner(Mat scene, Point2f[] lowerRight, ref Point2f[][] contours)
    {
        Point2f[] dest = bottomRightBoundingBox.GetCWWindingOrder();
        Point2f upperLeftCenter = upperLeftBoundingBox.GetCenter();

        using (Mat persp = Cv2.GetPerspectiveTransform(lowerRight, dest))
        {
            Point2f[] warpedLR = Cv2.PerspectiveTransform(lowerRight, persp);
            float expectedArea = (float)Cv2.ContourArea(warpedLR);

            int bestRotCount = 0;
            float bestRatio = 3.0f;
            Point2f[] bestTopLeft = null;
            float bestAreaRatio = 0;
            float bestLocalAreaRatio = 3;
            foreach (Point2f[] canid in contours)
            {
                // filter nepotism
                if (canid == lowerRight) // shouldn't happen, mathematically impossible
                    continue;

                // filter for not rectangle in replane space (parallel opposite sides), also get the rot count to reorient the square
                int rotCount = TryToReorientUpperLeft(canid, warpedLR, persp);
                if (rotCount == -1)
                    continue;

                // filter not inside expected point
                Point2f[] warpedUL = Cv2.PerspectiveTransform(canid, persp);
                double inExpected = Cv2.PointPolygonTest(warpedUL, upperLeftCenter, false);
                if (inExpected < 0.9f)
                    continue;

                // filter for ratio and area
                float aspectRatio = ComputeApparentRatio(warpedUL);
                float area = (float)Cv2.ContourArea(warpedUL);
                float areaRatio = CompareAreaRatio(expectedArea, area);
                bestLocalAreaRatio = Mathf.Min(bestLocalAreaRatio, areaRatio);
                float aspectWeight = 0.5f;
                // one is a perfect score, anything greater than 1 is worse
                if (((aspectRatio * aspectWeight) + (areaRatio * (1.0f - aspectWeight))) > bestRatio)
                {
                    continue;
                }

                bestAreaRatio = areaRatio;
                
                bestRatio = (aspectRatio * aspectWeight) + (areaRatio * (1.0f - aspectWeight));
                bestRotCount = rotCount;
                bestTopLeft = canid;
            }

            if (bestTopLeft == null)
                return null;
            
            return new CardCorner { corners = bestTopLeft, matchVal = bestRatio, neededRot = bestRotCount };
        }
    }

    // return CW wound card corners using the upper left and lower right corner squares
    private Point2f[] TryToBoundCardFromCorners(Mat scene, Point2f[] lowerRight, Point2f[] upperLeft)
    {
        if (lowerRight == null || upperLeft == null)
            return null;

        float[] topLine = GetLineFromPoints(upperLeft[0], upperLeft[1]);
        float[] rightLine = GetLineFromPoints(lowerRight[1], lowerRight[2]);
        float[] bottomLine = GetLineFromPoints(lowerRight[2], lowerRight[3]);
        float[] leftLine = GetLineFromPoints(upperLeft[3], upperLeft[0]);

        Point2f topLeftCardPt = upperLeft[0];
        Point2f topRightCardPt = GetIntersectionPoint(topLine, rightLine);
        Point2f bottomRightCardPt = lowerRight[2];
        Point2f bottomLeftCardPt = GetIntersectionPoint(bottomLine, leftLine);

        return new Point2f[]
        {
            topLeftCardPt,
            topRightCardPt,
            bottomRightCardPt,
            bottomLeftCardPt
        };
    }

    private Mat PerformFirstReplaneFull(Mat scene, Point2f[] possibleCard, float off, out Mat persp)
    {
        Point2f[] mainCorners = new Point2f[]
        {
                new Point2f(off, off),
                new Point2f(defaultCardWidth - off, off),
                new Point2f(defaultCardWidth - off, defaultCardHeight - off),
                new Point2f(off, defaultCardHeight - off)
        };

        persp = Cv2.GetPerspectiveTransform(possibleCard, mainCorners); 
        Mat replaned = new Mat();
        Size warpSize = new Size(defaultCardWidth, defaultCardHeight);
        Cv2.WarpPerspective(scene, replaned, persp, warpSize);
        return replaned;
    }


    private Mat lastGoodReplane = new Mat();
    public Mat GetLastGoodReplane()
    {
        // return null; // TODO : for debugginh
        return lastGoodReplane;
    }


    /***
     *  THE BIG FUNCTION, MANAGES STATE AND PARSING!!!!
     */
    public CustomCard ParseCard(Mat cardScene, CustomCard previousCard)
    {
        /* DEBUG
        if (mainSeeImage.texture != null)
            Destroy(mainSeeImage.texture);
        mainSeeImage.texture = OpenCvSharp.Unity.MatToTexture(cardScene);
        */

        print("ACTUALLY PARSING CARD");
        Point2f[][] contours;
        HierarchyIndex[] h;
        // DETECT CONTOURS AND SIMPLIFY THEM IF NEEDED
        DetectContours(cardScene, out contours, out h);
        Mat blackout = new Mat();
        cardScene.CopyTo(blackout);
        CvAruco.DrawDetectedMarkers(blackout, contours, null);

        cardParserManager.UpdateSeenImage(blackout);

        // POSSIBLE LOWER RIGHTS
        CardCorner[] bestLowerRights = FindBestLowerRightCardCorner(cardScene, ref contours);

        Mat blackOut = new Mat();
        cardScene.CopyTo(blackOut);

        foreach (CardCorner bestLowerRight in bestLowerRights)
        {
            // CLEAN UP THE CURRENT LOWER RIGHT
            if (bestLowerRight == null) break;
            bestLowerRight.corners = RotateWinding(bestLowerRight.corners, bestLowerRight.neededRot);
            bestLowerRight.neededRot = 0;
            
            // UPPER LEFT
            CardCorner bestUpperLeft = FindBestUpperLeftCardCorner(cardScene, bestLowerRight.corners, ref contours);
            if (bestUpperLeft == null)
            {
                print("Failed on upper left, which might be needed");
                return null;
            }
            
            bestUpperLeft.corners = RotateWinding(bestUpperLeft.corners, bestUpperLeft.neededRot);
            bestUpperLeft.neededRot = 0;

            // REMAP BY CORNERS
            Point2f[] possibleCard = TryToBoundCardFromCorners(cardScene, bestLowerRight.corners, bestUpperLeft.corners);
            if (possibleCard == null)
                return null;
            Mat replaned = PerformFirstReplaneFull(cardScene, possibleCard, cornerReplaneOffset, out Mat firstTMat);

            // predict most likely element from single replane, might not work but may improve performance.
            bestLowerRight.mostLikelyElement = GetMostLikelyElement(replaned, cornerReplaneOffset);

            // get the homography matrix from the replaned image to the template image space
            Mat hMat = KeypointMatchToTemplate(replaned, bestLowerRight, out CardType cardType, out CardElement cardElement, out int ID, out string cardName);
            if (hMat == null) return null;
            // REMINDER! THE HOMOGRAPHY MATRIX RETURNED HAS A BORDER ATTACHED TO IT!
            Cv2.WarpPerspective(cardScene, replaned, hMat * firstTMat, new Size(defaultCardPlusBorderWidth, defaultCardPlusBorderHeight));

            
            // the default used for keypoint matching has a border so that we can get better keypoints at the border of cards (since it doesn't use image edge in its feature finding)
            // if we replaned the image perfectly, we will have a matchBorder sized border
            Mat croppedReplaned = replaned[borderAmount, replaned.Height - borderAmount, borderAmount, replaned.Width - borderAmount];

            // Get an affine matrix representation of the crop, TODO : very much might not be good
            Point2f oldTopLeft = new Point2f(borderAmount, borderAmount);
            Point2f oldBottomRight = new Point2f(replaned.Width, replaned.Height);
            Point2f newTopLeft = new Point2f(0, 0);
            Point2f newBottomRight = new Point2f(replaned.Width - borderAmount, replaned.Height - borderAmount);
            Point2f[] oldCorners = new Point2f[] { oldTopLeft, new Point2f(oldBottomRight.X, oldTopLeft.Y), oldBottomRight, new Point2f(oldTopLeft.X, oldBottomRight.Y) };
            Point2f[] newCorners = new Point2f[] { newTopLeft, new Point2f(newBottomRight.X, newTopLeft.Y), newBottomRight, new Point2f(newTopLeft.X, newBottomRight.Y) };

            Mat affineRepOfCrop = Cv2.GetAffineTransform(oldCorners, newCorners);

            croppedReplaned.CopyTo(lastGoodReplane);
            
            replaned.Release();
            replaned.Dispose();

            // TODO : bound the bounding box less strictly and perhaps axis aligned?
            // TODO : ID function, by array or similar? : add to card template data
            return new CustomCard(1.0f, new BoundingBox(possibleCard[0], possibleCard[1], possibleCard[2], possibleCard[3]),
                cardType, cardElement, ID, cardName, affineRepOfCrop * hMat * firstTMat);
            // TODO : function to reaquire from old BB, 
        }
        return null;
    }

    /**
    *  Given expected card type and element, run thru the most likely cards and try to get the best keypoint matches
    *  
    *  TODO: better homo filtering, better foreach filtering
    */
   private Mat KeypointMatchToTemplate(Mat replaned, CardCorner bestLowerRight, 
       out CardType cardType, out CardElement cardElement, out int ID, out string cardName)
   {
        float t = Time.realtimeSinceStartup;
       // GET KEYPOINTS FOR THE REPLANED IMAGE
       GetKeypoints(replaned, out KeyPoint[] kp2, out Mat des2);
        print("Keypoints took: " + (Time.realtimeSinceStartup - t));
        // ITERATE THRU THE MOST LIKELY CARDS
        bool hasList = templateCardDict.TryGetValue(
            (int)bestLowerRight.mostLikelyType, 
            out List<TemplateCardData> cardDataList);



        int bestGoodMatches = 0;
        TemplateCardData bestCardData = null;
        Mat bestHomographyMat = null;
        print("Card List is " + cardDataList.Count);
        float bestPercent = 0.0f;
        float bestHistComp = 0.0f;
        string bestHistWinner = "";

        // TODO : possible sort paradigms
            // by fundy number
            // by homo percent
            // by homo number
        foreach (TemplateCardData cardData in cardDataList)
        {
            // TODO : debugging with histograms, fruitless due to sh*t lighting
            // TODO : see about tagging 'common' keypoints (those effectively shared by all cards of a type, to reduce confusions)
            Mat seenHist = MakeHSHistrogram(replaned);
            float comp = (float)Cv2.CompareHist(seenHist, cardData.hist, HistCompMethods.Correl);
            if (comp > bestHistComp)
            {
                bestHistComp = comp;
                bestHistWinner = cardData.name;
            }

            print("Getting card data for " + cardData.name + " from " + cardDataList.Count);
            KeyPoint[] kp1 = cardData.keypoints;
            Mat des1 = cardData.des;
            MatchKeypoints(kp1, kp2, des1, des2, out DMatch[] goodMatches);

            GetMatchedKeypoints(kp1, kp2, goodMatches, out Point2f[] m_kp1, out Point2f[] m_kp2);
            int initMatches = goodMatches.Length;
            if (FilterByFundy(ref m_kp1, ref m_kp2, ref goodMatches, 3.0f))
            {
                if (CheckIfEnoughMatch(goodMatches, initMatches))
                //if (CheckIfEnoughMatch(goodMatches, initMatches) && bestGoodMatches < goodMatches.Length) // discrete count version
                {
                    Mat homo = GetHomographyMatrix(m_kp2, m_kp1);
                    
                    if (homo != null && homo.Width == 3 && homo.Height == 3 && IsGoodHomography(homo, m_kp1, m_kp2, out float percent))
                    {
                        int survivedKP = Mathf.RoundToInt(m_kp1.Length * percent);
                        if (survivedKP > bestGoodMatches)
                        {
                            bestHomographyMat = homo;
                            //bestGoodMatches = goodMatches.Length;
                            bestGoodMatches = survivedKP;
                            bestCardData = cardData;
                            bestPercent = percent;
                        }
                    }
                }
            }
        }

        print("Best hist comp was " + bestHistWinner + " with " + bestHistComp);
        print("The entire templating took " + (Time.realtimeSinceStartup - t) + " seconds");
        if (bestCardData != null)
            print("Predicted by keys: " + bestGoodMatches + " for " + bestCardData.name + " with " + bestPercent);

        if (bestCardData != null)
        {
            cardType = bestCardData.cardType;
            cardElement = bestCardData.cardElement;
            ID = bestCardData.ID;
            cardName = bestCardData.name;
        } else
        {
            cardType = CardType.None;
            cardElement = CardElement.None;
            ID = -1;
            cardName = "None";

        }

        return bestHomographyMat;
   }


    // TODO 
    private float homoDist = 4.0f;
    private float percentAccepted = 0.7f;
    private bool IsGoodHomography(Mat homo, Point2f[] m_kp1, Point2f[] m_kp2, out float percent)
    {
        float totalKP = m_kp1.Length;
        float goodKP = 0.0f;
        Point2f[] warped_kp2 = Cv2.PerspectiveTransform(m_kp2, homo);
        for (int i = 0; i < m_kp2.Length; ++i)
        {
            if (warped_kp2[i].DistanceTo(m_kp1[i]) < homoDist)
                goodKP += 1.0f;
        }
        percent = (goodKP / totalKP);
        return (goodKP / totalKP) > percentAccepted;

    }


    /**
     * From the estimated mean values, get the closest average element and say that that is probably the element.
     * TODO : may not work on bad cameras that output too much red. 
     * Being color agnostic is an ideal we probably cannot meet...
     */
    private CardElement GetMostLikelyElement(Mat replaned, int cornerReplaneOffset)
    {
        Mat area = replaned[cornerReplaneOffset, replaned.Height - cornerReplaneOffset, cornerReplaneOffset, replaned.Width - cornerReplaneOffset];

        using (Mat crop = elementColorBoundingBox.CropByBox(area))
        {

            
            Scalar rMean = crop.Mean();
            //print("Response: " + (float)rMean.Val0 + ", " + (float)rMean.Val1 + ", " + (float)rMean.Val2);
            Color.RGBToHSV(new Color((float)rMean.Val2, (float)rMean.Val1, (float)rMean.Val0),
                out float rH, out float rS, out float rV);

            float bestDist = Mathf.Infinity;
            CardElement bestElement = CardElement.Dark;

            foreach (CardElement element in cardElementDict.Keys)
            {
                Scalar tMean = cardElementDict[element].typeScalar;
                //print("Template for " + element + ": " + (float)tMean.Val0 + ", " + (float)tMean.Val1 + ", " + (float)tMean.Val2);
                Color.RGBToHSV(new Color((float)tMean.Val2, (float)tMean.Val1, (float)tMean.Val0),
                    out float tH, out float tS, out float tV);

                float dist = Mathf.Sqrt((float)
                    ((tH - rH) * (tH - rH) +
                    (tS - rS) * (tS - rS)));

                print("Response Dist for " + element + ": " + dist);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestElement = element;
                }
            }
            print("Winner is " + bestElement);
            return bestElement;
        }
    }




    /************************************************************************************************************************************
     * **********************************************************************************************************************************
     * **********************************************************************************************************************************/
    // KEYPOINT LOGICS

    public void GetKeypoints(Mat im, out KeyPoint[] keypoints, out Mat des)
    {
        des = new Mat();
        using (var gray = im.CvtColor(ColorConversionCodes.BGR2GRAY))
        using (var surf = SIFT.Create())
        {
            surf.DetectAndCompute(gray, null, out keypoints, des);
        }
    }

    [Space(1)]
    public float loweRatio = 0.75f;
    public float kpDist;

    /**
    Match the keypoints of image 1 and 2 using BF batcher and the ratio test.
    Returns a list of matches which passed the ratio test.
    */
    public void MatchKeypoints(KeyPoint[] kp1, KeyPoint[] kp2, Mat des1, Mat des2, out DMatch[] goodMatches)
    {
        if (des1.Size().Width == 0 || des1.Size().Height == 0 || des2.Size().Width == 0 || des2.Size().Height == 0)
        {
            goodMatches = new DMatch[0];
            return;
        }

        using (var bf = new BFMatcher(NormTypes.L2))
        {
            DMatch[][] matches = bf.KnnMatch(des1, des2, 2);
            List<DMatch> goodMatchesList = new List<DMatch>();
            foreach (DMatch[] m_n in matches)
            {
                if (m_n.Length < 2)
                {
                    continue;
                }

                if (m_n[0].Distance < loweRatio * m_n[1].Distance)
                {
                    float dist0 = (float)kp1[m_n[0].QueryIdx].Pt.DistanceTo(kp2[m_n[0].TrainIdx].Pt);
                    if (dist0 < kpDist)
                        goodMatchesList.Add(m_n[0]);
                }
                else
                {
                    float dist0 = (float)kp1[m_n[0].QueryIdx].Pt.DistanceTo(kp2[m_n[0].TrainIdx].Pt);
                    float dist1 = (float)kp1[m_n[1].QueryIdx].Pt.DistanceTo(kp2[m_n[1].TrainIdx].Pt);
                    if (dist0 < dist1 && dist0 < kpDist && !goodMatchesList.Contains(m_n[1]))
                        goodMatchesList.Add(m_n[0]);
                }

            }
            goodMatches = goodMatchesList.ToArray();
        }
    }

    /**
    Match the keypoints of image 1 and 2 using BF batcher and the ratio test.
    The current ratio is 0.7.
    Returns a list of matches which passed the ratio test.
    */
    public void MatchKeypointsBoring(KeyPoint[] kp1, KeyPoint[] kp2, Mat des1, Mat des2, out DMatch[] goodMatches)
    {
        if (des1.Size().Width == 0 || des1.Size().Height == 0 || des2.Size().Width == 0 || des2.Size().Height == 0)
        {
            goodMatches = new DMatch[0];
            return;
        }

        using (var bf = new BFMatcher(NormTypes.L2))
        {
            DMatch[][] matches = bf.KnnMatch(des1, des2, 2);
            List<DMatch> goodMatchesList = new List<DMatch>();
            foreach (DMatch[] m_n in matches)
            {
                if (m_n.Length != 2)
                {
                    Debug.LogError("ERROR: Matching is being mean.");
                    continue;
                }
                if (m_n[0].Distance < loweRatio * m_n[1].Distance)
                {
                     goodMatchesList.Add(m_n[0]);
                }
            }
            goodMatches = goodMatchesList.ToArray();
        }
    }

    public int keypointMatchesRequired = 20;
    public float keypointMatchPercentRequired = 0.6f;
    public bool CheckIfEnoughMatch(DMatch[] matches, int initialMatchCount)
    {
        return matches.Length >= keypointMatchesRequired
               && matches.Length > initialMatchCount * keypointMatchPercentRequired;
    }


    private Point2d[] ConvertFromF(Point2f[] f)
    {
        Point2d[] r = new Point2d[f.Length];
        for (int i = 0; i < r.Length; ++i)
        {
            r[i] = new Point2d(f[i].X, f[i].Y);
        }
        return r;
    }
    private bool FilterByFundy(ref Point2f[] kp1_pt, ref Point2f[] kp2_pt, ref DMatch[] matches, double dist = 3)
    {
        if (kp1_pt.Length <= 8) return false;

        Mat mask = new Mat(new int[] { kp1_pt.Length }, MatType.CV_8UC1);
        Mat fundy = Cv2.FindFundamentalMat(ConvertFromF(kp1_pt), ConvertFromF(kp2_pt), FundamentalMatMethod.Ransac, 1.0f, 0.9f, mask);

        List<Point2f> n_kp1_pt = new List<Point2f>();
        List<Point2f> n_kp2_pt = new List<Point2f>();
        List<DMatch> newMatches = new List<DMatch>();
        // TODO : very inefficient
        for (int i = 0; i < kp1_pt.Length; ++i)
        {
            if (mask.Get<bool>(i))
            {
                n_kp1_pt.Add(kp1_pt[i]);
                n_kp2_pt.Add(kp2_pt[i]);
                newMatches.Add(matches[i]);
            }
        }
        kp1_pt = n_kp1_pt.ToArray();
        kp2_pt = n_kp2_pt.ToArray();
        print(matches.Length + " vs. new " + newMatches.Count);
        matches = newMatches.ToArray();
        print("Then: " + Time.realtimeSinceStartupAsDouble);

        return true;
    }
    
    private Mat GetHomographyMatrix(Point2f[] src, Point2f[] dest)
    {
        Mat homo = Cv2.FindHomography(ConvertFromF(src), ConvertFromF(dest), HomographyMethods.Ransac, 2);
        return homo;
    }

    protected void GetMatchedKeypoints(KeyPoint[] kp1, KeyPoint[] kp2, DMatch[] matches, out Point2f[] m_kp1, out Point2f[] m_kp2)
    {
        m_kp1 = new Point2f[matches.Length];
        m_kp2 = new Point2f[matches.Length];

        for (int i = 0; i < matches.Length; ++i)
        {
            m_kp1[i] = kp1[matches[i].QueryIdx].Pt;
            m_kp2[i] = kp2[matches[i].TrainIdx].Pt;
        }
    }

















    /*******************************************************************************************************
     *******************************************************************************************************
     *******************************************************************************************************/

    public enum CardElement
    {
        None = 1,
        Fire = 2, 
        Water = 4, 
        Wind = 8,
        Earth = 16,
        Light = 32,
        Dark = 64
    }

    public enum CardType
    {
        None = 128,
        Attack = 256,
        Defense = 512,
        Influence = 1024
    }

    public class CardCorner
    {
        public float matchVal;
        public Point2f[] corners; // CW
        public int neededRot;
        public CardType mostLikelyType;
        public CardElement mostLikelyElement;
    }

    public class TemplateCardData
    {
        public TemplateCardData(Mat hist, KeyPoint[] k, Mat d, int id, string name, CardType type, CardElement element)
        {
            this.hist = hist;
            keypoints = k;
            des = d;
            ID = id;
            this.name = name;
            cardType = type;
            cardElement = element;
        }

        ~TemplateCardData()
        {
            des.Dispose();
            des.Release();
        }

        public Mat hist;
        public KeyPoint[] keypoints;
        public Mat des;
        public int ID;
        public string name;
        public CardType cardType;
        public CardElement cardElement;
    }

    public class CardTypeTemplateData
    {
        public CardTypeTemplateData(Mat ti)
        {
            typeImage = ti;
        }

        ~CardTypeTemplateData()
        {
            typeImage.Release();
            typeImage.Dispose();
        }
        public Mat typeImage;
    }

    public class CardElementTemplateData
    {
        public CardElementTemplateData(Scalar ti)
        {
            typeColor = new Color((float)ti.Val0, (float)ti.Val1, (float)ti.Val2, (float)ti.Val3);
            typeScalar = ti;
        }
        public Color typeColor;
        public Scalar typeScalar;
    }

    public class StickerTemplateData
    {
        public StickerTemplateData(Mat binImg, Scalar backCol, Scalar iconCol)
        {
            binImage = binImg;
            backColor = backCol;
            iconColor = iconCol;
        }

        public Mat binImage;
        public Scalar backColor;
        public Scalar iconColor; // TODO : should just be white?
    }

    /**
     * Normalized bounding box (0 to 1 for width and height), users must convert to pixel space if needed.
     * Bounding box manages all corners so can be rotated.
     * DOES NOT CHECK IF YOU ACTUALLY GIVE IT A BOX!!!
     */
    [System.Serializable]
    public class BoundingBox
    {
        public Point2f ul, ur, lr, ll;

        public BoundingBox(Point2f ul, Point2f lr)
        {
            this.ul = ul;
            this.lr = lr;
            this.ll = new Point2f(lr.X, ul.Y);
            this.ur = new Point2f(ul.X, lr.Y);
        }

        public BoundingBox(Point2f ul, Point2f ur, Point2f lr, Point2f ll)
        {
            this.ul = ul;
            this.ur = ur;
            this.lr = lr;
            this.ll = ll;
        }

        /**
         * Get the normalized size of the bounding box
         */
        public Point2f GetRelativeSize()
        {
            return new Point2f((float)Point2f.Distance(ul, ur), (float)Point2f.Distance(ul, ll));
        }

        public Point2f[] GetAABB(Size imSize)
        {
            return new Point2f[]
            {
                new Point2f(Mathf.Min(ul.X, ur.X, ll.X, lr.X) * imSize.Width, Mathf.Min(ul.Y, ur.Y, ll.Y, lr.Y) * imSize.Height),
                new Point2f(Mathf.Max(ul.X, ur.X, ll.X, lr.X) * imSize.Width, Mathf.Max(ul.Y, ur.Y, ll.Y, lr.Y) * imSize.Height)
            };
        }

        public OpenCvSharp.Rect GetAABBRect(Size size)
        {
            Point2f[] aabb = GetAABB(size);

            int top = Mathf.Clamp(Mathf.RoundToInt(aabb[0].Y), 0, size.Height);
            int bottom = Mathf.Clamp(Mathf.RoundToInt(aabb[1].Y), 0, size.Height);
            int left = Mathf.Clamp(Mathf.RoundToInt(aabb[0].X), 0, size.Width);
            int right = Mathf.Clamp(Mathf.RoundToInt(aabb[1].X), 0, size.Width);

            return new OpenCvSharp.Rect(left, top, right - left, bottom - top);
        }
        // Returns a perspective matrix 
        // !!!!! assuming you've cropped the image by pixelAABB !!!!!
        public Mat GetPerspectiveMatrixWithCrop(Size imageSize, out Point2f[] pixelAABB)
        {
            pixelAABB = GetAABB(imageSize);

            Point2f relSize = GetRelativeSize();
            // goes ul, ur, lr, ll
            Point2f[] newCorners = new Point2f[]
            {
                new Point2f(0,0),
                new Point2f(relSize.X * imageSize.Width, 0),
                new Point2f(relSize.X * imageSize.Width, relSize.Y * imageSize.Height),
                new Point2f(0,relSize.Y * imageSize.Height)
            };

            Point2f[] pixelCorners = new Point2f[]
            {
                new Point2f((ul.X * imageSize.Width) - pixelAABB[0].X, (ul.Y * imageSize.Height) - pixelAABB[0].Y),
                new Point2f((ur.X * imageSize.Width) - pixelAABB[0].X, (ur.Y * imageSize.Height) - pixelAABB[0].Y),
                new Point2f((lr.X * imageSize.Width) - pixelAABB[0].X, (lr.Y * imageSize.Height) - pixelAABB[0].Y),
                new Point2f((ll.X * imageSize.Width) - pixelAABB[0].X, (ll.Y * imageSize.Height) - pixelAABB[0].Y),
            };

            return Cv2.GetPerspectiveTransform(pixelCorners, newCorners);
        }

        public Mat CropByBox(Mat im, Size size)
        {
            float offsetX = (im.Width * ul.X) + 2;
            float offsetY = (im.Height * ul.Y) + 2;
            int sizeX = Mathf.RoundToInt(im.Width * (lr.X - ul.X)) - 4;
            int sizeY = Mathf.RoundToInt(im.Height * (lr.Y - ul.Y)) - 4;

            using (Mat trans_mat = new Mat(2, 3, MatType.CV_32F))
            {
                Mat newIm = new Mat();
                trans_mat.SetArray(0, 0, 1, 0, -offsetX, 0, 1, -offsetY);
                Cv2.WarpAffine(im, newIm, trans_mat, new Size(sizeX, sizeY));
                newIm = newIm.Resize(size);
                return newIm;
            }
        }

        public Mat CropByBox(Mat im)
        {
            float offsetX = (im.Width * ul.X) + 2;
            float offsetY = (im.Height * ul.Y) + 2;
            int sizeX = Mathf.RoundToInt(im.Width * (lr.X - ul.X)) - 4;
            int sizeY = Mathf.RoundToInt(im.Height * (lr.Y - ul.Y)) - 4;

            using (Mat trans_mat = new Mat(2, 3, MatType.CV_32F))
            {
                Mat newIm = new Mat();
                trans_mat.SetArray(0, 0, 1, 0, -offsetX, 0, 1, -offsetY);
                Cv2.WarpAffine(im, newIm, trans_mat, new Size(sizeX, sizeY));


                return newIm;
            }
        }

        public Point2f GetCenter()
        {
            return (ul + lr) * 0.5f;
        }

        // starts from upperleft and goes clockwise
        public Point2f[] GetCWWindingOrder()
        {
            return new Point2f[]
            {
                new Point2f(ul.X, ul.Y),
                new Point2f(lr.X, ul.Y),
                new Point2f(lr.X, lr.Y),
                new Point2f(ul.X, lr.Y)
            };
        }

        public float GetArea(Mat mat)
        {
            float width = Mathf.Abs(mat.Height * ul.Y - mat.Height * lr.Y);
            float height = Mathf.Abs(mat.Width * ul.X - mat.Width * lr.X);
            return width * height;
        }
    }

    public void FillBoundingBoxes(string text)
    {
        string[] s = text.Split('\n');
        if (s.Length != 6) return;

        upperLeftBoundingBox = MakeBoundingBoxFromEditorStr(s[0]);
        print(upperLeftBoundingBox);
        bottomRightBoundingBox = MakeBoundingBoxFromEditorStr(s[1]);
        print(bottomRightBoundingBox);
        elementColorBoundingBox = MakeBoundingBoxFromEditorStr(s[2]);
        print(elementColorBoundingBox);
        stickerBoundingBox1 = MakeBoundingBoxFromEditorStr(s[3]);
        stickerBoundingBox2 = MakeBoundingBoxFromEditorStr(s[4]);
        stickerBoundingBox3 = MakeBoundingBoxFromEditorStr(s[5]);
        print("Success");
    }

    private BoundingBox MakeBoundingBoxFromEditorStr(string v)
    {
        string[] corners = v.Split(',');
        foreach (string c in corners)
            print(c);

        return new BoundingBox(
            new Point2f(float.Parse(corners[0].Trim()), float.Parse(corners[1].Trim())),
            new Point2f(float.Parse(corners[2].Trim()), float.Parse(corners[3].Trim())));
    }

    [TextArea(6, 12)]
    [Tooltip("upper left, bottom right, element, sticker 1, 2, 3")]
    public string boundBoxText;
}

#if UNITY_EDITOR
[CustomEditor(typeof(CardParser))]
public class CardParserEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        CardParser card = (CardParser)target;
        if (GUILayout.Button("Build Boxes"))
        {
            card.FillBoundingBoxes(card.boundBoxText);
        }
    }
}
#endif