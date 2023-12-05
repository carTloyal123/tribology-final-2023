using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RampMenuController : MonoBehaviour
{
    private static string MASS_TEXT = "Mass in kg: ";
    private static string VELOCITY_TEXT = "Forward velocity in m/s: ";

    public Slider massSlider;
    public Slider rampAngleSlider;
    
    public TMP_Text rampAngleText;
    public TMP_Text velocityText;
    public TMP_Text massText;
    public TMP_Text massSliderText;
    public GameObject demoGO;
    public Button startButton;
    public Button restartButton;
    
    // Private
    private bool demoStarted = false;

    private Rigidbody sliderBody;
    private Vector3 stopVelocity;
    private Vector3 stopAngularVelocity;
    private bool sliderStopped = true;

    private Vector3 sliderStartPos;
    private Vector3 sliderStartEulers;
    private float rampStartAngle;
    private float sliderMass;

    private float sliderVelocity;
    
    void Start()
    {
        if (demoGO != null)
        {
            sliderBody = demoGO.GetComponentInChildren<Rigidbody>();
            StopSlider();
            UpdateMassText(sliderBody.mass.ToString(CultureInfo.InvariantCulture));

            var transform1 = sliderBody.transform;
            sliderStartPos = transform1.position;
            sliderStartEulers = transform1.eulerAngles;
            rampStartAngle = WrapToRightAngle(demoGO.transform.eulerAngles.z);
            Debug.Log($"Ramp start angle: {rampStartAngle}");
        }
        
        if (rampAngleSlider != null)
            rampAngleSlider.onValueChanged.AddListener(OnSliderValueChange);
        
        if (massSlider)
            massSlider.onValueChanged.AddListener(OnMassSliderChanged);

        if (startButton != null)
            startButton.onClick.AddListener(StartButtonClicked);
        
        if (restartButton)
            restartButton.onClick.AddListener(RestartButtonClicked);
        
    }

    private void OnMassSliderChanged(float value)
    {
        sliderMass = value;
        UpdateMassText(value.ToString(CultureInfo.InvariantCulture));
    }

    private void UpdateMassText(string value)
    {
        if (massText)
            massText.text = MASS_TEXT + value;

        if (massSliderText)
            massSliderText.text = MASS_TEXT + value;
    }

    private void RestartButtonClicked()
    {
        Debug.Log($"Restarting Demo!");
        demoStarted = false;
        DisableControls(isEnabled: true);

        rampAngleSlider.value = rampStartAngle;
        sliderVelocity = 0.0f;
        sliderMass = 1.0f;
        
        if (rampAngleSlider)
        {
            rampAngleSlider.enabled = true;
        }
        
        if (demoGO != null)
        {
            demoGO.transform.eulerAngles = new Vector3(0.0f, 0.0f, -rampStartAngle);
        }
        
        var startText = startButton.gameObject.GetComponentInChildren<TMP_Text>();
        if (startText)
        {
            if (demoStarted)
            {
                startText.text = "Pause Demo!";
            }
            else
            {
                startText.text = "Start Demo!";
            }
        }
        
        if (sliderBody)
        {
            StartSlider();
            var sliderGo = sliderBody.gameObject;
            sliderGo.transform.position = sliderStartPos;
            sliderGo.transform.rotation = Quaternion.Euler(sliderStartEulers);
            sliderBody.velocity = Vector3.zero;
            sliderBody.angularVelocity = Vector3.zero;
            StopSlider();
        }
    }
    
    private void StartButtonClicked()
    {
        Debug.Log($"Start button pressed for slider value: {rampAngleSlider.value}");
        if (sliderBody)
            sliderBody.mass = sliderMass;
        
        if (sliderBody)
            ChangeSliderState();

        DisableControls(false);
        
        demoStarted = !demoStarted;
        
        var startText = startButton.gameObject.GetComponentInChildren<TMP_Text>();
        if (startText)
        {
            if (demoStarted)
            {
                startText.text = "Pause Demo!";
            }
            else
            {
                startText.text = "Start Demo!";
            }
        }
    }

    private void ChangeSliderState()
    {
        if (sliderStopped)
        {
            StartSlider();
        }
        else
        {
            StopSlider();
        }
    }

    private void StopSlider()
    {
        stopVelocity = sliderBody.velocity;
        stopAngularVelocity = sliderBody.angularVelocity;
        sliderBody.isKinematic = true;
        sliderStopped = true;
    }

    private void StartSlider()
    {
        sliderBody.isKinematic = false;
        sliderBody.AddForce( stopVelocity, ForceMode.VelocityChange );
        sliderBody.AddTorque( stopAngularVelocity, ForceMode.VelocityChange );
        sliderStopped = false;
    }
    
    private void OnSliderValueChange(float value)
    {
        // Debug.Log($"Slider value: {rampAngleSlider.value} or {value}");
        if (rampAngleText)
        {
            rampAngleText.text = $"Current Ramp Angle: {value} degrees.";
        }
        
        if (demoGO != null)
        {
            var currentEulers = demoGO.transform.rotation.eulerAngles;
            if (!demoStarted)
            {
                demoGO.transform.rotation = Quaternion.Euler(currentEulers.x, currentEulers.y, -rampAngleSlider.value);
            }
        }
    }

    private void DisableControls(bool isEnabled = true)
    {
        if (massSlider)
            massSlider.interactable = isEnabled;

        if (rampAngleSlider)
            rampAngleSlider.interactable = isEnabled;
    }

    // Update is called once per frame
    void Update()
    {
        // update orientation of demo game object here
        if (demoStarted)
        {
            var currentVel = sliderBody.velocity.magnitude;
            if (sliderVelocity < currentVel)
                sliderVelocity = currentVel;
            if (velocityText)
                velocityText.text = VELOCITY_TEXT + $"{sliderVelocity:F2}";
            
        }
    }
    
    
    // Helpers
    private float WrapToRightAngle(float val)
    {
        var temp = val;
        while (temp >= 90.0f)
        {
            temp -= 90.0f;
        }
        while (temp <= 0.0f)
        {
            temp += 90.0f;
        }
        return temp;
    }
}
