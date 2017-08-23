﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.IO;
using DatabaseManagerLibrary;
using DatabaseManagerLibrary.BIN;
using LocationInterface.Utils;

namespace LocationInterface.Pages
{
    /// <summary>
    /// Interaction logic for MapViewPage.xaml
    /// </summary>
    public partial class MapViewPage : Page
    {
        protected Image CurrentImage { get; set; }
        protected Action ShowHomePage { get; set; }
        protected Database Database { get; set; }
        protected List<Utils.Point> Points { get; set; }
        protected double PointMultiplier { get; set; }
        protected Vector2 PointOffset { get; set; }
        protected static Camera Camera { get; set; }
        public Table[] LoadedTables { get; set; }
        protected bool LoadingPoints { get; private set; }

        public MapViewPage(Action ShowHomePage)
        {
            Database = new BINDatabase("LocationData");
            this.ShowHomePage = ShowHomePage;
            
            LoadedTables = new Table[0];
            Points = new List<Utils.Point>();
            LoadingPoints = false;
            PointOffset = new Vector2(1);
            PointMultiplier = 1;
            Camera = new Camera();

            InitializeComponent();

            for (int i = 0; i < 11; i++) ((ComboBoxItem)deckSelectionComboBox.Items[i]).IsEnabled = File.Exists(string.Format("Images\\Deck{0}.bmp", i + 1));

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Dispatcher.Invoke(Update);
                        Thread.Sleep((int)(1000 / 30));
                    }
                    catch (TaskCanceledException) { }
                }
            });
        }

        public void SetTables(LocationDataFile[] dataFiles)
        {
            LoadedTables = new Table[dataFiles.Length];
            for (int i = 0; i < dataFiles.Length; i++)
                LoadedTables[i] = Database.GetTable(dataFiles[i].TableName);
        }

        protected void Update()
        {
            bool shiftDown = Keyboard.IsKeyDown(Key.LeftShift);
            if (Keyboard.IsKeyDown(Key.A)) Camera.Move(5, 0);
            if (Keyboard.IsKeyDown(Key.D)) Camera.Move(-5, 0);
            if (Keyboard.IsKeyDown(Key.W)) Camera.Move(0, 5);
            if (Keyboard.IsKeyDown(Key.S)) Camera.Move(0, -5);
            if (Keyboard.IsKeyDown(Key.R)) Scale(shiftDown ? -.05 : -.001);
            if (Keyboard.IsKeyDown(Key.Y)) Scale(shiftDown ? +.05 : +.001);
            if (Keyboard.IsKeyDown(Key.T)) Translate(0, shiftDown ? -4 : -1);
            if (Keyboard.IsKeyDown(Key.F)) Translate(shiftDown ? -4 : -1, 0);
            if (Keyboard.IsKeyDown(Key.G)) Translate(0, shiftDown ? +4 : +1);
            if (Keyboard.IsKeyDown(Key.H)) Translate(shiftDown ? +4 : +1, 0);

            if (!LoadingPoints) foreach (Utils.Point point in Points) point.Update(Camera, PointOffset, PointMultiplier);

            if (CurrentImage != null) Canvas.SetLeft(CurrentImage, Camera.Position.X);
            if (CurrentImage != null) Canvas.SetTop(CurrentImage, Camera.Position.Y);
        }

        protected void Scale(double change)
        {
            PointMultiplier += change;
        }
        protected void Translate(double x, double y)
        {
            PointOffset += new Vector2(x, y);
        }

        public void LoadTables()
        {
            int currentDeck = deckSelectionComboBox.SelectedIndex + 1;
            string macAddress = macAddressEntry.Text;
            Task.Run(() => LoadTables(currentDeck, macAddress));
        }
        private void LoadTables(int deckNumber, string macAddress)
        {
            try
            {
                // Load Points
                Stopwatch timer = Stopwatch.StartNew();
                LoadingPoints = true;
                Points = new List<Utils.Point>();
                Console.WriteLine("Loading tables...");
                foreach (Table table in LoadedTables)
                {
                    Stopwatch tableTimer = Stopwatch.StartNew();
                    Record[] records = table.GetRecords("MAC", macAddress);
                    tableTimer.Stop();
                    Console.WriteLine("Loaded {0} of {1} records in {2:0.000} seconds.", records.Length, table.RecordCount, tableTimer.ElapsedMilliseconds / 1000d);
                    for (int i = 0; i < records.Length; i++)
                        if (records[i].GetValue<string>("Deck") == string.Format("Deck{0}", deckNumber))
                            Points.Add(new Utils.Point(records[i].GetValue<double>("X"), records[i].GetValue<double>("Y")));
                }
                timer.Stop();
                Console.WriteLine("Added {0} points from {1} table(s) in {2:0.000} seconds.", Points.Count, LoadedTables.Length, timer.ElapsedMilliseconds / 1000d);

                Dispatcher.Invoke(delegate
                {
                    ImageSource image = new BitmapImage(new Uri(string.Format("Images\\Deck{0}.bmp", deckNumber), UriKind.Relative));
                    CurrentImage = new Image
                    {
                        Width = image.Width,
                        Height = image.Height,
                        Name = "DeckImage",
                        Source = image,
                    };
                    canvas.Children.Clear();
                    canvas.Children.Add(CurrentImage);
                    Canvas.SetLeft(CurrentImage, Camera.Position.X);
                    Canvas.SetTop(CurrentImage, Camera.Position.Y);

                    foreach (Utils.Point point in Points)
                    {
                        point.SetEllipse(new Ellipse() { Width = 5, Height = 5, Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00)) });
                        canvas.Children.Add(point.Ellipse);
                    }
                    Console.WriteLine("Added {0} points to the canvas.", Points.Count);
                });
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Deck image file could not be found.", "File Not Found");
            }
            finally
            {
                LoadingPoints = false;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ShowHomePage();
        }
        private void MacAddressEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (deckSelectionComboBox != null && macAddressEntry.Text.Length == 17)
                LoadTables();
        }
        private void DeckSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadTables();
        }
    }
}
