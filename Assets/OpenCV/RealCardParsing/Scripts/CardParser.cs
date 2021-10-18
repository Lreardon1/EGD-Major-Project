using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using UnityEngine.UI;
using OpenCvSharp.Aruco;
using OpenCvSharp.XFeatures2D;

public class CardParser : MonoBehaviour
{
    public RawImage replanedRawImage;
    public RawImage contourImage;
    public RawImage arucoImage;
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

    void Update()
    {
        ParseCard(OpenCvSharp.Unity.TextureToMat(image));
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

    private float GetMatchScore(Mat im1, Mat template, float histWeight)
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


    private float AttemptToMatchCardByTemplateFromRect(Mat card, Mat matchThing, Point2f[] rect, out int rotCount, out Mat best)
    {
        // TODO : shape matching IS gonna be the best go...
        Size s = matchThing.Size();
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

        // TODO : remove

        Mat subCard = card.SubMat(top, bottom, left, right);
        
        for (int i = 0; i < newRect.Length; ++i)
        {
            newRect[i].Y -= top;
            newRect[i].X -= left;
        }

        int currentRot = 0;
        int bestRot = 0;
        float bestMatch = 0.0f;

        best = null;

        while (currentRot < 4) {

            Mat perspMat = Cv2.GetPerspectiveTransform(newRect, destRect);

            Mat warped = new Mat();
            Cv2.WarpPerspective(subCard, warped, perspMat, matchThing.Size());
            float matchScore = GetMatchScore(warped, matchThing, 0.1f);

            if (matchScore > bestMatch)
            {
                bestMatch = matchScore;
                bestRot = currentRot;
                best = warped;
            }
            else warped.Release(); // TODO


            perspMat.Release();

            // ROTATE
            Point2f nowFirst = newRect[3];
            newRect[3] = newRect[2];
            newRect[2] = newRect[1];
            newRect[1] = newRect[0];
            newRect[0] = nowFirst;
            currentRot++;
        }
        subCard.Release();

        rotCount = bestRot;
        return bestMatch;
    }

    public struct CardCorner
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

    // RETURNS A WINDING ORDER ROTATED VERSION OF RECT OR NULL BUT DOES NOT RETURN RECT IS ANY WAY TRANSFORMED
    private Point2f[] GetRatioAndOrientationOfTL(Point2f[] rect, Point2f[] original, Mat persp, out float ratio, out float area)
    {
        ratio = 1.0f / ComputeApparentRatio(rect);
        area = 0;
        if (ratio < 0.9f)
            return null;

        Point2f[] warped = Cv2.PerspectiveTransform(rect, persp);
        Point2f[] warpedOG = Cv2.PerspectiveTransform(original, persp);
        // TODO : check that they are in expected places better than this : this is WAY easier if you have persp be on the original bounding box of lower right
        // TODO : you can also spin round the winding order and if a corner angle is too little or too great disqualify.
        foreach (Point2f pt_w in warped)
        {
            foreach(Point2f pt_og in warpedOG)
            {
                if (pt_w.X + pt_w.Y > pt_og.X + pt_og.Y)
                    return null;
            }
        }

        area = (float)Cv2.ContourArea(warped);

        // TODO : an alternative way could be to check if each is a line along axis
        Point2f[] line1 = new Point2f[] { warped[0], warped[1] };
        Point2f[] line2 = new Point2f[] { warped[2], warped[3] };
        float intersectDot = GetIntersectDot(line1, line2);
        //print("For ratio " + ratio + " got " + intersectDot + " for first");
        if (intersectDot > -0.97f && intersectDot < 0.97f)
            return null;

        line1 = new Point2f[] { warped[1], warped[2] };
        line2 = new Point2f[] { warped[3], warped[0] };
        intersectDot = GetIntersectDot(line1, line2);
        //print("For ratio " + ratio + " got " + intersectDot + " for second");
        if (intersectDot > -0.97f && intersectDot < 0.97f)
            return null;

        int topLeftInd = FindMinSum(warped);
        Point2f[] newRect = new Point2f[4];
        rect.CopyTo(newRect, 0);
        while (topLeftInd != 0)
        {
            Point2f overstep = newRect[0];
            newRect[0] = newRect[1];
            newRect[1] = newRect[2];
            newRect[2] = newRect[3];
            newRect[3] = overstep;
            topLeftInd--;
        }

        // TODO : check if has number, and/or, check if inside expected box
        // BIG TODO : get standard mapping to test all these things!!!!

        return newRect;
    }

    private Point2f[] FindTLCornerFromBRCorner(Point2f[] corner, Point2f[][] rects)
    {
        Point2f[] dest = new Point2f[] {
            new Point2f(0, 0), new Point2f(10.0f, 0),
            new Point2f(10.0f, 10.0f), new Point2f(0, 10.0f) }; // TODO : this works but the points are technically 'off the image'

        Mat persp = Cv2.GetPerspectiveTransform(corner, dest);
        Point2f[] warped = Cv2.PerspectiveTransform(corner, persp);
        float expectedArea = (float)Cv2.ContourArea(warped);

        float bestRatio = 0.8f;
        Point2f[] bestTopLeft = null;
        foreach (Point2f[] rect in rects)
        {
            if (rect == corner)
            {
                print("CAUGHT");
                continue;
            }

            float ratio;
            float area;
            Point2f[] topLeft = GetRatioAndOrientationOfTL(rect, corner, persp, out ratio, out area);
            if (topLeft != null && ratio > bestRatio && Mathf.Abs(area - expectedArea) < 20)
            {
                bestRatio = ratio;
                bestTopLeft = topLeft;
            }
        }
        return bestTopLeft;
    }

    private Point2f[][] GetCardBoundsFromLRCorner(Point2f[] corner, Point2f[][] rects)
    {
        // TODO : improve search putting up a AABB on who is eligible to by considered for top left
        // would massively improve detection
            // like all my stops it requires a standard card, that I DONT HAVE!!! FUCK

        Point2f[] tag = GetBestParallelMatchToCorner(corner, rects); //TODO : tag may not be 4 side
        Point2f[] topLeft = FindTLCornerFromBRCorner(corner, rects); // ?SHOULD? BE orientated s.t. 
        if (topLeft == null)
            return null; // failed to find card

        float[] topLine = GetLineFromPoints(topLeft[0], topLeft[1]);
        float[] rightLine = GetLineFromPoints(corner[1], corner[2]);
        float[] bottomLine = GetLineFromPoints(corner[2], corner[3]);
        float[] leftLine = GetLineFromPoints(topLeft[3], topLeft[0]);

        Point2f topLeftCardPt = topLeft[0];
        Point2f topRightCardPt = GetIntersectionPoint(topLine, rightLine);
        Point2f bottomRightCardPt = corner[2];
        Point2f bottomLeftCardPt = GetIntersectionPoint(bottomLine, leftLine);

        //return corner;
        //return topLeft;
        return new Point2f[][]
        {
            topLeft,
            corner,
            new Point2f[] {topLeftCardPt,
            topRightCardPt,
            bottomRightCardPt,
            bottomLeftCardPt }
        }; 
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

    private Point2f[][] FindCardsFromContours(Mat card, Point2f[][] rects)
    {
        // Two schemes:
        // 3 corner
        // 2 corner, we start with this
        List<int> rotList = new List<int>();
        List<CardCorner> rectList = new List<CardCorner>();
        List<Mat> debugBests = new List<Mat>();
        foreach (Point2f[] rect in rects)
        {
            int rotCount = 0;
            Mat best;

            float matchToCurrent = AttemptToMatchCardByTemplateFromRect(card, brMatch, rect, out rotCount, out best);
            if (matchToCurrent > matchThresh) // TODO
            {
                rectList.Add(new CardCorner { corners = rect, matchVal = matchToCurrent, neededRot = rotCount });
                debugBests.Add(best);
            }
        }

        rectList.Sort((k1, k2) => k2.matchVal.CompareTo(k1.matchVal));
        if (rectList.Count == 0)
            return null;

        List<Point2f[]> temp = new List<Point2f[]>(rects);
        temp.Remove(rectList[0].corners);
        return GetCardBoundsFromLRCorner(RotateWinding(rectList[0].corners, rectList[0].neededRot), temp.ToArray());
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
        // TODO : signs might be flipped
        float a = line1[0] / -line1[1];
        float c = line1[2] / -line1[1];

        float b = line2[0] / -line2[1];
        float d = line2[2] / -line2[1];

        Point2f intersect = new Point2f();
        intersect.X = (d - c) / (a - b);
        intersect.Y = (a * ((d - c) / (a - b))) + c;
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

    int cInd = 80;
    public void ParseCard(Mat cardScene)
    {
        Mat greyCard = new Mat();
        Mat edgeCard = new Mat();
        Cv2.CvtColor(cardScene, greyCard, ColorConversionCodes.BGR2GRAY);
        Cv2.Canny(greyCard, edgeCard, 80, 255);
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(edgeCard, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
        Mat blackOut = new Mat();
        cardScene.CopyTo(blackOut);

        Dictionary arucoDict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
        DetectorParameters arucoParams = DetectorParameters.Create();
        /*
        Point2f[][] corners;
        int[] ids;
        Point2f[][] rejects;
        CvAruco.DetectMarkers(cardScene, arucoDict, out corners, out ids, arucoParams, out rejects);
        */
        Point2f[][] rectContours = SimplifyContours(contours, 4);
        Point2f[][] possibleCard = FindCardsFromContours(cardScene, rectContours);
        CvAruco.DrawDetectedMarkers(cardScene,  possibleCard, null);


        arucoImage.texture = OpenCvSharp.Unity.MatToTexture(cardScene);

        float off = 10;
        Point2f[] mainCorners = new Point2f[] 
        {
                new Point2f(off, off),
                new Point2f(defaultCardMat.Width - off, off),
                new Point2f(defaultCardMat.Width - off, defaultCardMat.Height - off),
                new Point2f(off, defaultCardMat.Height - off)
        };

        Mat persp = Cv2.GetPerspectiveTransform(possibleCard[2], mainCorners);

        Mat replaned = new Mat();
        Size warpSize = new Size(defaultCardMat.Width, defaultCardMat.Height);
        Cv2.WarpPerspective(cardScene, replaned, persp, warpSize);


        Mat readIn = bannerNumberBox.CropByBox(replaned);
        diffImages[1].texture = OpenCvSharp.Unity.MatToTexture(bannerNumberBox.CropByBox(replaned));
        numberText.text = "Card ID: " + GetCardIDFromShapes(readIn);


        replanedRawImage.texture = OpenCvSharp.Unity.MatToTexture(replaned);

        KeyPoint[] kp1; Mat des1;
        GetKeypoints(defaultCardMat, out kp1, out des1);
        Cv2.DrawKeypoints(defaultCardMat, kp1, blackOut);
        contourImage.texture = OpenCvSharp.Unity.MatToTexture(blackOut);


        replaned.Release();
        persp.Release();
        cardScene.Release();
        edgeCard.Release();
        blackOut.Release();
        greyCard.Release();
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
    }
}
