using System.Collections.Generic;
using UnityEngine;
using TMPro;

[AddComponentMenu("Doofus/TimerUI")]
public class TimerUI : MonoBehaviour
{
    public TMP_Text timerText;     // assign in inspector (big bottom-left)
    public string displayFormat = "0.00";
    public bool showCombined = false; // if true show "3.45 | 1.23" when two pulpits

    void Update()
    {
        if (PulpitSpawner.Instance == null || timerText == null)
        {
            if (timerText != null) timerText.text = "";
            return;
        }

        var pulpits = PulpitSpawner.Instance.GetActivePulpitsComponents(); // next method we add
        if (pulpits == null || pulpits.Count == 0)
        {
            timerText.text = "";
            return;
        }

        // gather timers (Pulpit exposes TimerRemaining property)
        List<float> times = new List<float>();
        foreach (var p in pulpits)
        {
            if (p != null) times.Add(p.TimerRemaining);
        }

        if (times.Count == 0) { timerText.text = ""; return; }

        times.Sort();

        if (showCombined && times.Count > 1)
        {
            timerText.text = $"{times[0].ToString(displayFormat)} | {times[1].ToString(displayFormat)}";
        }
        else
        {
            // show the soonest (smallest) time prominently, like reference
            timerText.text = times[0].ToString(displayFormat);
        }
    }
}
