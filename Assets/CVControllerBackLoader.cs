using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CVControllerBackLoader : MonoBehaviour
{
    public RawImage planeImage;
    public RawImage goodSeeImage;
    public RawImage stickerImage1;
    public RawImage stickerImage2;
    public RawImage stickerImage3;

    public TMP_Text playText;
    public TMP_Text cardText;

    // Start is called before the first frame update
    void Start()
    {
        CardParserManager.instance.ActivateCVForCombat(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
