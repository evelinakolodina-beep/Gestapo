using UnityEngine;

public class GroupTriggerHandler : MonoBehaviour
{
    private LetterGroup group;
    private LetterCombatManager combatManager;

    public void Initialize(LetterGroup group, LetterCombatManager combatManager)
    {
        this.group = group;
        this.combatManager = combatManager;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            combatManager.OnPlayerEnterTrigger(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            combatManager.OnPlayerExitTrigger(other.transform);
        }
    }
}