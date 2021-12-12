﻿using Microsoft.VisualStudio.Settings;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// Instance class to represent the ClippyVS user's settings
    /// </summary>
    [Export(typeof(IClippyVSSettings))]
    public class ClippyVSSettings : IClippyVSSettings
    {
        /// <summary>
        /// If true shows clippy at the VS startup
        /// </summary>
        public bool ShowAtStartup { get; set; } = true;

        /// <summary>
        /// String identifier for startup boolean in settings store
        /// </summary>
        private const string ShowAtStartupStoreName = "ShowAtStartup";

        /// <summary>
        /// If true shows clippy at the VS startup
        /// </summary>
        public string SelectedAssistantName { get; set; } = "";

        /// <summary>
        /// String identifier for startup boolean in settings store
        /// </summary>
        private const string SelectedAssistantNameName = "SelectedAssistantName";

        /// <summary>
        /// The real store in which the settings will be saved
        /// </summary>
        private readonly WritableSettingsStore writableSettingsStore;

        #region -- Constructors --

        /// <summary>
        /// Constructor for service injection
        /// </summary>
        /// <param name="vsServiceProvider"></param>
        [ImportingConstructor]
        public ClippyVSSettings(WritableSettingsStore store)
        {
            writableSettingsStore = store;
            LoadSettings();
        }

        #endregion

         /// <summary>
        /// Performs the store of the instance of this interface to the user's settings
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                if (!writableSettingsStore.CollectionExists(Constants.SettingsCollectionPath))
                {
                    writableSettingsStore.CreateCollection(Constants.SettingsCollectionPath);
                }

                writableSettingsStore.SetBoolean(Constants.SettingsCollectionPath, ShowAtStartupStoreName, ShowAtStartup);
                writableSettingsStore.SetString(Constants.SettingsCollectionPath, SelectedAssistantNameName, SelectedAssistantName);
                Debug.WriteLine("Setting stored which is {0}", ShowAtStartup);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Read the stored settings
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // Tries to retrieve the configurations if previously saved
                if (writableSettingsStore.PropertyExists(Constants.SettingsCollectionPath, ShowAtStartupStoreName))
                {
                    ShowAtStartup = writableSettingsStore.GetBoolean(Constants.SettingsCollectionPath, ShowAtStartupStoreName);
                    SelectedAssistantName = writableSettingsStore.GetString(Constants.SettingsCollectionPath, SelectedAssistantNameName);
                    Debug.WriteLine("Setting loaded which is {0} {1}", ShowAtStartup, SelectedAssistantName);
                }
            }
            catch (ArgumentException ex)
            {
                Debug.Fail(ex.Message);
            }
        }
    }
}
