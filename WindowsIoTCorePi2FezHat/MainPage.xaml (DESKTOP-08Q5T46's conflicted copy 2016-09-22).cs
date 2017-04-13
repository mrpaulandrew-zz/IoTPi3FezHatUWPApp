// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WindowsIoTCorePi2FezHat
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Globalization;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using GHIElectronics.UWP.Shields;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        FEZHAT hat;
        DispatcherTimer telemetryTimer;
        DispatcherTimer commandsTimer;

        ConnectTheDotsHelper ctdHelper;
        
        /// <summary>
        /// Main page constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            var deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();

            // Hard coding guid for sensors. Not an issue for this particular application which is meant for testing and demos
            List<ConnectTheDotsSensor> sensors = new List<ConnectTheDotsSensor> {
                new ConnectTheDotsSensor("SensorL01", "Light", "L"),
                new ConnectTheDotsSensor("SensorT01", "Temperature", "C")
            };
            
            //##EDIT
            ctdHelper = new ConnectTheDotsHelper(iotDeviceConnectionString: "HostName=BCSDemo1.azure-devices.net;DeviceId=FrogPi2;SharedAccessKey=dkcwl/vNsLY0eUZs4+DzGhCiL2SKTrvgJUfIrySQszA=",
                organization: "PurpleFrogSystems",
                location: "UK",
                sensorList: sensors);
               }

        private async Task SetupHatAsync()
        {
            this.hat = await FEZHAT.CreateAsync();
            
            this.telemetryTimer = new DispatcherTimer();
            this.telemetryTimer.Interval = TimeSpan.FromSeconds(5);
            this.telemetryTimer.Tick += this.TelemetryTimer_Tick;
            this.telemetryTimer.Start();

            this.commandsTimer = new DispatcherTimer();
            this.commandsTimer.Interval = TimeSpan.FromMilliseconds(1000);
            this.commandsTimer.Tick += this.CommandsTimer_Tick;
            this.commandsTimer.Start();
        }

        private void TelemetryTimer_Tick(object sender, object e)
        {
            // Light Sensor
            ConnectTheDotsSensor lSensor = ctdHelper.sensors.Find(item => item.measurename == "Light");
            lSensor.value = this.hat.GetLightLevel();
            
            this.ctdHelper.SendSensorData(lSensor);
            this.LightTextBox.Text = lSensor.value.ToString("P2", CultureInfo.InvariantCulture);

            this.LightProgress.Value = lSensor.value;

            
            if (this.LightProgress.Value * 100 < 60)
            {
                hat.D2.Color = new FEZHAT.Color(255, 255, 255);
                hat.D3.Color = new FEZHAT.Color(255, 255, 255);
            }
            else
            {
                hat.D2.TurnOff();
                hat.D3.TurnOff();
            }

            // Temperature Sensor
            var tSensor = ctdHelper.sensors.Find(item => item.measurename == "Temperature");
            tSensor.value = this.hat.GetTemperature();
            this.ctdHelper.SendSensorData(tSensor);

            this.TempTextBox.Text = tSensor.value.ToString("N2", CultureInfo.InvariantCulture) + " °C";

       
            System.Diagnostics.Debug.WriteLine("Temperature: {0} °C, Light {1} ", 
                tSensor.value.ToString("N2", CultureInfo.InvariantCulture), 
                lSensor.value.ToString("P2", CultureInfo.InvariantCulture)
                );
        }

        private async void CommandsTimer_Tick(object sender, object e)
        {
            string message = await ctdHelper.ReceiveMessage();

            if (message != string.Empty)
            {
                System.Diagnostics.Debug.WriteLine("Command Received: {0}", message);
                switch (message.ToUpperInvariant())
                {
                    case "D2RED":
                        hat.D2.Color = new FEZHAT.Color(255, 0, 0);
                        break;
                    case "D2GREEN":
                        hat.D2.Color = new FEZHAT.Color(0, 255, 0);
                        break;
                    case "D2BLUE":
                        hat.D2.Color = new FEZHAT.Color(0, 0, 255);
                        break;
                    case "D3RED":
                        hat.D3.Color = new FEZHAT.Color(255, 0, 0);
                        break;
                    case "D3GREEN":
                        hat.D3.Color = new FEZHAT.Color(0, 255, 0);
                        break;
                    case "D3BLUE":
                        hat.D3.Color = new FEZHAT.Color(0, 0, 255);
                        break;
                    case "PURPLEFROG":
                        hat.D2.Color = new FEZHAT.Color(128, 0, 128);
                        hat.D3.Color = new FEZHAT.Color(128, 0, 128);
                        break;
                    case "D2OFF":
                        hat.D2.TurnOff();
                        break;
                    case "D3OFF":
                        hat.D3.TurnOff();
                        break;
                    case "ALLOFF":
                        hat.D2.TurnOff();
                        hat.D3.TurnOff();
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unrecognized command: {0}", message);
                        break;
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize FEZ HAT shield
            await SetupHatAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
