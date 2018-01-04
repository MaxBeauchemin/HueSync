using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace HueSync
{
    class Program
    {
        static void Main()
        {
            var client = new HttpClient();

            var baseUrl = ConfigurationManager.AppSettings["PhilipsHueIpAddress"].ToString();
            var userToken = ConfigurationManager.AppSettings["PhilipsHueUserToken"].ToString();
            var lightId = ConfigurationManager.AppSettings["PhilipsHueLightId"].ToString();

            var template = "{0}/api/{1}/lights/{2}/state";

            var fullUrl = String.Format(template, baseUrl, userToken, lightId);

            var body = new HueObj();

            var lastCall = DateTime.Now;

            while (true)
            {
                var bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

                Graphics myGraphics = Graphics.FromImage(bitmap);

                myGraphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                            Screen.PrimaryScreen.Bounds.Y,
                                            0,
                                            0,
                                            Screen.PrimaryScreen.Bounds.Size);

                var offset = 25;

                var pixelCount = 0;
                var r = 0;
                var g = 0;
                var b = 0;
                for(int x = 0; x < Screen.PrimaryScreen.Bounds.Width; x += offset)
                {
                    for (int y = 0; y < Screen.PrimaryScreen.Bounds.Height; y += offset)
                    {
                        var c = bitmap.GetPixel(x, y);

                        r += c.R;
                        g += c.G;
                        b += c.B;
                        pixelCount++;
                    }
                }

                var color = Color.FromArgb(r / pixelCount, g / pixelCount, b / pixelCount);

                var hue = color.GetHue();
                var sat = color.GetSaturation();
                var bri = color.GetBrightness();

                body.hue = (int)((hue/360)* 65535);
                body.sat = (int)(sat*255);
                body.bri = (int)(bri*255);

                System.Threading.Thread.Sleep(100);

                var request = new HttpRequestMessage(HttpMethod.Put, fullUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                };

                var response = client.SendAsync(request);
            }
        }
    }

    class HueObj
    {
        public int hue { get; set; }
        public int sat { get; set; }
        public int bri { get; set; }
    }
}
