using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using OpenCvSharp.XFeatures2D;


// TODO : for all: better use of using block syntax for clean garbage
[System.Serializable]
public class BaseImageParser
{
    public float timeTilAbandonGoodArucoHomography;
    protected float timeOfLastGoodArucoHomography = 0;
    protected Mat arucoLastGoodHomography = null;

    public float timeTilAbandonGoodKeypointHomography;
    public float loweRatio = 0.7f;
    protected float timeOfLastGoodKeypointHomography = 0;
    protected Mat keypointLastGoodHomography;


    // assumes array has 4 elements related to corners
    private static Point2f GetCenterOfRect(Point2f[] rect)
    {
        return (rect[0] + rect[1] + rect[2] + rect[3]) * 0.25f;
    }

    // make 4 main corners, TODO : swap back after you get a correctly wound mat
    private static Point2f[] MakeMainCorners(Size size)
    {
        return new Point2f[] {
            new Point2f(0, 0), // 1
            new Point2f(size.Width, 0), // 4
            new Point2f(0, size.Height), // 2
            new Point2f(size.Width, size.Height), // 3
        };
    }

    // TODO : aruco cards are a good proof of concept but they look like programmer art.
    public static void FindArucoCards(InputArray im, out int[] ids,
    out Point2f[][] corners, out Point2f[][] rejected)
    {
        Dictionary arucoDict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
        DetectorParameters arucoParams = DetectorParameters.Create();

        CvAruco.DetectMarkers(im, arucoDict, out corners, out ids, arucoParams, out rejected);
        //CvAruco.DrawDetectedMarkers(im, corners, ids);
    }

    protected int FindIndexOfCard(int[] ids, int id)
    {
        int ind = -1;
        for (int i = 0; i < ids.Length; ++i)
            if (ids[i] == id)
                ind = i;
        return ind;
    }
       
    /**
     * Replane an image to a normalized plane using corners specified by aruco cards. 
     * TODO : figure out the appropriate combination of corners to allow for correct corner replaning
     * ARUCO ID TAGS SHOULD BE CLOCKWISE FROM UPPER LEFT. (IF 0, 0 IS UPPER LEFT???)
     */
    protected Mat ReplaneUsingAruco(int[] cornerIDs, Mat im, Size expectedSize, ref Mat debugAruco, bool shouldDebug = false)
    {
        double t = Time.realtimeSinceStartupAsDouble;
        int[] ids;
        Point2f[][] corners, rejected;
        // get aruco cards
        FindArucoCards(im, out ids, out corners, out rejected);
        if (shouldDebug) {
            debugAruco = new Mat(im.Size(), im.Type());
            im.CopyTo(debugAruco);
            CvAruco.DrawDetectedMarkers(debugAruco, corners, ids);
        }


        bool[] cornerGood = new bool[4] { false, false, false, false };
        Point2f[] mainCorners = new Point2f[4];

        // collect the corners
        for (int i = 0; i < corners.Length; ++i)
        {
            int ind = FindIndexOfCard(cornerIDs, ids[i]);
            if (ind == -1)
                continue;
            mainCorners[ind] = GetCenterOfRect(corners[i]);
            cornerGood[ind] = true;
        }
        int centerCount = 0;
        foreach (bool good in cornerGood)
            centerCount += good ? 1 : 0;

        // if we have 4 corners
        if (centerCount == 4)
        {
            Mat persp = Cv2.GetPerspectiveTransform(mainCorners, MakeMainCorners(expectedSize));
            arucoLastGoodHomography = persp;
            timeOfLastGoodArucoHomography = Time.time;
            Mat oMat = new Mat(expectedSize, im.Type());
            Cv2.WarpPerspective(im, oMat, persp, oMat.Size());
            return oMat;
        }
        else if (arucoLastGoodHomography != null 
            && timeOfLastGoodArucoHomography + timeTilAbandonGoodArucoHomography >= Time.time)
        {
            // if we have a good, unexpired homography matrix, use it.
            Mat oMat = new Mat(expectedSize, im.Type());
            Cv2.WarpPerspective(im, oMat, arucoLastGoodHomography, oMat.Size());
            return oMat;
        }

        return null;
    }




    /**
     * Get the keypoints.
     * TODO : currently using SURF
     */ 
    public static void GetKeypoints(Mat im, out KeyPoint[] keypoints, out Mat des)
    {
        des = new Mat();
        using (var gray = im.CvtColor(ColorConversionCodes.BGR2GRAY))
        using (var surf = SIFT.Create())
        {
            surf.DetectAndCompute(gray, null, out keypoints, des);
        }
    }

    /**
    Match the keypoints of image 1 and 2 using BF batcher and the ratio test.
    The current ratio is 0.7.
    Returns a list of matches which passed the ratio test.
    */
    public void MatchKeypoints(Mat des1, Mat des2, out DMatch[] goodMatches)
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
                    goodMatchesList.Add(m_n[0]);
            }
            goodMatches = goodMatchesList.ToArray();
        }
    }

    public bool CheckIfEnoughMatch(DMatch[] matches)
    {
        return matches.Length >= 4;
    }

    // could also use getHomography Matrix
    private Mat GetHomographyMatrix(Point2f[] src, Point2f[] dest)
    {
        return Cv2.GetPerspectiveTransform(src, dest);
    }

    protected void GetMatchedKeypoints(KeyPoint[] kp1, KeyPoint[] kp2, DMatch[] matches, out Point2f[] m_kp1, out Point2f[] m_kp2)
    {
        m_kp1 = new Point2f[matches.Length];
        m_kp2 = new Point2f[matches.Length];

        for (int i = 0; i < matches.Length; ++i)
        {
            m_kp1[i] = kp1[matches[i].TrainIdx].Pt;
            m_kp2[i] = kp1[matches[i].QueryIdx].Pt; // TODO : this could be backwards
        }
    }

    // TODO : serialize these somehow? prefabs??? would then require objects. Scriptable objects??? <-- probably
    protected float homoDistThresh = 2.0f;
    protected float homoPercentThresh = 50.0f;

    public bool CheckIfEnoughHomographyMatch(Mat homography, Point2f[] m_kp1, Point2f[] m_kp2)
    {
        Point2f[] h_m_kp2 = Cv2.PerspectiveTransform(m_kp2, homography);
        int goodCount = 0;
        for (int i = 0; i < m_kp1.Length; ++i)
        {
            if (h_m_kp2[i].DistanceTo(m_kp1[i]) < homoDistThresh)
                goodCount++; 
        }
        return (float)goodCount / m_kp1.Length > homoPercentThresh && goodCount >= 4;
    }

    public Mat ReplaneUsingKeypoints(Mat planedImage, Mat im, ref Mat debugMat, bool shouldDebug = false)
    {
        KeyPoint[] kp1; Mat des1;
        KeyPoint[] kp2; Mat des2;
        float t = Time.realtimeSinceStartup;
        GetKeypoints(planedImage, out kp1, out des1); // TODO : store this
        Debug.Log((Time.realtimeSinceStartup - t) + " to get keypoints");
        GetKeypoints(im, out kp2, out des2);

        DMatch[] matches;
        MatchKeypoints(des1, des2, out matches);

        if (shouldDebug)
        {
            debugMat = new Mat();
            Cv2.DrawMatches(planedImage, kp1, im, kp2, matches, debugMat);
        }

        if (CheckIfEnoughMatch(matches))
        {

            Point2f[] m_kp1, m_kp2;
            GetMatchedKeypoints(kp1, kp2, matches, out m_kp1, out m_kp2);
            Mat homography = Cv2.GetPerspectiveTransform(m_kp2, m_kp1); // TODO : may be reversed

            if (CheckIfEnoughHomographyMatch(homography, m_kp1, m_kp2))
            {
                keypointLastGoodHomography = homography;
                timeOfLastGoodKeypointHomography = Time.time;
                Mat output = new Mat(planedImage.Size(), planedImage.Type());
                Cv2.WarpPerspective(im, output, homography, planedImage.Size());
                Debug.Log("YOU GOT A WRONG TRANSFORM");
                return output;
            }
        }
        if (timeOfLastGoodKeypointHomography + timeTilAbandonGoodKeypointHomography > Time.time)
        {
            Mat output = new Mat(planedImage.Size(), planedImage.Type());
            Cv2.WarpPerspective(im, output, keypointLastGoodHomography, planedImage.Size());
            return output;
        }

        return null;
    }

    public Mat ReplaneUsingKeypoints(KeyPoint[] kp1, Mat des1, Size templateSize, MatType templateType, Mat im)
    {
        KeyPoint[] kp2; Mat des2;
        float t = Time.realtimeSinceStartup;
        GetKeypoints(im, out kp2, out des2);

        DMatch[] matches;
        MatchKeypoints(des1, des2, out matches);
        
        if (CheckIfEnoughMatch(matches))
        {
            Point2f[] m_kp1, m_kp2;
            GetMatchedKeypoints(kp1, kp2, matches, out m_kp1, out m_kp2);
            Mat homography = Cv2.GetPerspectiveTransform(m_kp2, m_kp1); // TODO : may be reversed

            if (CheckIfEnoughHomographyMatch(homography, m_kp1, m_kp2))
            {
                keypointLastGoodHomography = homography;
                timeOfLastGoodKeypointHomography = Time.time;
                Mat output = new Mat(templateSize, templateType);
                Cv2.WarpPerspective(im, output, homography, templateSize);
                Debug.Log("YOU GOT A WRONG TRANSFORM");
                return output;
            }
        }
        if (timeOfLastGoodKeypointHomography + timeTilAbandonGoodKeypointHomography > Time.time)
        {
            Mat output = new Mat(templateSize, templateType);
            Cv2.WarpPerspective(im, output, keypointLastGoodHomography, templateSize);
            return output;
        }

        return null;
    }

    public virtual bool UpdateParse(Mat newIm) { return true; }

    public bool CheckForOverMotion(Mat lastImage, Mat currentImage)
    {
        throw new System.NotImplementedException();
    }
    

    /**
     * Make an RGB/BGR histogram, useful for image comparisons.
     * TODO : we can use CompareHist to compare histograms and decide what the most likely type/card is
    */
    public Mat MakeRGBHistrogram(Mat im)
    {
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
        Cv2.CalcHist(new Mat[] { hsvIM }, channels, null, hist, 3, histSize, ranges);
        return hist;
    }


    public Mat AttemptTemplateMatching(Mat im, Mat template, TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed)
    {
        Mat output = new Mat();
        Cv2.MatchTemplate(im, template, output, mode);
        return output;
    }

    public void CompareByKeypoints(Mat im, KeyPoint[] kp1)
    {
        // TODO : NO NEED TO TRANSFORM IMAGE WE ALREADY HAVE IT TRANSFORMED, THEORETICALLY.
        // TODO : BUT AN ALT VERSION THAT ALLOWS A TRANSFORM MIGHT BE GOOD FOR LIKE STICKERS???
    }

    /**
     * USED FOR SHAPE MATCHING
     */ 
    public Mat GetTheOnlyShape(Mat im)
    {
        Mat edges = new Mat();
        Cv2.Canny(im, edges, 200, 250);


        Mat[] contours;
        Mat hierarchy = new Mat();
        Cv2.FindContours(edges, out contours, hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);
        if (contours.Length != 1)
            return null;

        return contours[0];
    }



    public double AttemptShapeMatching(Mat im, Mat desiredShape, out Mat bestMat)
    {

        Mat edges = new Mat();
        Cv2.Canny(im, edges, 200, 250);


        Mat[] contours;
        Mat hierarchy = new Mat();
        Cv2.FindContours(edges, out contours, hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxNone);
        Debug.Log("Found " + contours.Length + " contours");


        bestMat = null;
        double bestMatchVal = double.PositiveInfinity;
        foreach (Mat c in contours)
        {
            double matchVal = Cv2.MatchShapes(desiredShape, c, ShapeMatchModes.I1);
            if (matchVal <= bestMatchVal)
            {
                bestMatchVal = matchVal;
                bestMat = c;
            }
        }
        return bestMatchVal;
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
    }

}
