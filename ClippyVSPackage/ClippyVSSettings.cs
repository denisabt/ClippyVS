﻿using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Task = System.Threading.Tasks.Task;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// Instance class to represent the ClippyVS user's settings
    /// </summary>
    /// [Export(typeof(IClippyVSSettings))]
    /// 
    public class ClippyVSSettings : IClippyVSSettings
    {
        /// <summary>
        /// The real store in which the settings will be saved
        /// </summary>
        readonly WritableSettingsStore writableSettingsStore;

        #region -- Constructors --

        /// <summary>
        /// Default ctor
        /// </summary>
        public ClippyVSSettings()
        {

        }

        /// <summary>
        /// Constructor for service injection
        /// </summary>
        /// <param name="vsServiceProvider"></param>
        ///[ImportingConstructor]
        public ClippyVSSettings(System.IServiceProvider vsServiceProvider) : this()
        {
            var shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
            writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            LoadSettings();
        }

        #endregion

        /// <summary>
        /// If true shows clippy at the VS startup
        /// </summary>
        public bool ShowAtStartup { get; set; }

        /// <summary>
        /// Performs the store of the instance of this interface to the user's settings
        /// </summary>
        public void Store()
        {
            try
            {
                if (!writableSettingsStore.CollectionExists(Constants.SettingsCollectionPath))
                {
                    writableSettingsStore.CreateCollection(Constants.SettingsCollectionPath);
                }

                writableSettingsStore.SetString(Constants.SettingsCollectionPath, "ShowAtStartup", this.ShowAtStartup.ToString());
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
            // Default values
            this.ShowAtStartup = true;

            try
            {
                // Tries to retrieve the configurations if previously saved
                if (writableSettingsStore.PropertyExists(Constants.SettingsCollectionPath, "ShowAtStartup"))
                {
                    bool b = this.ShowAtStartup;
                    if (Boolean.TryParse(writableSettingsStore.GetString(Constants.SettingsCollectionPath, "ShowAtStartup"), out b))
                        this.ShowAtStartup = b;
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }
    }
}
