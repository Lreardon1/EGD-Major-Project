using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using OpenCvSharp.Aruco;
using OpenCvSharp.XFeatures2D;
using System.Linq;
using System;

public class CardParser : MonoBehaviour
{
    [Header("UI and Debugging")]
    public RawImage matcherImage;
    public RawImage contourImage;
    public RawImage arucoImage;
    public RawImage replaneImage;
    public RawImage[] diffImages;

    public Text numberText;
    
    [Space(10)]
    public Texture2D staticTestImage;
    
    // BOUNDING BOXES
    [Space(10)]
    [Header("Boudning Boxes")]
    public Point2f bottomRightBoundingBox_UL;
    public Point2f bottomRightBoundingBox_LR;
    public BoundingBox bottomRightBoundingBox;
    [Space(1)]
    public Point2f upperLeftBoundingBox_UL;
    public Point2f upperLeftBoundingBox_LR;
    public BoundingBox upperLeftBoundingBox;
    [Space(1)]
    public Point2f bannerNumberBox_UL;
    public Point2f bannerNumberBox_LR;
    public BoundingBox bannerNumberBox;
    [Space]
    public Point2f elementColorBox_UL;
    public Point2f elementColorBox_LR;
    public BoundingBox elementColorBox;

    [Space(10)]
    [Header("Match and Planing Scalars")]
    public int cornerReplaneOffset = 25;
    public float matchThresh = 0.8f;
    public float tagLineIntersectThresh = 4.0f;
    private int defaultCardWidth = 0;
    private int defaultCardHeight = 0;

    [Space(10)]
    [Header("Template Cards")]
    public int borderAmount = 10;
    public float defaultCardResizeAmount = 0.5f;
    public ScriptableCardImage[] cardTemplates;
    private Dictionary<int, List<TemplateCardData>> templateCardDict = new Dictionary<int, List<TemplateCardData>>();
    private Dictionary<CardType, CardTypeTemplateData> cardTypeDict = new Dictionary<CardType, CardTypeTemplateData>();
    private Dictionary<CardElement, CardElementTemplateData> cardElementDict = new Dictionary<CardElement, CardElementTemplateData>();
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



    private void Start()
    {
        bottomRightBoundingBox = new BoundingBox(bottomRightBoundingBox_UL, bottomRightBoundingBox_LR);
        upperLeftBoundingBox = new BoundingBox(upperLeftBoundingBox_UL, upperLeftBoundingBox_LR);
        bannerNumberBox = new BoundingBox(bannerNumberBox_UL, bannerNumberBox_LR);
        
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

    // TODO
    private Mat ExtractCardType(Mat cardMat)
    {
        throw new NotImplementedException();
    }
    private Color ExtractCardElement(Mat cardMat)
    {
        return Color.red;
    }

    /**
     * Bake the template card data for quicker use in detection and parsing.
     * Uses a pretty large amount of memory for great gains in performance.
     * Called on START using provided scriptable objects of cards.
     */
    private void BakeCardTemplateData()
    {
        templateCardDict.Add(0, new List<TemplateCardData>());
        
        foreach (ScriptableCardImage card in cardTemplates)
        {
            defaultCardWidth = card.cardTexture.width;
            defaultCardHeight = card.cardTexture.height;

            // extract keypoints
            int cardType = ConvertToIntMask(card.cardElement);
            int cardElement = ConvertToIntMask(card.cardType);

            using (Mat cardMat = OpenCvSharp.Unity.TextureToMat(card.cardTexture))
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

                // make keypoint data to store : TODO : this can be offloaded to editor for massive increase in exe size
                TemplateCardData cardKeypointData = new TemplateCardData(kp, des);

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
                webCamTexture = new WebCamTexture(webCamDevice.Value.name, 1920, 1080, 10);

                // read device params and make conversion map
                ReadTextureConversionParameters();

                webCamTexture.Play();
                print(webCamTexture.deviceName);
                print(webCamTexture.dimension);
                print(webCamDevice.Value.availableResolutions);
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

    /// <summary>
    /// Default initializer for MonoBehavior sub-classes
    /// </summary>
    protected virtual void Awake()
    {
        if (WebCamTexture.devices.Length > 0)
            DeviceName = WebCamTexture.devices[WebCamTexture.devices.Length - 2].name;
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
        // TODO
        using (Mat cardScene = OpenCvSharp.Unity.TextureToMat(staticTestImage))
        {
            //int f = 3;
            //Cv2.Resize(cardScene, cardScene, new Size(cardScene.Width / f, cardScene.Height / f));
            print(cardScene.Size());

            //Mat cardScene = OpenCvSharp.Unity.TextureToMat(image);
            ParseCard(cardScene);
        }
        return true;
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
        Debug.LogError("ERROR: Shape score outdated from changes, multiple shapes in type possible.");
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

    private void RotateWindingOnRect(Point2f[] newRect)
    {
        Point2f nowFirst = newRect[3];
        newRect[3] = newRect[2];
        newRect[2] = newRect[1];
        newRect[1] = newRect[0];
        newRect[0] = nowFirst;
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
                    print("Got in with" + diffScore);
                    bestMatch = diffScore;
                    bestRot = currentRot;
                }
                // ROTATE
                RotateWindingOnRect(newRect);
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

        /*
        float x_numer = (line1[1] * line2[2]) - (line2[1] * line1[2]);
        float y_numer = (line1[2] * line2[0]) - (line2[2] * line1[1]);
        float denom = (line1[0] * line2[1]) - (line2[1] * line1[0]);
        */
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
        else if (Mathf.Abs((a - b)) < 0.01f)
        {
            throw new System.Exception("WHAT YOU DOING? A PARALLEL LINES HAVE GOT NOTHING");
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
        // BIG TO-DO : more accurate this, always, not matter how accurate you get it.
        using (Mat greyCard = new Mat())
        using (Mat edgeCard = new Mat())
        {
            hierarchy = null;
            /*
            Cv2.CvtColor(scene, greyCard, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(greyCard, greyCard, new Size(5, 5), 1.0f); // crazy expensive
            Cv2.Canny(greyCard, edgeCard, 120, 255);
            Cv2.FindContours(edgeCard, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            */
            Dictionary arucoDict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
            DetectorParameters arucoParams = DetectorParameters.Create();
            arucoParams.CornerRefinementMinAccuracy = 0.000001;
            arucoParams.CornerRefinementMaxIterations = 500;
            arucoParams.DoCornerRefinement = true;
            arucoParams.AdaptiveThreshWinSizeMin = 7; // TODO
            arucoParams.AdaptiveThreshWinSizeMax = 25;
            arucoParams.AdaptiveThreshWinSizeStep = 6;

            Point2f[][] corners;
            int[] ids;
            //Mat testMat = 255 - scene;
            Mat testMat = new Mat();
            Cv2.BitwiseNot(scene, testMat);
            CvAruco.DetectMarkers(testMat, arucoDict, out corners, out ids, arucoParams, out contours);
            contours = contours.Concat(corners).ToArray();

            CvAruco.DrawDetectedMarkers(testMat, contours, null);
            ///matcherImage.texture = OpenCvSharp.Unity.MatToTexture(testMat);
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
            ///diffImages[1].texture = OpenCvSharp.Unity.MatToTexture(grey);
            ///diffImages[2].texture = OpenCvSharp.Unity.MatToTexture(colorMat);

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
        // TODO : THIS FUNCTION NO LONGER REMOVES ANYTHING FROM CONTOURS!!!
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

    // Expects lowerRight to be wound properly!!!
    private CardCorner FindBestUpperLeftCardCorner(Mat scene, Point2f[] lowerRight, ref Point2f[][] contours)
    {
        Point2f[] dest = bottomRightBoundingBox.GetCWWindingOrder();
        Point2f upperLeftCenter = upperLeftBoundingBox.GetCenter();

        using (Mat persp = Cv2.GetPerspectiveTransform(lowerRight, dest))
        {
            Point2f[] warpedLR = Cv2.PerspectiveTransform(lowerRight, persp);
            float expectedArea = (float)Cv2.ContourArea(warpedLR);

            int bestRotCount = 0;
            float bestRatio = Mathf.Infinity;
            Point2f[] bestTopLeft = null;
            float bestAreaRatio = 0;
            float bestLocalAreaRatio = 10;
            foreach (Point2f[] canid in contours)
            {
                // filter nepotism
                if (canid == lowerRight) // shouldn't happen
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
                    continue;

                bestAreaRatio = areaRatio;
                
                bestRatio = (aspectRatio * aspectWeight) + (areaRatio * (1.0f - aspectWeight));
                bestRotCount = rotCount;
                bestTopLeft = canid;
            }

            if (bestTopLeft == null)
                return null;

            // filter out the best from the contour list
            List<Point2f[]> temp = new List<Point2f[]>(contours);
            temp.Remove(bestTopLeft);
            contours = temp.ToArray();
            print("Best area ratio was: " + bestAreaRatio + ", compared to " + bestLocalAreaRatio + " was best locally");
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



    /***
     *  THE BIG FUNCTION, MANAGES STATE AND PARSING!!!!
     *  
     */ 
    public void ParseCard(Mat cardScene)
    {
        Point2f[][] contours;
        HierarchyIndex[] h;
        // DETECT CONTOURS AND SIMPLIFY THEM IF NEEDED
        DetectContours(cardScene, out contours, out h);

        // POSSIBLE LOWER RIGHTS
        CardCorner[] bestLowerRights = FindBestLowerRightCardCorner(cardScene, ref contours);

        foreach (CardCorner bestLowerRight in bestLowerRights)
        {
            // CLEAN UP THE CURRENT LOWER RIGHT
            if (bestLowerRight == null) break; // TODO : handle failure
            print("Desired rot = " + bestLowerRight.neededRot);
            bestLowerRight.corners = RotateWinding(bestLowerRight.corners, bestLowerRight.neededRot);
            bestLowerRight.neededRot = 0;

            // LOWER RIGHT DEBUGGING
            print("Selected corner got out with match val: " + bestLowerRight.matchVal);
            CvAruco.DrawDetectedMarkers(cardScene, new Point2f[][] { bestLowerRight.corners }, null);
            if (arucoImage.texture != null)
                Destroy(arucoImage.texture);
            arucoImage.texture = OpenCvSharp.Unity.MatToTexture(cardScene);

            // UPPER LEFT
            CardCorner bestUpperLeft = FindBestUpperLeftCardCorner(cardScene, bestLowerRight.corners, ref contours);
            if (bestUpperLeft == null)
            {
                print("Failed on upper left, which might be needed");
                return;
            }
            bestUpperLeft.corners = RotateWinding(bestUpperLeft.corners, bestUpperLeft.neededRot);
            bestUpperLeft.neededRot = 0;
            print("Best upper left got out");

            // REMAP BY CORNERS
            Point2f[] possibleCard = TryToBoundCardFromCorners(cardScene, bestLowerRight.corners, bestUpperLeft.corners);
            if (possibleCard == null)
                return;
            Mat replaned = PerformFirstReplaneFull(cardScene, possibleCard, cornerReplaneOffset, out Mat firstTMat);

            // predict most likely element from single replane, might not work but may improve performance.
            bestLowerRight.mostLikelyElement = GetMostLikelyElement(replaned, cornerReplaneOffset);
            // get the homography matrix from the replaned image to the template image space
            Mat hMat = KeypointMatchToTemplate(replaned, bestLowerRight, out CardType cardType, out CardElement cardElement);
            Cv2.WarpPerspective(cardScene, replaned, firstTMat * hMat, new Size(defaultCardWidth, defaultCardHeight)); // TODO : may be backwards matrix

            // the default used for keypoint matching has a border so that we can get better keypoints at the border of cards (since it doesn't use image edge in its feature finding)
            // if we replaned the image perfectly, we will have a matchBorder sized border
            Mat croppedReplaned = replaned[borderAmount, replaned.Height - borderAmount, borderAmount, replaned.Width - borderAmount];
            if (replaneImage.texture != null)
            {
                Destroy(replaneImage.texture);
            }
            replaneImage.texture = OpenCvSharp.Unity.MatToTexture(croppedReplaned);

            // Read in number data
            Mat readIn = bannerNumberBox.CropByBox(croppedReplaned);
            numberText.text = "Card ID: " + GetCardIDFromShapes(readIn);

            replaned.Release();

            break; // TODO : only 1 right now. The best.
        }
    }

    /**
    *  Given expected card type and element, run thru the most likely cards and try to get the best keypoint matches
    *  
    */
   private Mat KeypointMatchToTemplate(Mat replaned, CardCorner bestLowerRight, out CardType cardType, out CardElement cardElement)
   {

       // GET KEYPOINTS FOR THE REPLANED IMAGE
       GetKeypoints(replaned, out KeyPoint[] kp2, out Mat des2);

        // ITERATE THRU THE MOST LIKELY CARDS
        bool hasList = templateCardDict.TryGetValue(
            (int)bestLowerRight.mostLikelyType | (int)bestLowerRight.mostLikelyElement, 
            out List<TemplateCardData> cardDataList);
        int bestGoodMatches = 0;
        Mat bestHomographyMat = null;
        foreach (TemplateCardData cardData in cardDataList)
        {
            KeyPoint[] kp1 = cardData.keypoints;
            Mat des1 = cardData.des;
            MatchKeypointsBoring(kp1, kp2, des1, des2, out DMatch[] goodMatches);

            GetMatchedKeypoints(kp1, kp2, goodMatches, out Point2f[] m_kp1, out Point2f[] m_kp2);
            int initMatches = goodMatches.Length;
            if (FilterByFundy(ref m_kp1, ref m_kp2, ref goodMatches, 1.5f))
            {
                if (CheckIfEnoughMatch(goodMatches, initMatches) && bestGoodMatches < goodMatches.Length)
                {
                    bestGoodMatches = goodMatches.Length;
                    bestHomographyMat = GetHomographyMatrix(m_kp2, m_kp1);
                    if (bestHomographyMat == null 
                        || (bestHomographyMat.Type() != MatType.CV_32F && bestHomographyMat.Type() != MatType.CV_64F)
                        || bestHomographyMat.Width != 3 || bestHomographyMat.Height != 3)
                    {
                        Debug.LogError("Error: Failed to create homography matrix for " + goodMatches.Length + " keypoints.");
                    }
                }
            }
        }

        // TODO : we can also do the others if we got nothing good, but that would be expensive.

        cardType = bestLowerRight.mostLikelyType;
        cardElement = bestLowerRight.mostLikelyElement;
        return bestHomographyMat;
   }


    /**
     * From the estimated mean values, get the closest average element and say that that is probably the element.
     * TODO : may not work on bad cameras that output too much red. 
     * Being color agnostic is an ideal we probably cannot meet...
     */
    private CardElement GetMostLikelyElement(Mat replaned, int cornerReplaneOffset)
    {
        Mat area = replaned[cornerReplaneOffset, replaned.Width - cornerReplaneOffset, cornerReplaneOffset, replaned.Height - cornerReplaneOffset];

        using (Mat crop = elementColorBox.CropByBox(area))
        {
            Scalar tMean = crop.Mean();
            print("Template: " + (float)tMean.Val0 + ", " + (float)tMean.Val1 + ", " + (float)tMean.Val2);

            float bestDist = Mathf.Infinity;
            CardElement bestElement = CardElement.Dark;

            foreach (CardElement element in cardElementDict.Keys)
            {
                Scalar rMean = cardElementDict[element].typeScalar;
                print("Response: " + (float)rMean.Val0 + ", " + (float)rMean.Val1 + ", " + (float)rMean.Val2);

                float dist = Mathf.Sqrt((float)
                    ((rMean.Val0 - tMean.Val0) * (rMean.Val0 - tMean.Val0) +
                    (rMean.Val0 - tMean.Val1) * (rMean.Val0 - tMean.Val1) +
                    (rMean.Val0 - tMean.Val2) * (rMean.Val0 - tMean.Val2)));
                if (dist < bestDist)
                {
                    dist = bestDist;
                    bestElement = element;
                }
            }
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
    public float loweRatio = 0.7f;
    public float kpDist = 7;

    /**
    Match the keypoints of image 1 and 2 using BF batcher and the ratio test.
    The current ratio is 0.7.
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
                    Debug.LogError("ERROR: Matching is being mean.");
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
                    if (dist1 < dist0 && dist1 < kpDist && !goodMatchesList.Contains(m_n[0]))
                        goodMatchesList.Add(m_n[1]);
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

    public bool CheckIfEnoughMatch(DMatch[] matches, int initialMatchCount)
    {
        return matches.Length >= 4;// && matches.Length > initialMatchCount * 0.5f; // TODO : do better here
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
        Mat fundy = Cv2.FindFundamentalMat(ConvertFromF(kp1_pt), ConvertFromF(kp2_pt), FundamentalMatMethod.Ransac, dist, 0.99, mask);
        
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
        matches = newMatches.ToArray();
        return true;
    }

    //** THIS IS THE ONE WE ARE CURRENTLY USING!!! */
    private bool FilterByFundy(ref Point2d[] kp1_pt, ref Point2d[] kp2_pt, ref DMatch[] matches) {
        if (kp1_pt.Length <= 8) return false;
        
        Mat mask = new Mat(new int[] { kp1_pt.Length }, MatType.CV_8UC1);
        Mat fundy = Cv2.FindFundamentalMat(kp1_pt, kp2_pt, FundamentalMatMethod.Ransac, 2, 0.99, mask);

        List<Point2d> n_kp1_pt = new List<Point2d>();
        List<Point2d> n_kp2_pt = new List<Point2d>();
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
        matches = newMatches.ToArray();
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
        public TemplateCardData(KeyPoint[] k, Mat d)
        {
            keypoints = k;
            des = d;
        }

        ~TemplateCardData()
        {
            des.Dispose(); // TODO?
            des.Release();
        }

        public KeyPoint[] keypoints;
        public Mat des;
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
        public CardElementTemplateData(Color ti)
        {
            typeColor = ti; // TODO : may be bgr? or rgb
            typeScalar = new Scalar(ti.b, ti.g, ti.r, ti.a);
        }
        public Color typeColor;
        public Scalar typeScalar;
    }

    /**
     * Normalized bounding box (0 to 1 for width and height), users must convert to pixel space if needed.
     * Bounding box manages all corners so can be rotated.
     * DOES NOT CHECK IF YOU ACTUALLY GIVE IT A BOX!!!
     */
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
    }
}
