﻿using System.Windows;
using System.IO;
using LocationInterface.Utils;
using DatabaseManagerLibrary;
using DatabaseManagerLibrary.CSV;
using Newtonsoft.Json;

namespace LocationInterface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static DataIndex DataIndex { get; protected set; }
        public static ImageIndex ImageIndex { get; protected set; }

        public App()
        {
            ValidateFolders();
            ValidateDatabaseTables();

            DataIndex.SaveIndex();
            ImageIndex.ScanForFiles();
            ImageIndex.SaveIndex();
        }

        /// <summary>
        /// Validate the existance of folders and index files
        /// </summary>
        public void ValidateFolders()
        {
            if (!Directory.Exists(SettingsManager.Active.AnalysisFolder)) Directory.CreateDirectory(SettingsManager.Active.AnalysisFolder);
            if (!Directory.Exists(SettingsManager.Active.ImageFolder)) Directory.CreateDirectory(SettingsManager.Active.ImageFolder);
            if (!Directory.Exists(SettingsManager.Active.DataCacheFolder)) Directory.CreateDirectory(SettingsManager.Active.DataCacheFolder);
            if (!Directory.Exists(SettingsManager.Active.LocationDataFolder)) Directory.CreateDirectory(SettingsManager.Active.LocationDataFolder);
            if (!Directory.Exists(Constants.PLUGINFOLDER)) Directory.CreateDirectory(Constants.PLUGINFOLDER);
            PluginManager.Load();
            try
            {
                if (!File.Exists($"{ SettingsManager.Active.LocationDataFolder }\\index.json") || (DataIndex = DataIndex.LoadIndex()) == null) { File.Create($"{ SettingsManager.Active.LocationDataFolder }\\index.json").Close(); DataIndex = new DataIndex(); }
                else DataIndex.VerifyDataFiles();
            }
            catch (JsonSerializationException)
            {
                File.Create($"{ SettingsManager.Active.LocationDataFolder }\\index.json").Close();
                DataIndex = new DataIndex();
            }
            try
            {
                if (!File.Exists($"{ SettingsManager.Active.ImageFolder }\\index.json") || (ImageIndex = ImageIndex.LoadIndex()) == null) { File.Create($"{ SettingsManager.Active.ImageFolder }\\index.json").Close(); ImageIndex = new ImageIndex(); }
                else ImageIndex.VerifyImageFiles();
            }
            catch (JsonSerializationException)
            {
                File.Create($"{ SettingsManager.Active.ImageFolder }\\index.json").Close();
                ImageIndex = new ImageIndex();
            }
        }

        /// <summary>
        /// Validate the existance of requried database tables
        /// </summary>
        public void ValidateDatabaseTables()
        {
            Database database = new CSVDatabase(SettingsManager.Active.EmailDatabase);
            if (database.GetTable("Contacts") == null) database.CreateTable("Contacts", new CSVTableFields("Name:string,EmailAddress:string"));
            if (database.GetTable("Presets") == null) database.CreateTable("Presets", new CSVTableFields("Name:string,Subject:string,Body:string"));
            database.SaveChanges();
        }
    }
}
