using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class ArchardController : MonoBehaviour
{
    [Serializable]
    public class SliderVariation
    {
        public string name;
        public GameObject go;

        public SliderVariation(string name)
        {
            this.name = name;
            this.go = null;
        }

        public SliderVariation(string name, GameObject go)
        {
            this.name = name;
            this.go = go;
        }
    }

    public class SurfaceMaterial
    {
        public string name;
        public float wearCoeffecient;

        public SurfaceMaterial(string name, float wearCoeffecient)
        {
            this.name = name;
            this.wearCoeffecient = wearCoeffecient;
        }
    }

    private static float MPA_TO_PA = 1000000.0f;
    private static float CM_TO_M = 0.001f;
    private static float M3_TO_CM3 = 1.0f / 1000000.0f;


    private const int SLIDER_CIRCLE = 1;
    private const int SLIDER_RECT = 2;
    
    private static string VOLUME_REMOVED_TEXT = "Volume removed in cm^3: ";
    private static string DISTANCE_TRAVELED_TEXT = "Distance traveled in m: ";
    private static string HARDNESS_VALUE_TEXT = "Hardness Value in MPA: ";
    private static string STARTING_VELOCITY_TEXT = "Starting Velocity in m/s: ";
    private static string SLIDER_DIAMETER_TEXT = "Slider Diameter in cm: ";
    private static string ELAPSED_TIME_TEXT = "Elapsed Time: ";
    private static string WEAR_COEFF_TEXT = "Wear Coefficient (k): ";
    private static string PRESSURE_TEXT = "Normal Load (N): ";
    private static string ERROR_TEXT = "Error: ";

    [FormerlySerializedAs("demoGO")] public GameObject demoGo;
    [FormerlySerializedAs("platformGO")] public GameObject platformGo;

    public Button startButton;
    public Button restartButton;

    public TMP_Text errorText;
    
    public TMP_Text velocityText;
    public TMP_Text volumeRemovedText;
    public TMP_Text distanceTraveledText;
    public TMP_Text elapsedTimeText;
    public TMP_Text wearCoeffText;

    public TMP_Text velocitySliderText;
    public TMP_Text hardnessSliderText;
    public TMP_Text sliderDiameterText;
    public TMP_Text pressureText;

    public Slider hardnessSlider;
    public Slider velocitySlider;
    public Slider diameterSlider;
    public Slider pressureSlider;

    public TMP_Dropdown materialDropdown;
    public TMP_Dropdown sliderTypeDropdown;

    public SliderVariation[] sliders;

    // Private
    private bool goodToGo = true;
    private bool demoStarted = false;
    
    private int sliderSelection = 0;

    private float startingVelocity;
    private float sliderVelocity;
    private float sliderDiameter;
    private float materialHardness;
    private float materialWearCoeffecient;
    private float distanceTraveled;
    private float volumeLost;
    private float normalPressure;
    private float sliderArea;
    
    private float startTime;
    private float elapsedTime;

    private Vector3 platformStartPosition;
    private Vector3 demoGoStartingPosition;

    private List<SurfaceMaterial> surfaces = new List<SurfaceMaterial>()
    {
        new SurfaceMaterial("Gold/Gold", 1500.0f / MPA_TO_PA),
        new SurfaceMaterial("Chalk/Slate (charts)", 1.5f / MPA_TO_PA),
        new SurfaceMaterial("Chalk/Slate (empirical)", 0.0272f),
        new SurfaceMaterial("Chalk/Slate (lubed)", 1.0f / (10^7)),
        new SurfaceMaterial("Copper/Steel", 5.0f),
        new SurfaceMaterial("Steel/Copper", 1.7f),
        new SurfaceMaterial("Steel/Steel", 126.0f / (10 ^ 4)),
        new SurfaceMaterial("Iron/Iron (lubed)", 5.0f / (10 ^ 4)),
    };

    void Start()
    {
        ConnectDropdowns();
        ConnectButtons();
        SetupSliderGameObjects();
        ConnectSliders();
        PopulateSurfaces();
        SetInitialValues();
    }

    private void SetInitialValues()
    {
        platformStartPosition = platformGo ? platformGo.transform.position : new Vector3(x: 50.0f, y: 0.0f, z: 0.0f);
        demoGoStartingPosition = demoGo ? demoGo.transform.position : Vector3.zero;
        
        volumeLost = 0.0f;
        sliderArea = 0.0f;
        normalPressure = pressureSlider ? pressureSlider.value : 100.0f;

        HardnessChanged(hardnessSlider.value);
        DiameterChanged(diameterSlider.value);
        VelocityChanged(velocitySlider.value);
        PressureChanged(pressureSlider.value);
        
        UpdateVolumeText();
        SetError(showError: false);
    }

    private void ConnectButtons()
    {
        if (startButton)
            startButton.onClick.AddListener(StartButtonPressed);

        if (restartButton)
            restartButton.onClick.AddListener(RestartButtonPressed);
    }



    private void CalculateSliderArea()
    {
        switch (sliderSelection)
        {
            case SLIDER_RECT:
                sliderArea = Mathf.Pow(sliderDiameter*CM_TO_M, 2.0f);
                break;
            case SLIDER_CIRCLE:
                sliderArea = MathF.PI * Mathf.Pow(((CM_TO_M*sliderDiameter) / 2.0f), 2.0f); // pi r^2
                break;
            
            default:
                sliderArea = 1.0f;
                Debug.LogWarning("Please provide correct slider selection");
                break;
        }
    }
    void StartButtonPressed()
    {
        if (sliderSelection == 0)
        {
            SetError("Must select a slider type!", true);
            Debug.LogError("Must select slider type first!");
            return;  
        }

        SetError("No Error", false);

        DisableControls(false);
        CalculateSliderArea();
        demoStarted = true;
        startTime = Time.time;
    }

    void RestartButtonPressed()
    {
        // set everything back to zero
        // dont reset text but do reset underlying values
        distanceTraveled = 0.0f;
        volumeLost = 0.0f;
        sliderDiameter = 1.0f; // cm
        demoStarted = false;
        UpdateElapsedTime(true);
        DisableControls(isEnabled: true);
        SetError("No Error", false);
        if (platformGo)
            platformGo.transform.position = platformStartPosition;

        if (demoGo)
            demoGo.transform.position = demoGoStartingPosition;
    }

    private void SetupSliderGameObjects()
    {
        foreach (var slider in sliders)
        {
            if (slider.go)
                slider.go.SetActive(false);
        }
    }

    private void ConnectDropdowns()
    {
        materialDropdown.onValueChanged.AddListener(MaterialSelectionChanged);
        sliderTypeDropdown.onValueChanged.AddListener(SliderTypeChanged);
    }

    private void SliderTypeChanged(int value)
    {
        Debug.Log($"New slider value: {value}");
        sliderSelection = value;
        if (value == 0)
            return;

        
        try
        {
            for (int i = 1; i <= sliders.Count(); i++)
            {
                var selected = sliders[i];
                if (value == i && selected.go != null)
                {
                    selected.go.SetActive(true);
                }
                else
                {
                    selected.go.SetActive(false);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void MaterialSelectionChanged(int value)
    {
        try
        {
            if (value >= surfaces.Count) return;
            var selectedSurface = surfaces[value];
            Debug.Log($"Selected new material: {selectedSurface.name}");
            materialWearCoeffecient = selectedSurface.wearCoeffecient;
            if (wearCoeffText)
                wearCoeffText.text = WEAR_COEFF_TEXT + $"{materialWearCoeffecient:F4}";
        }
        catch (Exception e)
        {
            Debug.Log($"Error selecting new material for value: {value} with error: {e.Message}");
        }
    }

    private void ConnectSliders()
    {
        hardnessSlider.onValueChanged.AddListener(HardnessChanged);
        velocitySlider.onValueChanged.AddListener(VelocityChanged);
        diameterSlider.onValueChanged.AddListener(DiameterChanged);
        pressureSlider.onValueChanged.AddListener(PressureChanged);
    }

    private void PressureChanged(float value)
    {
        normalPressure = value;
        UpdatePressureText();
    }

    private void UpdatePressureText()
    {
        if (pressureText)
            pressureText.text = PRESSURE_TEXT + $"{normalPressure}";
    }

    private void HardnessChanged(float value)
    {
        materialHardness = value;
        UpdateHardness();
    }

    private void UpdateHardness()
    {
        if (hardnessSliderText)
        {
            hardnessSliderText.text = HARDNESS_VALUE_TEXT + $"{materialHardness}";
        }
    }

    private void VelocityChanged(float value)
    {
        startingVelocity = value;
        UpdateStartingVelocity();
    }

    private void UpdateStartingVelocity()
    {
        if (velocityText)
        {
            velocitySliderText.text = STARTING_VELOCITY_TEXT + $"{startingVelocity:F1}";
        }
    }

    private void DiameterChanged(float value)
    {
        sliderDiameter = value;
        UpdateDiameter();
    }

    private void UpdateDiameter()
    {
        if (sliderDiameterText)
            sliderDiameterText.text = SLIDER_DIAMETER_TEXT + $"{sliderDiameter:F1}";

        // change slider diameter
        if (demoGo)
        {
            demoGo.transform.localScale = new Vector3(x: sliderDiameter, y: 1.0f, z: sliderDiameter);
        }
}

    private void UpdateVolumeText()
    {
        if (volumeRemovedText)
            
            volumeRemovedText.text = VOLUME_REMOVED_TEXT + volumeLost.ToString("0.##E+0");
    }

    private void PopulateSurfaces()
    {
        if (materialDropdown)
        {
            foreach (SurfaceMaterial option in surfaces)
            {
                materialDropdown.options.Add(new TMP_Dropdown.OptionData()
                {
                    text = option.name
                });
            }

            materialDropdown.value = 0;
        }

        if (sliderTypeDropdown)
        {
            foreach (var slider in sliders)
            {
                sliderTypeDropdown.options.Add(new TMP_Dropdown.OptionData()
                {
                    text = slider.name
                });
            }

            sliderTypeDropdown.value = 0;
        }

    }

    private void UpdateElapsedTime(bool restart = false)
    {
        if (restart)
        {
            var lastRun = elapsedTime;
            elapsedTime = 0.0f;
            elapsedTimeText.text = ELAPSED_TIME_TEXT + elapsedTime.ToString("0.00") + $" (last: {lastRun:F})";
        }
        else
        {
            elapsedTime = Time.time - startTime;
            elapsedTimeText.text = ELAPSED_TIME_TEXT + elapsedTime.ToString("0.00");
        }
    }

    private void UpdateDistanceTraveled()
    {
        if (distanceTraveledText)
        {
            distanceTraveledText.text = DISTANCE_TRAVELED_TEXT + $" {distanceTraveled:F}";
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void UpdateVolumeCalculation()
    {
        // volume = k W L / M where M is hardness
        // check which type of item is being used for WL or pi*r^2
        Debug.Log($"k {materialWearCoeffecient}, area: {sliderArea}, distance: {distanceTraveled}, hardness: {materialHardness}, volume: {volumeLost}");
        // include normal load calculation from pressure
        
        volumeLost = materialWearCoeffecient * normalPressure * distanceTraveled / (materialHardness * MPA_TO_PA);
        volumeLost *= 1000000.0f;
        UpdateVolumeText();
    }

    private void UpdateSliderPosition()
    {
        // calculate height taken off of slider, move slider down that much
        var deltaHeight = M3_TO_CM3 * (- volumeLost / (sliderArea));
        if (demoGo)
            demoGo.transform.position = new Vector3(x: 0.0f, y: demoGoStartingPosition.y + deltaHeight, z: 0.0f);
        
        Debug.Log($"Moving slider down: {deltaHeight} cm with volume lost: {volumeLost}, and area: {sliderArea}");
    }
    
    private void AdvanceTerrain()
    {
        var deltaDistance = startingVelocity * Time.deltaTime;
        if (platformGo)
        {
            distanceTraveled += deltaDistance;
            platformGo.transform.Translate(new Vector3(-deltaDistance, 0, 0));
            UpdateDistanceTraveled();
            UpdateVolumeCalculation();
            UpdateSliderPosition();
        }
    }

    private void DisableControls(bool isEnabled)
    {
        if (diameterSlider)
            diameterSlider.interactable = isEnabled;

        if (velocitySlider)
            velocitySlider.interactable = isEnabled;

        if (hardnessSlider)
            hardnessSlider.interactable = isEnabled;

        if (materialDropdown)
            materialDropdown.interactable = isEnabled;

        if (sliderTypeDropdown)
            sliderTypeDropdown.interactable = isEnabled;

        if (startButton)
            startButton.interactable = isEnabled;

        if (pressureSlider)
            pressureSlider.interactable = isEnabled;
    }

    private void SetError(string msg = "", bool showError = true)
    {
        if (errorText)
        {
            errorText.text = ERROR_TEXT + msg;
            errorText.gameObject.SetActive(showError);
        }
        else
            Debug.LogWarning($"Error description: {msg}");
    }
    
    void Update()
    {
        if (goodToGo && demoStarted)
        {
            UpdateElapsedTime();
            AdvanceTerrain();
        }
    }
}
