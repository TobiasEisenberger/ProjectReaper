using TMPro;
using UnityEngine;

public class UIDeathMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text deathMessage;
    [SerializeField] private TextMeshProUGUI counterReapers;
    [SerializeField] private TextMeshProUGUI counterRunners;

    private void Start()
    {
        deathMessage.enabled = false;
        counterReapers.enabled = false;
        counterRunners.enabled = false;
    }
}