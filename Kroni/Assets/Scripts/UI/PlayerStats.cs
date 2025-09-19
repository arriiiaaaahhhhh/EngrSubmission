using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    // Fields for oxygen stat monitoring
    public int maxLungCapacity;
    public int currentLungCapacity;
    Coroutine LungCapacityCo;

    // Check if players head is under the water
    bool swimCheck;

    [Header("UI")]
    public Slider LungCapacitySlider;

    // Start is called before the first frame update
    void Start()
    {
        // Start at max capacity
        currentLungCapacity = maxLungCapacity;
        LungCapacitySlider.maxValue = maxLungCapacity;
    }

    // Update is called once per frame
    void Update()
    {
        if (!UIBaseClass.menuOpen)
        {
            // Refresh lungs when above surface
            if (!PlayerController.isSwimming && swimCheck == true)
            {
                swimCheck = false;
                StopCoroutine(LungCapacityCo);
                ChangeLungCapacity(currentLungCapacity, maxLungCapacity);
            }
            // Decrease oxygen when below water
            if (PlayerController.isSwimming && swimCheck == false)
            {
                swimCheck = true;
                LungCapacityCo = StartCoroutine(DecreaseLungCapacity(currentLungCapacity, 2, 2));
            }

            // Update Slider
            LungCapacitySlider.value = currentLungCapacity;
        }
    }

    // Slowly decrease remaining oxygen when below surface
    IEnumerator DecreaseLungCapacity(int LungCapacity, int interval, int amount) {
        while (true) { 
            yield return new WaitForSeconds(interval);
            // Ensure that lung capacity doesn't drop beneath 0.
            if (currentLungCapacity > 0) {
                currentLungCapacity = Mathf.Max(currentLungCapacity - amount, 0);
            }
        }
    }

    // Refresh when above water
    public void ChangeLungCapacity(int LungCapacity, int refreshAmount) {
        if (refreshAmount > 0) {
            currentLungCapacity = Mathf.Min(currentLungCapacity + refreshAmount, maxLungCapacity);
        }
        else { currentLungCapacity = Mathf.Max(currentLungCapacity + refreshAmount, 0); }
    }

    // If a player picks up litter, increase the oxygen in their lungs
    public void BonusOxygen() {
        maxLungCapacity += 10;
        currentLungCapacity += 10;
        LungCapacitySlider.maxValue = maxLungCapacity;
    }
}
