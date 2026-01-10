using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Elements")]
    public GameObject tutorialPanel;
    public TMP_Text instructionText;
    public Image handCursor;
    public GameObject swipeArrow;

    [Header("Animation")]
    public float handAnimSpeed = 1f;
    public Vector2 handStartPos = new Vector2(-2f, 0);
    public Vector2 handEndPos = new Vector2(2f, 0);

    private bool isTutorialActive = false;
    private const string TUTORIAL_KEY = "TutorialCompleted";

    void Start()
    {
        // İlk açılışta tutorial göster
        if (PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 0)
        {
            StartCoroutine(ShowTutorialSequence());
        }
        else
        {
            HideTutorial();
        }
    }

    IEnumerator ShowTutorialSequence()
    {
        isTutorialActive = true;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        // Adım 1: Hoş geldin mesajı
        if (instructionText != null)
        {
            instructionText.text = "XacT'a Hoş Geldin!\n\nŞekilleri parmağınla keserek hedef alana ulaş! ✂️";
        }

        yield return new WaitForSeconds(3f);

        // Adım 2: Nasıl kesilir
        if (instructionText != null)
        {
            instructionText.text = "Şekli kesmek için üzerinden çizgi çek 👇";
        }

        // El animasyonu başlat
        if (handCursor != null)
        {
            StartCoroutine(AnimateHand());
        }

        yield return new WaitForSeconds(4f);

        // Adım 3: Oyun kuralları
        if (instructionText != null)
        {
            instructionText.text = "Hedef yüzdeye ulaşmak için 3 hakkın var!\n\nHazır mısın? 🎯";
        }

        if (handCursor != null)
        {
            handCursor.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(3f);

        // Tutorial'ı tamamla
        CompleteTutorial();
    }

    IEnumerator AnimateHand()
    {
        if (handCursor == null) yield break;

        handCursor.gameObject.SetActive(true);
        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * handAnimSpeed, 1f);
            
            Vector2 pos = Vector2.Lerp(handStartPos, handEndPos, t);
            handCursor.rectTransform.anchoredPosition = pos;

            yield return null;
        }
    }

    void CompleteTutorial()
    {
        PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
        PlayerPrefs.Save();
        HideTutorial();
    }

    void HideTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        if (handCursor != null)
            handCursor.gameObject.SetActive(false);
        
        if (swipeArrow != null)
            swipeArrow.SetActive(false);

        isTutorialActive = false;
    }

    // Public metodlar - UI butonlarından çağrılabilir
    public void SkipTutorial()
    {
        StopAllCoroutines();
        CompleteTutorial();
    }

    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_KEY);
        PlayerPrefs.Save();
        Debug.Log("Tutorial sıfırlandı. Oyunu yeniden başlatın.");
    }

    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }
}