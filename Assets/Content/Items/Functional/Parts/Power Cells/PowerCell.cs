using SS3D.Engine.Inventory;
using Mirror;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SS3D.Content.Items.Functional.Generic.PowerCells
{
    /// <summary>
    /// Handles powercells
    /// </summary>
    // TODO: Put the power cell properties in this file
    // TODO: Make the public variables low case
    public class PowerCell : NetworkBehaviour, IChargeable
    {
        [SerializeField] private PowerCellProperties propertiesPrefab = null;
        [SerializeField] private BlinkingLight redLight;
        [SerializeField] private BlinkingLight orangeLight;
        [SerializeField] private BlinkingLight yellowLight;
        [SerializeField] private BlinkingLight greenLight;
        [SerializeField] private MeshRenderer redMeshRenderer;
        [SerializeField] private MeshRenderer orangeMeshRenderer;
        [SerializeField] private MeshRenderer yellowMeshRenderer;
        [SerializeField] private MeshRenderer greenMeshRenderer;

        // This is used to tell clients which lights should be on.
        [SyncVar(hook = nameof(SyncLightState))] private BatteryLightState lightState;

        // properties of a power cell, maximum charge, recharge rate
        public PowerCellProperties Properties { get; private set; } = null;

        private void Start()
        {
            Properties = Instantiate(propertiesPrefab);
            Properties.name = propertiesPrefab.name;
            UpdateLights(GetPowerPercentage());
        }

        public int GetChargeRate()
        {
            return Properties.PowerSupply.ChargeRate;
        }

        public void AddCharge(int amount)
        {
            int newValue = Mathf.Clamp(Properties.PowerSupply.Charge + amount, 0, Properties.PowerSupply.MaxCharge);
            Properties.PowerSupply = Properties.PowerSupply.WithCharge(newValue);
            UpdateLights(GetPowerPercentage());
        }

        private void Update()
        {
            UpdateLights(GetPowerPercentage());
        }

        public float GetPowerPercentage()
        {
            return Mathf.Round(Properties.PowerSupply.Charge * 100 / Properties.PowerSupply.MaxCharge) / 100;
        }

        /// <summary>
        /// Turn off all lights
        /// </summary>
        private void TurnOffLights()
        {
            redLight.TurnLightOff();
            orangeLight.TurnLightOff();
            yellowLight.TurnLightOff();
            greenLight.TurnLightOff();
            redMeshRenderer.enabled = false;
            greenMeshRenderer.enabled = false;
            yellowMeshRenderer.enabled = false;
            orangeMeshRenderer.enabled = false;
        }

        /// <summary>
        /// Changes lightState variable based on powerPercentage.
        /// Power groups are (0 - .25), (.25 - .50), (.50 - .75), (.75 - 1) 
        /// </summary>
        /// <param name="powerPercentage">Value between 0 - 1</param>
        [Server]
        private void UpdateLights(float powerPercentage)
        {
            if (0 <= powerPercentage && powerPercentage < .10f) // power @ 25%
            {
                SyncLightState(BatteryLightState.NO_BATTERY);
            }
            else if (.10f <= powerPercentage && powerPercentage < .35f) // power @ 50%
            {
                SyncLightState(BatteryLightState.RED);
            }
            else if (.35f <= powerPercentage && powerPercentage < .65f) // power @ 75%
            {
                SyncLightState(BatteryLightState.ORANGE);
            }
            else if (.65f <= powerPercentage && powerPercentage < .90f)
            {
                SyncLightState(BatteryLightState.YELLOW);
            }
            else if (.90f <= powerPercentage)
            {
                SyncLightState(BatteryLightState.BATTERY_FULL);
            }
        }

        [Server]
        /// <summary>
        /// Helper function called by server to change the lightState.
        /// It simply calls the SyncVar hook on the server, which changes
        /// the variable, which in turn causes the SyncVar hook on the 
        /// client to be called.
        /// </summary>
        private void SyncLightState(BatteryLightState newState)
        {
            if (newState != lightState)
            {
                SyncLightState(lightState, newState);
            }
        }

        /// <summary>
        /// Turns on lights based on the lightState variable.
        /// This is a SyncVar hook function. It is *automatically* called on the
        /// client when the lightState variable is changed on the server.
        /// This is the only place that lightState should be set directly.
        /// </summary>
        private void SyncLightState(BatteryLightState oldState, BatteryLightState newState)
        {
            lightState = newState;
            Debug.Log("Lightstate: "+lightState+", Charge: "+GetPowerPercentage());
            switch (lightState)
            {
                case BatteryLightState.NO_BATTERY:
                    TurnOffLights();
                    break;
                case BatteryLightState.RED:
                    redLight.MakeLightStayOn();
                    orangeLight.TurnLightOff();
                    yellowLight.TurnLightOff();
                    greenLight.TurnLightOff();
                    redMeshRenderer.enabled = true;
                    greenMeshRenderer.enabled = false;
                    yellowMeshRenderer.enabled = false;
                    orangeMeshRenderer.enabled = false;
                    break;
                case BatteryLightState.ORANGE:
                    orangeLight.MakeLightStayOn();
                    redLight.TurnLightOff();
                    yellowLight.TurnLightOff();
                    greenLight.TurnLightOff();
                    redMeshRenderer.enabled = false;
                    greenMeshRenderer.enabled = false;
                    yellowMeshRenderer.enabled = false;
                    orangeMeshRenderer.enabled = true;
                    break;
                case BatteryLightState.YELLOW:
                    yellowLight.MakeLightStayOn();
                    redLight.TurnLightOff();
                    orangeLight.TurnLightOff();
                    greenLight.TurnLightOff();
                    redMeshRenderer.enabled = false;
                    greenMeshRenderer.enabled = false;
                    yellowMeshRenderer.enabled = true;
                    orangeMeshRenderer.enabled = false;
                    break;
                case BatteryLightState.BATTERY_FULL:
                    greenLight.MakeLightStayOn();
                    redLight.TurnLightOff();
                    orangeLight.TurnLightOff();
                    yellowLight.TurnLightOff();
                    redMeshRenderer.enabled = false;
                    greenMeshRenderer.enabled = true;
                    yellowMeshRenderer.enabled = false;
                    orangeMeshRenderer.enabled = false;
                    break;
            }
        }

        private enum BatteryLightState
        {
            NO_BATTERY,
            RED,
            ORANGE,
            YELLOW,
            BATTERY_FULL
        }
    }
}