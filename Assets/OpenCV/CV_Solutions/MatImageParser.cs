using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;

[System.Serializable]
public class MatImageParser : BaseImageParser
{
    protected ImageParserManager manager;
    protected bool bInit = false;
    public bool bHasMat = false;

    public Texture2D baseImage;
    public Mat baseMatImage;
    public Mat replaneImage;
    protected Mat lastImage;

    public List<CardData> strayedCards { get; private set; } // SUPER VOLATILE, COULD HAVE CARDS IN IT, MIGHT NOT
    public List<CardBoxData> structuredCardBoxes { get; private set; } // STABLE, NO TOUCH AFTER INIT
    public Dictionary<string, CardData> structuredCards { get; private set; } // STABLE, CARD OBJECTS STAY THE SAME, ARE UPDATED TO BE NULL, BACK, FRONT, ETC. EVERY UPDATE
    
    // TODO : update this function (here or in an subclass) to change the names and bounding boxes of structured cards
    // TODO : could also set expiration date on card parsing
    protected virtual void InitCardBBoxes()
    {
        structuredCardBoxes = new List<CardBoxData>();
        // FIRST BOUNDING BOX
        CardBoxData cbd1 = new CardBoxData
        {
            boundingBox = new BoundingBox(new Point2f(0, 0), new Point2f(0.25f, 0.25f)),
            cardName = "Upper Left",
            cardType = CardImageParser.CardSuperType.Normal
        };
        structuredCardBoxes.Add(cbd1);

        // SECOND BOUNDING BOX
        CardBoxData cbd2 = new CardBoxData
        {
            boundingBox = new BoundingBox(new Point2f(0.75f, 0.75f), new Point2f(1, 1)),
            cardName = "Lower Right",
            cardType = CardImageParser.CardSuperType.Normal
        };
        structuredCardBoxes.Add(cbd2);
    }

    // TODO : this is probably not a function that needs modification, if we need more functionality see InitCardBBoxes() first.
    protected virtual void InitCards()
    {
        strayedCards = new List<CardData>();
        structuredCards = new Dictionary<string, CardData>();

        foreach (CardBoxData cbd in structuredCardBoxes)
        {
            CardData cd = new CardData
            {
                card = CardImageParser.Create(cbd.cardType),
                cardPos = Vector2.zero,
                cardRot = 0.0f,
                bb = cbd.boundingBox
            };
            structuredCards.Add(cbd.cardName, cd);
        }
    }

    // CALLED BEFORE ANYTHING ELSE
    public virtual void Initialize(Texture2D baseImage, ImageParserManager m)
    {
        this.baseImage = baseImage;
        manager = m;
        InitCardBBoxes();
        InitCards();

        baseMatImage = OpenCvSharp.Unity.TextureToMat(baseImage); // TODO : texture params are avaiable if needed

        bInit = true;
    }

    protected void FindStrayCards(Mat im, Mat baseIm)
    {
        int rowShift = Mathf.Max(manager.rowShift, 0);
        int colShift = Mathf.Max(manager.colShift, 0);
        

        im.SubMat(new OpenCvSharp.Rect(colShift, rowShift, im.Cols - colShift, im.Rows - rowShift))
            .CopyTo(im.SubMat(new OpenCvSharp.Rect(0, 0, im.Cols - colShift, im.Rows - rowShift)));

        Mat[] imSplit, baseImSplit;
        Cv2.Split(im, out imSplit);
        Cv2.Split(baseIm, out baseImSplit);
        Mat[] finalDiffs = new Mat[imSplit.Length];

        for (int i = 0; i < imSplit.Length; ++i)
        {
            imSplit[i] = imSplit[i].Resize(baseImSplit[i].Size(), 1.0f, 1.0f); // TODO : in a replaned case, this will already be true
            imSplit[i] *= manager.greyOut;

            int s = manager.guassSize;
            Cv2.GaussianBlur(imSplit[i], imSplit[i], new Size(s, s), manager.sigma, manager.sigma);
            Cv2.GaussianBlur(baseImSplit[i], baseImSplit[i], new Size(s, s), manager.sigma, manager.sigma);
            Cv2.EqualizeHist(imSplit[i], imSplit[i]);
            Cv2.EqualizeHist(baseImSplit[i], baseImSplit[i]);

            finalDiffs[i] = new Mat(imSplit[i].Size(), imSplit[i].Type());
            Cv2.Absdiff(imSplit[i], baseImSplit[i], finalDiffs[i]);
        }
        Mat finalDiff = finalDiffs[0] + finalDiffs[1] + finalDiffs[2];

        Cv2.Threshold(finalDiff, finalDiff, manager.testStrayThresh, 255, ThresholdTypes.Binary);
        Cv2.Canny(finalDiff, finalDiff, 150, 255);

        //Cv2.Erode(finalDiff, finalDiff, new Mat(), iterations: manager.dilateAmount);
        //Cv2.Dilate(finalDiff, finalDiff, new Mat(), iterations: manager.dilateAmount);

        Mat[] contours;
        Mat h = new Mat();
        Cv2.FindContours(finalDiff, out contours, h, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
        Debug.Log(contours.Length);
        Mat newIm = new Mat(im.Size(), im.Type());
        im.CopyTo(newIm);
        Cv2.DrawContours(finalDiff, contours, -1, Scalar.Green, 10);

        manager.GiveDebugStray(finalDiff);

        // TODO : expiration system
        // TODO : find random thrown cards system

    }

    public List<Mat> testMats = new List<Mat>();
    public Mat debugAruco;

    public override bool UpdateParse(Mat im)
    {
        if (!bInit)
        {
            Debug.LogError("DID NOT CALL INIT ON MatImageParser BEFORE UPDATE");
            return false;
        }

        FindStrayCards(manager.GetTestStray1(), manager.GetTestStray2());
        // CheckForOverMotion(lastImage, im); // TODO
        lastImage = im;
        testMats.Clear();
        // TODO : resize image if must


        // TODO : many options for replane, most are untested !!!
        replaneImage = ReplaneUsingAruco(new int[] { 1, 2, 3, 4 }, im, baseMatImage.Size(), ref debugAruco, true);
        bHasMat = replaneImage != null;
        if(!bHasMat)
        {
            return false;
        }

        foreach (KeyValuePair <string, CardData> card in structuredCards)
        {
            float t = Time.realtimeSinceStartup;
            OpenCvSharp.Rect aabb = card.Value.bb.GetAABBRect(replaneImage.Size());
            // TODO : current not applying transform, just taking the AABB and cropping, will probably work like magic
            card.Value.card.UpdateParse(replaneImage.SubMat(aabb));
            Debug.Log("Card Time: " + (Time.realtimeSinceStartup - t));

            // TODO : if valid card, append to stray cards
            // TODO : see if you can somehow get the card center pos, scale and rot (scale less important)

            //testMats.Add(replaneImage.SubMat(aabb));
        }

        return true;
    }



    [System.Serializable]
    public struct CardData
    {
        public CardImageParser card;
        public Vector2 cardPos;
        public float cardRot;
        public BoundingBox bb;
    }

    [System.Serializable]
    public struct CardBoxData
    {
        public string cardName;
        public CardImageParser.CardSuperType cardType;
        public BoundingBox boundingBox;
    }

}
