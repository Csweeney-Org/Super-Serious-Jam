using UnityEngine;
using UnityEngine.UI; // Required for the Slider

public class SimpleHealthBarUI : MonoBehaviour
{
    [SerializeField] private SpinCharacterController controller;
    [SerializeField] private Slider healthSlider; 

    private void Update()
    {
        if (controller != null && healthSlider != null)
        {
            // Sliders expect [0,1] -> percentage of health
            healthSlider.value = controller.CurrentToppleHealth / controller.MaxToppleHealth;
        }
    }
}