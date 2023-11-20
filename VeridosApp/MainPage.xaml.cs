using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace VeridosApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    

    public sealed partial class MainPage : Page
    {
        private MessageWebSocket webSocket;
        private DataWriter dataWriter;
        public FaceEventData faceData { get; set; } = new FaceEventData();
        public class FaceEventData
        {
            public string Mode { get; set; }
            public bool? isFacePresent { get; set; }
            public bool? isFaceCorrect { get; set; }
            public int? Height { get; set; }
            public int? Width { get; set; }
            public string image_base64 { get; set; }
            public bool? MouthClosed { get; set; }
            public bool? EyesOpen { get; set; }
            public double? EyesOpenDistance { get; set; }
            public double? Roll { get; set; }
            public double? Yaw { get; set; }
            public double? Pitch { get; set; }
            public double? CenterLeft { get; set; }
            public double? CenterTop { get; set; }
            public double? Blur { get; set; }
        }

        public async Task ConnectToWebSocket()
        {
            webSocket = new MessageWebSocket();
            webSocket.Control.MessageType = SocketMessageType.Utf8;
            webSocket.MessageReceived += WebSocket_MessageReceived;

            Uri serverUri = new Uri("ws://192.168.10.85:3000");
            try
            {
                await webSocket.ConnectAsync(serverUri);
                TextBlock1.Text = "Connected To Server";
            }
            catch (Exception ex)
            {
                TextBlock1.Text = ex.ToString();
                // Handle the exception, e.g., log or show an error message
            }
            

            // Initialize DataWriter for sending data if needed.
            dataWriter = new DataWriter(webSocket.OutputStream);
        }

        private void UpdateUI()
        {
            // Update the UI based on the new faceData
            // Assuming you have a property for FaceDataStackPanel in your XAML
            
        }

        public MainPage()
        {
            this.InitializeComponent();
            ConnectToWebSocket();
        }

       


        private async void WebSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                string jsonMessage = reader.ReadString(reader.UnconsumedBufferLength);

                // Parse the JSON message
                var data = JsonConvert.DeserializeObject<FaceEventData>(jsonMessage);
                if(data.isFacePresent == true && data.isFaceCorrect == true)
                {
                    // Extract image_base64 from the data and display it in your image view
                    // Marshal the UI update to the UI thread
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        byte[] imageBytes = Convert.FromBase64String(data.image_base64);
                        using (MemoryStream stream = new MemoryStream(imageBytes))
                        {
                            // Create a BitmapImage and set its source asynchronously
                            BitmapImage image = new BitmapImage();
                            await image.SetSourceAsync(stream.AsRandomAccessStream());

                            // Set the source of your Image control
                            ImageBlocks.Source = image;
                            TextBlock10.Text = "Mode : " + data.Mode.ToString();
                            TextBlock2.Text = "Eyes Open : " + data.EyesOpen.ToString();
                            TextBlock3.Text = "Mouth Closed : " + data.MouthClosed.ToString();
                            TextBlock4.Text = "Height : " + data.Height.ToString();
                            TextBlock5.Text = "Width : " + data.Width.ToString();
                            TextBlock6.Text = "Pitch : " + data.Pitch.ToString();
                            TextBlock7.Text = "Yaw : " + data.Yaw.ToString();
                            TextBlock8.Text = "Roll : " + data.Roll.ToString();
                            TextBlock9.Text = "Blur : " + data.Blur.ToString();
                        }
                    });
                }
            }
        }
    }
}
