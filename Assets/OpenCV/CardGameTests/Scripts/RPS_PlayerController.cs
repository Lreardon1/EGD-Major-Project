using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RPS_PlayerController : MonoBehaviour
{
    private RPS_PlayManager manager;
    private Dictionary<RPS_Card.CardType, int> myCards;
    private CardParser cardParser;

    public TMP_Text playText;
    public TMP_Text cardTrackText;
    public Image progressIndicator;

    private bool debugMode;

    public void Init(RPS_PlayManager manager, Dictionary<RPS_Card.CardType, int> initCards)
    {
        this.manager = manager;
        myCards = new Dictionary<RPS_Card.CardType, int>(initCards);
        cardParser = FindObjectOfType<CardParser>(); // find the dontdestroyonload card parser...
        debugMode = cardParser == null;
        if (!debugMode)
        {
            cardParser.UpdateMode(CardParser.ParseMode.RPS_Mode);
            cardParser.RPS_StableUpdateEvent.AddListener(HandleStableUpdate);
            cardParser.RPS_ToNewUpdateEvent.AddListener(HandleNewUpdate);
            cardParser.RPS_ToNullUpdateEvent.AddListener(HandleUnknownUpdate);
            // TODO : start up a DeviceCamera

            if (WebCamTexture.devices.Length > 0)
                DeviceName = WebCamTexture.devices[0].name;
        }
        UpdateCardTrackText();
    }

    private void UpdateCardTrackText()
    {
        cardTrackText.text = "<color=#0000FF>" + myCards[RPS_Card.CardType.Water]
            + "   <color=#FF0000>" + myCards[RPS_Card.CardType.Fire] 
            + "   <color=#00FF00>" + myCards[RPS_Card.CardType.Wind];
    }

    private void HandleUnknownUpdate(RPS_Card.CardType cardType)
    {
        print("RPS_: NULL OUT: " + cardType);
        bestVisibleCard = cardType; // TODO : better system than this, maybe a percent based data structure???
    }

    private void HandleNewUpdate(RPS_Card.CardType cardType)
    {
        print("RPS_: NEW: " + cardType);
        bestVisibleCard = cardType;
    }

    private void HandleStableUpdate(RPS_Card.CardType cardType)
    {
        print("RPS_: STABLE: " + cardType);
        bestVisibleCard = cardType;
    }


    private bool ContinueButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
    private RPS_Card.CardType bestVisibleCard = RPS_Card.CardType.Unknown;

    private bool IsShowingValidCard()
    {
        if (debugMode || Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Alpha3))
        {
            bestVisibleCard = RPS_Card.CardType.Unknown;
            if (Input.GetKey(KeyCode.Alpha1))
                bestVisibleCard = RPS_Card.CardType.Water;
            else if (Input.GetKey(KeyCode.Alpha2))
                bestVisibleCard = RPS_Card.CardType.Fire;
            else if (Input.GetKey(KeyCode.Alpha3))
                bestVisibleCard = RPS_Card.CardType.Wind;
            else return false;

            return myCards[bestVisibleCard] > 0;
        }
        else return myCards[bestVisibleCard] > 0 && bestVisibleCard != RPS_Card.CardType.Unknown;
    }

    internal bool HasCard(RPS_Card.CardType ct)
    {
        return myCards[ct] > 0;
    }

    private RPS_Card.CardType GetBestVisibleCard()
    {
        return bestVisibleCard;
    }

    private IEnumerator IHandleBid()
    {
        progressIndicator.fillAmount = 0.0f;
        playText.text = "Place your bid face down on the spacebar enough to press it.";

        yield return new WaitUntil(ContinueButtonPressed);
        playText.text = "";

        RPS_Card card = new RPS_Card(RPS_Card.CardType.Unknown);
        manager.SendCardInContext(card);
    }


    public float timeToShowPlay = 0.7f;

    private IEnumerator IHandlePlay()
    {
        playText.text = "Show a card to play it";
        float t = 0;
        lookingForInput = true;
        while (t < timeToShowPlay)
        {

            progressIndicator.fillAmount = t / timeToShowPlay;
            if (IsShowingValidCard())
            {
                t += Time.deltaTime;
                playText.text = "You are most likely showing " + GetBestVisibleCard() + " to play.";
                manager.playerPlayObj.SetCard(new RPS_Card(GetBestVisibleCard()));
            }
            else
            {
                t = Mathf.Max(t - Time.deltaTime * 3.0f, 0);
            }
            if (t >= timeToShowPlay) break;

            yield return new WaitForEndOfFrame();
        }

        lookingForInput = false;
        RPS_Card card = new RPS_Card(GetBestVisibleCard());
        print("Played " + card.type);
        progressIndicator.fillAmount = 0.0f;
        LoseCard(new RPS_Card(GetBestVisibleCard()));
        playText.text = "";
        // TODO : polish anims and make a card
        manager.SendCardInContext(card);
    }

    public float timeToShowTrade = 0.7f;

    private IEnumerator IHandleTradeDecision()
    {
        playText.text = "Show your bid card to trade it, or keep it by pressing space";
        lookingForInput = true;
        float t = 0;
        while (t < timeToShowTrade)
        {
            if (ContinueButtonPressed()) goto NoTrade;

            if (IsShowingValidCard())
            {
                t += Time.deltaTime;
                playText.text = "You are mostly showing " + GetBestVisibleCard() + " as your bid to trade.";
                progressIndicator.fillAmount = (t / timeToShowTrade);
                manager.playerBidObj.SetCard(new RPS_Card(GetBestVisibleCard()));
            }
            else
            {
                t = Mathf.Max(0, t - Time.deltaTime * 2.0f);
                progressIndicator.fillAmount = (t / timeToShowTrade);
                playText.text = "Show your bid card to trade it, or keep it by pressing space";
            }
            if (t >= timeToShowTrade) break;

            yield return new WaitForEndOfFrame();
        }

        RPS_Card.CardType bc = GetBestVisibleCard();
        lookingForInput = false;
        cardParser.ResetRPS();
        playText.text = "You are trading your " + bc + " card";
        progressIndicator.fillAmount = 0.0f;
        yield return new WaitForSeconds(1.0f);

        manager.SendTradeCardsDecision(true, bc);
        yield break;
    NoTrade:
        lookingForInput = false;
        print("NO TRADE");
        playText.text = "No trade performed, take back your bid.";
        lookingForInput = false;
        manager.SendTradeCardsDecision(false);
    }


    private IEnumerator IHandleRevealCardByEnemyDecision()
    {
        print("MADE IT HERE!!!");
        playText.text = "Your opponent has decided to take your bid, show your bid now...";
        float t = 0;
        lookingForInput = true;
        while (t < timeToShowTrade)
        {
            progressIndicator.fillAmount = t / timeToShowTrade;

            if (IsShowingValidCard())
            {
                t += Time.deltaTime;
                playText.text = "You are mostly showing " + GetBestVisibleCard() + " as you bid to trade.";
                manager.playerBidObj.SetCard(new RPS_Card(GetBestVisibleCard()));
            }
            else
            {
                t = Mathf.Max(t - Time.deltaTime * 2.0f, 0);
            }
            if (t >= timeToShowTrade) break;

            yield return new WaitForEndOfFrame();
        }

        lookingForInput = false;
        playText.text = "";
        progressIndicator.fillAmount = 0.0f;
        manager.SendBidCardAfterDecision(GetBestVisibleCard());
    }

    public void RequestBidReveal()
    {
        StartCoroutine(IHandleRevealCardByEnemyDecision());
    }


    public void GainCard(RPS_Card gained)
    {
        myCards[gained.type] += 1;
        UpdateCardTrackText();
    }

    internal void LoseCard(RPS_Card lost)
    {
        myCards[lost.type] -= 1;
        UpdateCardTrackText();
    }

    internal void RequestBid()
    {
        StartCoroutine(IHandleBid());
    }

    internal void RequestPlay()
    {
        StartCoroutine(IHandlePlay());
    }

    internal void RequestTradeDecision()
    {
        print("HANDLE TRADE DEAL!!!");
        StartCoroutine(IHandleTradeDecision());
    }










    
    protected OpenCvSharp.Unity.TextureConversionParams TextureParameters { get; private set; }
    protected bool forceFrontalCamera = false;
    WebCamTexture webCamTexture;
    WebCamDevice? webCamDevice;
    protected bool lookingForInput = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Q))
        {
            MemorySceneLoader.LoadToOverworld("RPS_CardGame");
            enabled = false;
        }

        if (!lookingForInput) {
            cardParser.ResetRPS();
            return;
        }
        if (webCamTexture == null) return;

        if (!webCamTexture.isPlaying)
        {
            print("Starting play again");
            // webCamTexture.Stop();
            webCamTexture.Play();
        }
        if (webCamTexture != null && webCamTexture.didUpdateThisFrame)
        {
            // this must be called continuously
            ReadTextureConversionParameters();
            cardParser.ProcessTexture(webCamTexture);
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
