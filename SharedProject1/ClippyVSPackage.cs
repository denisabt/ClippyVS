using System;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using SharedProject1;

namespace Recoding.ClippyVSPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "0.4", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideService(typeof(OleMenuCommandService))]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]

    [Guid(Constants.guidClippyVSPkgString)]
    [ProvideOptionPage(typeof(OptionsPage), "Clippy VS", "General", 0, 0, supportsAutomation: true)]

    public sealed class ClippyVisualStudioPackage : AsyncPackage
    {
        public SpriteContainer SpriteContainer { get; private set; }

        /// <summary>
        /// Default ctor
        /// https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/designing-xml-command-table-dot-vsct-files?view=vs-2022
        /// </summary>
        public ClippyVisualStudioPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            try
            {
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
                base.Initialize();

                OleMenuCommandService mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(this.DisposalToken);
                Application.Current.MainWindow.ContentRendered += MainWindow_ContentRendered;

                // Add our command handlers for menu (commands must exist in the .vsct file)

                if (null != mcs)
                {
                    // Create the commands for the menu item.
                    CommandID menuCommandID = new CommandID(Constants.guidClippyVSCmdSet, (int)PkgCmdIDList.cmdShowClippy);
                    MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                    mcs.AddCommand(menuItem);

                    CommandID menuCommand2ID = new CommandID(Constants.guidClippyVSCmdSet, (int)PkgCmdIDList.cmdShowMerlin);
                    MenuCommand menuItem2 = new MenuCommand(MenuItemCallback, menuCommand2ID);
                    mcs.AddCommand(menuItem2);
                }
                await Command1.InitializeAsync(this).ConfigureAwait(true);
                await Command2.InitializeAsync(this);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception !" + e.Message);
            }
        }

        async void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            var token = new CancellationToken();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
            var shellSettingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            IClippyVSSettings settings = new ClippyVSSettings(writableSettingsStore);
            SpriteContainer = new SpriteContainer(this);

            if (settings.ShowAtStartup)
                SpriteContainer.Show();
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            if (SpriteContainer == null)
            {
                SpriteContainer = new SpriteContainer(this);
            }

            //Application.Current.Windows.OfType<SpriteContainer>().First().Show();
            SpriteContainer.Show();

        }
    }
}
