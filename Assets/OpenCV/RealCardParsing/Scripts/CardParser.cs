using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using OpenCvSharp.Aruco;
using OpenCvSharp.XFeatures2D;
using System;

public class CardParser : MonoBehaviour
{
    public RawImage matcherImage;
    public RawImage contourImage;
    public RawImage arucoImage;
    public RawImage replaneImage;
    public RawImage[] diffImages;
    public Text numberText;
    public Texture2D image;
    public Texture2D defaultCardTexture;
    public float defaultCardResizeAmount = 0.5f;
    public Mat defaultCardMat;

    // BOUNDING BOXES
    [Space(10)]
    public Point2f bottomRightBoundingBox_UL;
    public Point2f bottomRightBoundingBox_LR;
    public BoundingBox bottomRightBoundingBox;
    [Space(10)]

    public Point2f upperLeftBoundingBox_UL;
    public Point2f upperLeftBoundingBox_LR;
    public BoundingBox upperLeftBoundingBox;
    [Space(10)]

    public Point2f bannerNumberBox_UL;
    public Point2f bannerNumberBox_LR;
    public BoundingBox bannerNumberBox;


    public float matchThresh = 0.8f;
    public float tagLineIntersectThresh = 4.0f;

    private Mat brMatch;

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



    private void Start()
    {
        bottomRightBoundingBox = new BoundingBox(bottomRightBoundingBox_UL, bottomRightBoundingBox_LR);
        upperLeftBoundingBox = new BoundingBox(upperLeftBoundingBox_UL, upperLeftBoundingBox_LR);
        bannerNumberBox = new BoundingBox(bannerNumberBox_UL, bannerNumberBox_LR);

        defaultCardMat = OpenCvSharp.Unity.TextureToMat(defaultCardTexture);
        Cv2.Resize(defaultCardMat, defaultCardMat,
            new Size(defaultCardMat.Size().Width * defaultCardResizeAmount, defaultCardMat.Size().Height * defaultCardResizeAmount));

        brMatch = bottomRightBoundingBox.CropByBox(defaultCardMat);
        print(brMatch.Size());
        diffImages[0].texture = OpenCvSharp.Unity.MatToTexture(brMatch);
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
        //Debug.Log("Time to complete: " + (Time.realtimeSinceStartupAsDouble - t) + ". Delta Time: " + Time.deltaTime);
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

        //Mat cardScene = OpenCvSharp.Unity.TextureToMat(input);
        Mat cardScene = OpenCvSharp.Unity.TextureToMat(image);
        ParseCard(cardScene);
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

        Cv2.CvtColor(im, hsvIM, ColorConversionCodes.BGR2HSV); // TODO : BGR might be wrong here?
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
        Mat blur1 = new Mat(), templateBlur = new Mat();
        Cv2.GaussianBlur(im1, blur1, new Size(5, 5), 0.4f, 0.4f);
        Cv2.GaussianBlur(template, templateBlur, new Size(5, 5), 0.4f, 0.4f);

        Mat diff = new Mat();
        Cv2.Absdiff(templateBlur, blur1, diff);
        Scalar channelSums = Cv2.Sum(diff);
        double sum = channelSums.Val0 + channelSums.Val1 + channelSums.Val2 + channelSums.Val3;

        
        float diffOp = 1.0f - ((float)sum / (template.Size().Height * template.Size().Width * 3 * 256));
        float histOp = GetHistogramMatch(im1, template);

        diff.Release();
        blur1.Release();
        templateBlur.Release();

        return (histWeight * histOp) + ((1f - histWeight) * diffOp);
    }

    int count = 0;
    int vel = 0;
    private float GetShapeScore(Mat im, out int type)
    {
        using (Mat greyBR = new Mat())
        using (Mat greyIM = new Mat()) 
        {
            // TODO : perform this on all of the shapes, and store them s.t. you don't recalc everytime 
            // TODO : jerry rigging is fine but should not be getting done at runtime.
            // (might also be more stable if you guarenteed success in editor rather than runtime)
            Cv2.CvtColor(brMatch, greyBR, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(greyBR, greyBR, new Size(3, 3), 2);
            Cv2.AdaptiveThreshold(greyBR, greyBR, 125, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 9, 12);
            Point[][] templateShape;
            HierarchyIndex[] h;
            Cv2.FindContours(greyBR, out templateShape, out h, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            if (templateShape.Length != 2)
                print("WELL SHIT, that's a problem now!");
            print(templateShape[0].ToString());
            Mat black = Mat.Zeros(greyBR.Size(), greyBR.Type());
            Cv2.DrawContours(black, templateShape, 1, Scalar.White, 1);
            diffImages[0].texture = OpenCvSharp.Unity.MatToTexture(black);

            // get all grey shapes and test them
            Cv2.CvtColor(im, greyIM, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(greyIM, greyIM, new Size(3, 3), 2);
            Cv2.AdaptiveThreshold(greyIM, greyIM, 125, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 9, 12);
            Point[][] imShapes;
            Cv2.FindContours(greyIM, out imShapes, out h, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);

            float bestMatch = 10.0f;
            foreach (Point[] shape in imShapes)
            {
                float match = (float)Cv2.MatchShapes(templateShape[1], shape, ShapeMatchModes.I1);
                bestMatch = Mathf.Min(bestMatch, match);
            }

            if (Input.GetKeyDown(KeyCode.Q)) vel++;
            if (count == vel)
            {
                Cv2.DrawContours(greyIM, new Point[][] { imShapes[1] }, -1, Scalar.White);
                diffImages[1].texture = OpenCvSharp.Unity.MatToTexture(greyIM);
            }
            count++;
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
        int bottom = Mathf.CeilToInt(Mathf.Max(newRect[0].Y, newRect[1].Y, newRect[2].Y, newRect[3].Y)); // TODO?
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

    // TODO : this guy is fucking it up because Hu are invariant
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
        int bottom = Mathf.CeilToInt(Mathf.Max(newRect[0].Y, newRect[1].Y, newRect[2].Y, newRect[3].Y)); // TODO?
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

    public class CardCorner
    {
        public float matchVal;
        public Point2f[] corners; // CW
        public int neededRot;
    }

    // this is trying to find the tag
    // TODO : at this point it would be better to be arranged appro (clear start to winding order)
    // TODO : this could also be a better way to figure out orientation
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
                    // TODO : we can also now give back the lines paralleled from this using foundPoitns
                    return shape;
                }
            }
        }
        return null;
    }

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
        if (Mathf.Abs(line1[0]) < 0.0001f && Mathf.Abs(line2[1]) < 0.0001f)
        {
            intersect.X = line2[2] / line2[0];
            intersect.Y = line1[2] / line1[1];
            return intersect;
        }
        else if (Mathf.Abs(line1[1]) < 0.0001f && Mathf.Abs(line2[0]) < 0.0001f)
        {
            intersect.X = line1[2] / line1[0];
            intersect.Y = line2[2] / line2[1];
            return intersect;
        }
        else if (Mathf.Abs((a - b)) < 0.001f)
            return new Point2f(Mathf.Infinity, Mathf.Infinity);

        intersect.X =  (d - c) / (a - b);
        intersect.Y = a * ((d - c) / (a - b)) + c;
        return intersect;
    }


    private float GetDistanceFromLine(float[] line, Point2f pt)
    {
        return Mathf.Abs((line[0] * pt.X) + (line[1] * pt.Y) + line[2]) 
            / Mathf.Sqrt((line[0] * line[0]) + (line[1] * line[1]));
    }

    private Point2f[][] SimplifyContours(Point[][] contours, int sizeReq=-1)
    {
        List<Point2f[]> contourList = new List<Point2f[]>();

        foreach (Point[] cont in contours)
        {
            Point[] simp = Cv2.ApproxPolyDP(cont, 0.04f * Cv2.ArcLength(cont, true), true);
            if (sizeReq == -1 || simp.Length == sizeReq)
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

    private int GetCardIDFromShapes(Mat im)
    {
        using (Mat grey = new Mat())
        {
            Cv2.CvtColor(im, grey, ColorConversionCodes.BGR2GRAY);
            float avg = (float)Cv2.Mean(grey).Val0;
            Cv2.Threshold(255 - grey, grey, 255 - avg, 255, ThresholdTypes.Binary);
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(grey, out contours, out hierarchy, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
            Point2f[][] simpCnts = SimplifyContours(contours, -1);
            diffImages[2].texture = OpenCvSharp.Unity.MatToTexture(grey);

            int val = 0;
            foreach (Point2f[] cnt in simpCnts)
            {
                val += cnt.Length;
            }
            return val;
        }
    }


    private void DetectContours(Mat scene, out Point2f[][] contours, out HierarchyIndex[] hierarchy)
    {
        // BIG TODO : more accurate this
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

            Point2f[][] corners;
            int[] ids;
            CvAruco.DetectMarkers(scene, arucoDict, out corners, out ids, arucoParams, out contours);
        }
    }
    
    private CardCorner FindBestLowerRightCardCorner(Mat scene, ref Point2f[][] contours)
    {
        // Two schemes:
        // 3 corner
        // 2 corner, we start with this
        List<CardCorner> rectList = new List<CardCorner>();
        foreach (Point2f[] rect in contours)
        {
            int rotCount = 0; // todod : more robust checking here, currently we use shape to filter and diff to select rotation
            float matchByShape = AttemptToMatchByShape(scene, brMatch, rect);
            float matchToDiff = AttemptToMatchCardByTemplate(scene, brMatch, rect, out rotCount);
            if (matchByShape > matchThresh)
            {
                rectList.Add(new CardCorner { corners = rect, matchVal = (matchByShape * 0.8f) + (matchToDiff * 0.2f), neededRot = rotCount });
            }
        }

        rectList.Sort((k1, k2) => k2.matchVal.CompareTo(k1.matchVal));
        if (rectList.Count == 0)
            return null;

        List<Point2f[]> temp = new List<Point2f[]>(contours);
        temp.Remove(rectList[0].corners);
        contours = temp.ToArray();
        return rectList[0];
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
        //print("For ratio " + ratio + " got " + intersectDot + " for first");
        if (intersectDot > -0.97f && intersectDot < 0.97f)
            return -1;

        line1 = new Point2f[] { warpedCanid[1], warpedCanid[2] };
        line2 = new Point2f[] { warpedCanid[3], warpedCanid[0] };
        intersectDot = GetIntersectDot(line1, line2);
        //print("For ratio " + ratio + " got " + intersectDot + " for second");
        if (intersectDot > -0.97f && intersectDot < 0.97f)
            return -1;

        // todo : rot may be off here
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

    private bool CompareAreaForULMatch(float expected, float area)
    {
        float ratio = expected / area;
        ratio = (ratio < 1.0f) ? 1.0f / ratio : ratio;
        return ratio < 1.7f; // TODO : arbitrary
    }

    // TODO : Expects lowerRight to be wound properly!!!
    private CardCorner FindBestUpperLeftCardCorner(Mat scene, Point2f[] lowerRight, ref Point2f[][] contours)
    {
        Point2f[] dest = bottomRightBoundingBox.GetCWWindingOrder();
        Point2f upperLeftCenter = upperLeftBoundingBox.GetCenter();

        using (Mat persp = Cv2.GetPerspectiveTransform(lowerRight, dest))
        {
            Point2f[] warpedLR = Cv2.PerspectiveTransform(lowerRight, persp);
            float expectedArea = (float)Cv2.ContourArea(warpedLR);

            int bestRotCount = 0;
            float bestRatio = 0.8f;
            Point2f[] bestTopLeft = null;

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
                float ratio = 1.0f / ComputeApparentRatio(warpedUL);
                float area = (float)Cv2.ContourArea(warpedUL);
                if (ratio < bestRatio || !CompareAreaForULMatch(expectedArea, area))
                    continue;

                bestRatio = ratio;
                bestRotCount = rotCount;
                bestTopLeft = canid;
            }

            if (bestTopLeft == null)
                return null;

            // filter out the best from the contour list
            List<Point2f[]> temp = new List<Point2f[]>(contours);
            temp.Remove(bestTopLeft);
            contours = temp.ToArray();

            return new CardCorner { corners = bestTopLeft, matchVal = bestRatio, neededRot = bestRotCount };
        }
    }

    private Point2f[] TryToBoundCardFromCorners(Mat scene, Point2f[] lowerRight, Point2f[] upperLeft)
    {
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

    public void ParseCard(Mat cardScene)
    {
        count = 0;
        Mat blackOut = new Mat();
        Point2f[][] contours;
        HierarchyIndex[] h;
        // DETECT CONTOURS AND SIMPLIFY THEM IF NEEDED
        DetectContours(cardScene, out contours, out h);
        // Point2f[][] rectContours = SimplifyContours(ConvertPoint2fToPoint( contours, 4);
        cardScene.CopyTo(blackOut);

        // LOWER RIGHT
        CardCorner bestLowerRight = FindBestLowerRightCardCorner(cardScene, ref contours);
        if (bestLowerRight == null) return; // TODO : handle failure
        print("Desired rot = " + bestLowerRight.neededRot);
        bestLowerRight.corners = RotateWinding(bestLowerRight.corners, bestLowerRight.neededRot);
        bestLowerRight.neededRot = 0;
        print("Selected corner got out with match val: " + bestLowerRight.matchVal);
        CvAruco.DrawDetectedMarkers(cardScene, new Point2f[][] { bestLowerRight.corners  }, null);
        arucoImage.texture = OpenCvSharp.Unity.MatToTexture(cardScene);

        CardCorner bestUpperLeft = FindBestUpperLeftCardCorner(cardScene, bestLowerRight.corners, ref contours);
        if (bestUpperLeft == null) print("WELL SHIT, I got the right guy?");
        if (bestUpperLeft == null) return; // TODO : handle failure
        bestUpperLeft.corners = RotateWinding(bestUpperLeft.corners, bestUpperLeft.neededRot);
        bestUpperLeft.neededRot = 0;
        print("Best upper left got out");

        Point2f[] possibleCard = TryToBoundCardFromCorners(cardScene, bestLowerRight.corners, bestUpperLeft.corners);

        if (possibleCard == null)
        {
            numberText.text = "No Card detected";
            return;
        }

        float off = 25;
        Point2f[] mainCorners = new Point2f[] 
        {
                new Point2f(off, off),
                new Point2f(defaultCardMat.Width - off, off),
                new Point2f(defaultCardMat.Width - off, defaultCardMat.Height - off),
                new Point2f(off, defaultCardMat.Height - off)
        };

        Mat persp = Cv2.GetPerspectiveTransform(possibleCard, mainCorners);

        Mat replaned = new Mat();
        Size warpSize = new Size(defaultCardMat.Width, defaultCardMat.Height);
        Cv2.WarpPerspective(cardScene, replaned, persp, warpSize);

        GetKeypoints(replaned, out KeyPoint[] kp2, out Mat des2);

        int matchBorder = 10;
        KeyPoint[] kp1; Mat des1;
        Mat matchDefault = new Mat();
        Cv2.CopyMakeBorder(defaultCardMat, matchDefault, matchBorder, matchBorder, matchBorder, matchBorder, BorderTypes.Constant, Scalar.White);

        GetKeypoints(matchDefault, out kp1, out des1);
        Cv2.DrawKeypoints(matchDefault, kp1, blackOut);
        contourImage.texture = OpenCvSharp.Unity.MatToTexture(blackOut);

        CvAruco.DrawDetectedMarkers(cardScene, new Point2f[][] { bestLowerRight.corners, bestUpperLeft.corners, possibleCard }, null);
        arucoImage.texture = OpenCvSharp.Unity.MatToTexture(cardScene);

        MatchKeypoints(kp1, kp2, des1, des2, out DMatch[] goodMatches); // TODO : A POSSIBLITY FOR CUSTOM MATCHING DUE TO HOW CLOSE THEY ARE SUPPOSE TO BE, given initial mapping
        if (CheckIfEnoughMatch(goodMatches))
        {
            GetMatchedKeypoints(kp1, kp2, goodMatches, out Point2f[] m_kp1, out Point2f[] m_kp2);
            Mat n = new Mat();
            Cv2.DrawMatches(matchDefault, kp1, replaned, kp2, goodMatches, n);
            matcherImage.texture = OpenCvSharp.Unity.MatToTexture(n);

            Cv2.WarpPerspective(replaned, replaned, GetHomographyMatrix(m_kp2, m_kp1), matchDefault.Size());
            replaneImage.texture = OpenCvSharp.Unity.MatToTexture(replaned);
        }
        else
        {
            print("I don't think this was a card");
        }
        Mat croppedReplaned = replaned[matchBorder, replaned.Height - matchBorder, matchBorder, replaned.Width - matchBorder];


        Mat readIn = bannerNumberBox.CropByBox(croppedReplaned);
        diffImages[1].texture = OpenCvSharp.Unity.MatToTexture(bannerNumberBox.CropByBox(croppedReplaned));
        numberText.text = "Card ID: " + GetCardIDFromShapes(readIn);
        print("CARD ID IS: " + GetCardIDFromShapes(readIn));


        //replanedRawImage.texture = OpenCvSharp.Unity.MatToTexture(replaned);
        replaned.Release();
        persp.Release();
        cardScene.Release();
        blackOut.Release();
    }

    public void GetKeypoints(Mat im, out KeyPoint[] keypoints, out Mat des)
    {
        des = new Mat();
        using (var gray = im.CvtColor(ColorConversionCodes.BGR2GRAY))
        using (var surf = SIFT.Create())
        {
            surf.DetectAndCompute(gray, null, out keypoints, des);
        }
    }

    public float loweRatio = 0.7f;
    public float kpDist = 30;
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
                if (m_n.Length != 2)
                {
                    Debug.LogError("ERROR: Matching is being mean.");
                    continue;
                }
                if (m_n[0].Distance < loweRatio * m_n[1].Distance) {
                    float dist0 = (float)kp1[m_n[0].QueryIdx].Pt.DistanceTo(kp2[m_n[0].TrainIdx].Pt);
                    if (dist0 < kpDist)
                        goodMatchesList.Add(m_n[0]);
                } else
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

    public bool CheckIfEnoughMatch(DMatch[] matches)
    {
        return matches.Length >= 4;
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

    // could also use getHomography Matrix
    private Mat GetHomographyMatrix(Point2f[] src, Point2f[] dest)
    {
        
        return Cv2.FindHomography(ConvertFromF(src), ConvertFromF(dest), HomographyMethods.Ransac, 1);
        return Cv2.GetPerspectiveTransform(src, dest);
    }

    protected void GetMatchedKeypoints(KeyPoint[] kp1, KeyPoint[] kp2, DMatch[] matches, out Point2f[] m_kp1, out Point2f[] m_kp2)
    {
        m_kp1 = new Point2f[matches.Length];
        m_kp2 = new Point2f[matches.Length];

        for (int i = 0; i < matches.Length; ++i)
        {
            m_kp1[i] = kp1[matches[i].QueryIdx].Pt;
            m_kp2[i] = kp2[matches[i].TrainIdx].Pt; // TODO : this could be backwards
        }
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
            Point2f[] newCorners = new Point2f[] // TODO : could be backwards (X and Y), could also be too big or too small
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
