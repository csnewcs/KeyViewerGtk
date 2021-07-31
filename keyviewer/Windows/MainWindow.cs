using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Gtk;
using Gdk;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace keyviewer
{
    class MainWindow : Gtk.Window
    {
        Dictionary<string, Gtk.Image> images = new Dictionary<string, Gtk.Image>();
        Dictionary<string, Pixbuf> originImages = new Dictionary<string, Pixbuf>();
        Dictionary<string, Pixbuf> invertedImages = new Dictionary<string, Pixbuf>();
        public MainWindow() : base("test")
        {
            this.Destroyed += delegate { Application.Quit(); };
            SetDefaultSize(256, 256);
            KeyPressEvent += KeyPressed;
            KeyReleaseEvent += KeyReleased;
            this.Name = "toplevel";
            string transportationCss = @"#toplavel {background-color: rgba(0, 255, 255, 0.5)}";
            var styleProvider = new CssProvider();
            styleProvider.LoadFromData(transportationCss);
            StyleContext.AddProvider(styleProvider, 100);

            JArray keys = JArray.Parse(File.ReadAllText("Key.json"));
            foreach (var key in keys)
            {
                images.Add(key.ToString(), new Gtk.Image());
            }
            MakeImages(images.Keys.ToArray());
            Grid imageGrid = new Grid();
            int i = 1;
            foreach (var image in images)
            {
                image.Value.Pixbuf = originImages[image.Key];
                imageGrid.Attach(image.Value, i, 1, 1, 1);
                i++;
            }

            Add(imageGrid);
            ShowAll();
        }
        private void KeyPressed(object sender, KeyPressEventArgs e)
        {
            string key = e.Event.Key.ToString().ToUpper();
            if (images.ContainsKey(key))
            {
                Thread thread = new Thread(() => {
                    Application.Invoke(delegate {
                        images[key].Pixbuf = invertedImages[key];
                    });
                });
                thread.Start();
            }
        }
        private void KeyReleased(object sender, KeyReleaseEventArgs e)
        {
            string key = e.Event.Key.ToString().ToUpper();
            if (images.ContainsKey(key))
            {
                Thread thread = new Thread(() => {
                    Application.Invoke(delegate {
                        images[key].Pixbuf = originImages[key];
                    });
                });
                thread.Start();
            }
        }
        private void MakeImages(string[] keys)
        {
            FontCollection fontCollection = new FontCollection();
            fontCollection.Install("NotoSans-Bold.ttf");
            var a = fontCollection.Find("Noto Sans");
            Font notoSans = a.CreateFont(128, FontStyle.Regular);
            Parallel.ForEach(keys, key => {
                var newImg = SixLabors.ImageSharp.Image.Load("Key Background.png");
                newImg.Mutate(image => {
                    image.DrawText(key.ToString(), notoSans, new SixLabors.ImageSharp.Color(new Rgba32(255, 255, 255, 255)), new SixLabors.ImageSharp.Point(0, 0));
                });
                newImg.Save($"MadeImages/{key}.png");
                originImages.Add(key, new Pixbuf($"MadeImages/{key}.png"));
            });
            Parallel.ForEach(keys, key => {
                var img = SixLabors.ImageSharp.Image.Load($"MadeImages/{key}.png");
                img.Mutate(image => {
                    image.Invert();
                });
                img.Save($"MadeImages/{key}_invert.png");
                invertedImages.Add(key, new Pixbuf($"MadeImages/{key}_invert.png"));
            });
        }
    }
}
