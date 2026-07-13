using UnityEngine;

public class Border : MonoBehaviour
{
    [SerializeField] private string targetTag = "Boss";

    private void OnTriggerEnter(Collider other)
    { 
        
        EventManager.TriggerBorderHit();

            Debug.Log("касание границы");
        if (other.gameObject.CompareTag(targetTag))
        {
           
        }
    }
}