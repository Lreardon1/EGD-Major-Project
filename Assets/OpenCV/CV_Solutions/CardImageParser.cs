using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine;

[System.Serializable]
public class CardImageParser : BaseImageParser
{

    public class ParseableRegion
    {
        public BoundingBox bb;

        public virtual bool PerformRegionParse(Mat region, CardImageParser card) { return true; }

        public class ParseableRegionX : ParseableRegion
        {
            ParseableRegionX t;
            public override bool PerformRegionParse(Mat region, CardImageParser card)
            {
                return base.PerformRegionParse(region, card);
            }
        }
    }

    public static Mat cardTemplate;
    public static KeyPoint[] templateKeypoints;
    public static Mat templateDescriptors;
    public static List<ParseableRegion> parseableRegions = new List<ParseableRegion>();

    public static void InitCardTemplate(Mat template)
    {
        cardTemplate = template;
        GetKeypoints(template, out templateKeypoints, out templateDescriptors);


        // TODO : CREATE PARSEABLE REGIONS
        ParseableRegion pr1 = new ParseableRegion(), pr2 = new ParseableRegion();
        pr1.bb = new BoundingBox(new Point2f(0, 0), new Point2f(0.25f, 0.25f));
        parseableRegions.Add(pr1);
        pr2.bb = new BoundingBox(new Point2f(0.25f, 0.25f), new Point2f(0.75f, 0.75f));
        parseableRegions.Add(pr2);
    }

    // USED FOR SUBCLASSING BUT MAY END UP BEING UNUSED
    public enum CardSuperType
    {
        Normal
    }
    public enum CardType
    {
        
    }

    public enum CardElement
    {

    }

    public static CardImageParser Create(CardSuperType superType)
    {
        return new CardImageParser();
    }

    public int cardID;

    public int cardValueA;
    public float cardValueB;
    public int cardValueC;

    public Mat debugMat;
    public bool shouldDebug = false;

    public override bool UpdateParse(Mat newIm)
    {
        Mat replanedImg;
        if (shouldDebug)
            replanedImg = ReplaneUsingKeypoints(cardTemplate, newIm, ref debugMat, true);
        else
            replanedImg = ReplaneUsingKeypoints(templateKeypoints, templateDescriptors, cardTemplate.Size(), cardTemplate.Type(), newIm);

        // TODO : back of card
        if (replanedImg == null)
            return false;
        
        // IF WE MAKE IT HERE THEN WE KNOW THAT THE CARD IS FACE UP AND CORRECTLY NORMALIZED
        foreach (ParseableRegion pr in parseableRegions)
        {
            OpenCvSharp.Rect rect = pr.bb.GetAABBRect(replanedImg.Size());
            Mat subMat = replanedImg.SubMat(rect);
            bool TODO_doSomethingWithThis = pr.PerformRegionParse(subMat, this);
        }
        // TODO : 
            // feature detection
                // by keypoints + or - homography inliers
       
        return true;
    }
}
