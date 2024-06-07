using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public GUISkin skin;
    public string subtitleText = "";
    public float pauseTime = 1f; // Pauzetijd na elke pauseCharCount karakters
    public int pauseCharCount = 10; // Aantal karakters voordat een pauze wordt ingelast
    private float timer = 0f;
    private int characterIndex = 0;
    private string displayedText = "";
    public bool subtitlesEnabled = true; // Nieuwe variabele om de status van de ondertiteling bij te houden

    void OnGUI()
    {
        GUI.skin = skin;

        // Stel de positie en grootte van de ondertitel in
        Rect subtitleRect = new Rect(0, Screen.height - 100, Screen.width, 100);

        // Weergeef de ondertitel alleen als ondertiteling is ingeschakeld
        if (subtitlesEnabled)
        {
            GUI.Label(subtitleRect, displayedText);
        }
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(subtitleText) && subtitlesEnabled)
        {
            timer += Time.deltaTime;

            if (timer >= 0.5f)
            {
                // Voeg een karakter toe aan de weergegeven tekst
                displayedText = subtitleText.Substring(0, characterIndex);
                characterIndex++;

                timer = 0.1f; //snelheid van inladen karakter

                // Controleer of we een pauze moeten inlassen
                if (characterIndex % pauseCharCount == 0)
                {
                    StartCoroutine(AddPause(1f));
                }

                // Als alle karakters zijn weergegeven, reset de timer en index
                if (characterIndex > subtitleText.Length)
                {
                    timer = 0f;
                    characterIndex = 0;
                }
            }
        }
    }

    // Methode om een nieuwe ondertitel weer te geven
    public void DisplaySubtitle(string text)
    {
        subtitleText = text;
        characterIndex = 0;
        displayedText = "";
        timer = 0f;
    }

    // Coroutine voor het toevoegen van een pauze
    IEnumerator AddPause(float time)
    {
        yield return new WaitForSeconds(time);
    }

    // Methode om de ondertiteling aan of uit te zetten
    public void ToggleSubtitles()
    {
        subtitlesEnabled = !subtitlesEnabled; // Wissel de status van de ondertiteling om
        PlayerPrefs.SetInt("SubtitlesEnabled", subtitlesEnabled ? 1 : 0); // Sla de status op in PlayerPrefs
    }

    // Methode om de status van de ondertiteling te controleren bij het starten van het spel
    void Start()
    {
        // Controleer of de status van de ondertiteling is opgeslagen in PlayerPrefs
        if (PlayerPrefs.HasKey("SubtitlesEnabled"))
        {
            // Haal de opgeslagen waarde op
            int subtitlesStatus = PlayerPrefs.GetInt("SubtitlesEnabled");

            // Zet de status van de ondertiteling op basis van de opgeslagen waarde
            subtitlesEnabled = subtitlesStatus == 1 ? true : false;
        }
    }
}
