﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Recoding.ClippyVSPackage.Configurations;
using SharedProject1.AssistImpl;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// Interaction logic for SpriteContainer.xaml
    /// </summary>
    public partial class SpriteContainer
    {
        /// <summary>
        /// The instance of Clippy, our hero
        /// </summary>
        private Clippy Clippy { get; set; }
        private Merlin Merlin { get; set; }
        private Genius Genius { get; set; }
        private bool _showMerlin;
        private bool _showGenius;

        /// <summary>
        /// This VSIX package
        /// </summary>
        private readonly AsyncPackage _package;

        /// <summary>
        /// The settings store of this package, to save preferences about the extension
        /// </summary>
        private readonly WritableSettingsStore _userSettingsStore;

        private double RelativeLeft { get; set; }

        private double RelativeTop { get; set; }

        private readonly DocumentEvents _docEvents;
        private readonly BuildEvents _buildEvents;
        private readonly FindEvents _findEvents;
        private ProjectItemsEvents _projectItemsEvents;
        private ProjectItemsEvents _csharpProjectItemsEvents;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="package">The Shell Package</param>
        /// <param name="showMerlin">Indicates if Merlin should be shown - get rid of this soon</param>
        /// <param name="showGenius">Indicates if Genius should be shown - dito</param>
        public SpriteContainer(AsyncPackage package, bool showMerlin = false, bool showGenius = false)
        {
            this._package = package;
            _showMerlin = showMerlin;
            _showGenius = showGenius;

            SettingsManager settingsManager = new ShellSettingsManager(this._package);
            _userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            InitializeComponent();

            this.Owner = Application.Current.MainWindow;
            this.Topmost = false;

            #region Register event handlers

            ThreadHelper.ThrowIfNotOnUIThread();
            //IVsActivityLog activityLog = package.GetServiceAsync(typeof(SVsActivityLog))
            //    .ConfigureAwait(true).GetAwaiter().GetResult() as IVsActivityLog;
            //if (activityLog == null) return;
            //System.Windows.Forms.MessageBox.Show("Found the activity log service.");
            var dte = (EnvDTE.DTE)package.GetServiceAsync(typeof(EnvDTE.DTE)).ConfigureAwait(true).GetAwaiter().GetResult();
            _docEvents = dte.Events.DocumentEvents;
            _buildEvents = dte.Events.BuildEvents;
            _findEvents = dte.Events.FindEvents;
            //_projectItemsEvents = dte.Events.Proj

            RegisterToDteEvents(dte);

            this.Owner.LocationChanged += Owner_LocationChanged;
            this.Owner.StateChanged += Owner_StateOrSizeChanged;
            this.Owner.SizeChanged += Owner_StateOrSizeChanged;

            this.LocationChanged += SpriteContainer_LocationChanged;

            #endregion

            #region -- Restore Sprite postion --

            double? storedRelativeTop = null;
            double? storedRelativeLeft = null;

            double relativeTop;
            double relativeLeft;

            try
            {
                if (_userSettingsStore.PropertyExists(Constants.SettingsCollectionPath, nameof(RelativeTop)))
                    storedRelativeTop = Double.Parse(_userSettingsStore.GetString(Constants.SettingsCollectionPath, nameof(RelativeTop)));

                if (_userSettingsStore.PropertyExists(Constants.SettingsCollectionPath, nameof(RelativeLeft)))
                    storedRelativeLeft = Double.Parse(_userSettingsStore.GetString(Constants.SettingsCollectionPath, nameof(RelativeLeft)));
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($@"SettingsStore Exception while reading: {ex.Message}");
            }

            if (!storedRelativeTop.HasValue || !storedRelativeLeft.HasValue)
            {
                RecalculateSpritePosition(out relativeTop, out relativeLeft, true);
                storedRelativeTop = relativeTop;
                storedRelativeLeft = relativeLeft;

                this.RelativeLeft = relativeLeft;
                this.RelativeTop = relativeTop;

                StoreRelativeSpritePosition(storedRelativeTop.Value, storedRelativeLeft.Value);
            }
            else
            {
                RecalculateSpritePosition(out relativeTop, out relativeLeft);

                this.RelativeLeft = relativeLeft;
                this.RelativeTop = relativeTop;
            }

            double ownerTop = this.Owner.Top;
            double ownerLeft = this.Owner.Left;

            if (this.Owner.WindowState == WindowState.Maximized)
            {
                ownerTop = 0;
                ownerLeft = 0;
            }

            this.Top = ownerTop + storedRelativeTop.Value;
            this.Left = ownerLeft + storedRelativeLeft.Value;

            #endregion


            //// /TEMP

            if (_showMerlin)
                ReviveMerlin();
            else if (_showGenius)
                ReviveGenius();
            else
                ReviveClippy();
        }

        private void PopulateContextMenu()
        {
            var values = Enum.GetValues(typeof(ClippyAnimation));
            if (_showMerlin)
            {
                values = Enum.GetValues(typeof(MerlinAnimations));
            }
            if (_showGenius)
            {
                values = Enum.GetValues(typeof(GeniusAnimations));
            }
            //// TEMP: create a voice for each animation in the context menu
#if DEBUG
            var pMenu = (ContextMenu)this.Resources["CmButton"];
            pMenu.Items.Clear();

            foreach (var val in values)
            {
                var menuItem = new MenuItem()
                {
                    Header = val.ToString(),
                    Name = "cmd" + val
                };
                menuItem.Click += cmdTestAnimation_Click;
                pMenu.Items.Add(menuItem);
            }
#endif

        }

        public void ReviveClippy()
        {
            if (Merlin != null)
            {
                Merlin.Dispose();
                Merlin = null;
            }
            _showMerlin = false;
            _showGenius = false;

            ClippySpriteContainer.Width = 124;
            ClippySpriteContainer.Height = 93;
            ClippyGrid.Width = 124;
            ClippyGrid.Height = 93;
            AssistantCanvasOverlay0.Height = 93;
            AssistantCanvasOverlay1.Visibility = Visibility.Hidden;

            Clippy = new Clippy((Canvas)FindName("AssistantCanvasOverlay0"));
            Clippy.StartAnimation(ClippyAnimation.Greeting);

            PopulateContextMenu();
        }

        public void ReviveMerlin()
        {
            if (Clippy != null)
            {
                Clippy.Dispose();
                Clippy = null;
            }

            _showMerlin = true;
            _showGenius = false;
            this.Width = 128;
            this.Height = 128;
            ClippyGrid.Width = 150;
            ClippyGrid.Height = 150;
            AssistantCanvasOverlay0.Height = 150;
            AssistantCanvasOverlay1.Visibility = Visibility.Hidden;

            Merlin = new Merlin((Canvas)this.FindName("AssistantCanvasOverlay0"));
            Merlin.StartAnimation(MerlinAnimations.Greet);

            PopulateContextMenu();
        }

        public void ReviveGenius()
        {
            if (Clippy != null)
            {
                Clippy.Dispose();
                Clippy = null;
            }

            _showGenius = true;
            _showMerlin = false;
            this.Width = 124;
            this.Height = 93;
            ClippyGrid.Width = 124;
            ClippyGrid.Height = 93;
            AssistantCanvasOverlay0.Height = 93;

            // Genius has to layers, thus overlay 0 and 1 need to be passed for this one.
            Genius = new Genius((Canvas)FindName("AssistantCanvasOverlay0"), (Canvas)FindName("AssistantCanvasOverlay1"));
            Genius.StartAnimation(GeniusAnimations.Greeting);

            PopulateContextMenu();
        }

        private void RegisterToDteEvents(EnvDTE.DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _docEvents.DocumentOpening += DocumentEvents_DocumentOpening;
            _docEvents.DocumentSaved += DocumentEvents_DocumentSaved;
            _docEvents.DocumentClosing += DocEvents_DocumentClosing;

            _buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            _buildEvents.OnBuildDone += BuildEvents_OnBuildDone;

            _findEvents.FindDone += FindEventsClass_FindDone;
            try
            {
                if (_projectItemsEvents is Events2 events2)
                {
                    this._projectItemsEvents = events2.ProjectItemsEvents;
                    this._projectItemsEvents.ItemAdded += ProjectItemsEvents_ItemAdded;
                    this._projectItemsEvents.ItemRemoved += ProjectItemsEvents_ItemRemoved;
                    this._projectItemsEvents.ItemRenamed += ProjectItemsEvents_ItemRenamed;
                }

                this._csharpProjectItemsEvents = dte.Events.GetObject("CSharpProjectItemsEvents") as ProjectItemsEvents;
                if (this._csharpProjectItemsEvents == null)
                    return;

                this._csharpProjectItemsEvents.ItemAdded += ProjectItemsEvents_ItemAdded;
                this._csharpProjectItemsEvents.ItemRemoved += ProjectItemsEvents_ItemRemoved;
                this._csharpProjectItemsEvents.ItemRenamed += ProjectItemsEvents_ItemRenamed;
            }
            catch (Exception exev)
            {
                Debug.WriteLine("Events binding failure");
            }
        }

        #region -- IDE Event Handlers --

        private void ProjectItemsEvents_ItemRenamed(ProjectItem projectItem, string oldName)
        {
            if (_showMerlin)
                Merlin.StartAnimation(MerlinAnimations.Writing, true);
            else if (_showGenius)
                Genius.StartAnimation(GeniusAnimations.Greeting);
            else
                Clippy.StartAnimation(ClippyAnimation.Writing, true);
        }

        private void ProjectItemsEvents_ItemRemoved(ProjectItem projectItem)
        {
            if (_showMerlin)
                Merlin.StartAnimation(MerlinAnimations.DoMagic2, true);
            else if (_showGenius)
                Genius.StartAnimation(GeniusAnimations.EmptyTrash);
            else
                Clippy.StartAnimation(ClippyAnimation.EmptyTrash, true);
        }

        private void ProjectItemsEvents_ItemAdded(ProjectItem projectItem)
        {
            if (_showMerlin)
                Merlin.StartAnimation(MerlinAnimations.Congratulate, true);
            else if (_showGenius)
                Genius.StartAnimation(GeniusAnimations.Congratulate);
            else
                Clippy.StartAnimation(ClippyAnimation.Congratulate, true);

        }

        private void FindEventsClass_FindDone(vsFindResult result, bool cancelled)
        {
            if (_showMerlin)
                Merlin.StartAnimation(MerlinAnimations.Searching, true);

            else if (_showGenius)
                Genius.StartAnimation(GeniusAnimations.Searching);
            else
                Clippy.StartAnimation(ClippyAnimation.Searching, true);

        }

        private void BuildEvents_OnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            if (_showMerlin)
                Merlin.StartAnimation(MerlinAnimations.Congratulate, true);

            else if (_showGenius)
                Genius.StartAnimation(GeniusAnimations.Congratulate);
            else
                Clippy.StartAnimation(ClippyAnimation.Congratulate, true);

        }

        private void DocEvents_DocumentClosing(Document document)
        {
            if (_showMerlin)
                Merlin.StartAnimation(MerlinAnimations.GestureDown, true);

            else if (_showGenius)
                Genius.StartAnimation(GeniusAnimations.GestureDown);
            else
                Clippy.StartAnimation(ClippyAnimation.GestureDown, true);

        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            if (_showMerlin)
                Merlin.StartAnimation(MerlinAnimations.Processing, true); // GetTechy

            else if (_showGenius)
                Genius.StartAnimation(GeniusAnimations.GetTechy);
            else
                Clippy.StartAnimation(ClippyAnimation.Processing, true); // GetTechy

        }

        private void DocumentEvents_DocumentSaved(Document document)
        {
            if (_showMerlin)
                Merlin.StartAnimation(MerlinAnimations.Congratulate2, true);
            else if (_showGenius)
                Genius.StartAnimation(GeniusAnimations.Save);
            else
                Clippy.StartAnimation(ClippyAnimation.Save, true);

        }

        private void DocumentEvents_DocumentOpening(string documentPath, bool readOnly)
        {

            if (_showMerlin)
                Merlin.StartAnimation(MerlinAnimations.LookUp);
            else if (_showGenius)
                Genius.StartAnimation(GeniusAnimations.LookUp);
            else
                Clippy.StartAnimation(ClippyAnimation.LookUp);

        }

        #endregion

        #region -- Owner Event Handlers --

        private void Owner_StateOrSizeChanged(object sender, EventArgs e)
        {
            RecalculateSpritePosition(out _, out _);
        }

        private void Owner_LocationChanged(object sender, EventArgs e)
        {
            // Detach the LocationChanged event of the Sprite Window to avoid recursing relative positions calculation... 
            // TODO: find a better approach
            this.LocationChanged -= SpriteContainer_LocationChanged;

            var ownerTop = this.Owner.Top;
            var ownerLeft = this.Owner.Left;

            if (this.Owner.WindowState == WindowState.Maximized)
            {
                ownerTop = 0;
                ownerLeft = 0;
            }

            this.Top = ownerTop + this.RelativeTop;
            this.Left = ownerLeft + this.RelativeLeft;

            // Reattach the location changed event for the Clippy Sprite Window
            this.LocationChanged += SpriteContainer_LocationChanged;
        }

        #endregion

        #region -- ClippyWindow Event Handlers --

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.FindResource("CmButton") is ContextMenu cm)
            {
                cm.PlacementTarget = sender as Button;
                cm.IsOpen = true;
            }
        }

        private async void cmdClose_Click(object sender, RoutedEventArgs e)
        {
            var window = this;

            if (_showMerlin)
                await Merlin.StartAnimationAsync(MerlinAnimations.Wave, true);
            else if (_showGenius)
                await Genius.StartAnimationAsync(GeniusAnimations.Goodbye);
            else
                await Clippy.StartAnimationAsync(ClippyAnimation.GoodBye, true);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_package.DisposalToken);
            window.Owner.Focus();
            window.Close();
        }

        private void cmdRandom_Click(object sender, RoutedEventArgs e)
        {
            Random rmd = new Random();
            if (!_showMerlin)
            {
                var randomInt = rmd.Next(0, Clippy.AllAnimations.Count);

                Clippy.StartAnimation(Clippy.AllAnimations[randomInt]);
            }
            else if (_showGenius)
            {
                var randomInt = rmd.Next(0, Genius.AllAnimations.Count);

                Genius.StartAnimation(Genius.AllAnimations[randomInt]);
            }
            else
            {
                var randomInt = rmd.Next(0, Merlin.AllAnimations.Count);

                Merlin.StartAnimation(Merlin.AllAnimations[randomInt]);
            }
        }

        private void cmdTestAnimation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_showMerlin)
                {

                    var merlinAnimation = (MerlinAnimations)Enum.Parse(typeof(MerlinAnimations),
                        ((MenuItem)sender).Header.ToString());
                    Merlin.StartAnimation(merlinAnimation, true);

                }
                else if (_showGenius)
                {
                    var geniusAnimation = (GeniusAnimations)Enum.Parse(typeof(GeniusAnimations),
                            ((MenuItem)sender).Header.ToString());
                    Genius.StartAnimation(geniusAnimation, true);
                }
                else
                {
                    var menuItem = sender as MenuItem;
                    if (menuItem == null) return;

                    var animation = (ClippyAnimation)Enum.Parse(typeof(ClippyAnimation), menuItem.Header.ToString());
                    Clippy.StartAnimation(animation, true);
                }
            }
            catch (Exception)
            {
                // NOP
            }
        }

        private void ClippySpriteContainer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Hide();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        private void ClippySpriteContainer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                if (_showMerlin)
                {
                    Merlin.StartAnimation(MerlinAnimations.Idle1_1, true);
                }
                else if (_showGenius)
                {
                    Genius.StartAnimation(GeniusAnimations.Idle1, true);
                }
                else
                {
                    Clippy.StartAnimation(ClippyAnimation.Idle11, true);
                }
            }
        }

        private void SpriteContainer_LocationChanged(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("Parent {0} {1}", this.Owner.Top, this.Owner.Left));
            //System.Diagnostics.Debug.WriteLine(String.Format("Child {0} {1}", this.Top, this.Left));

            RecalculateSpritePosition(out var relativeTop, out var relativeLeft);

            this.RelativeLeft = relativeLeft;
            this.RelativeTop = relativeTop;

            try
            {
                StoreRelativeSpritePosition(relativeTop, relativeLeft);
            }
            catch
            {
                //NOP
            }
        }

        #endregion

        private void RecalculateSpritePosition(out double relativeTop, out double relativeLeft, bool getDefaultPositioning = false)
        {
            // Managing multi-screen scenarios
            var ownerScreen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this.Owner).Handle);
            double leftBoundScreenCorrection = ownerScreen.Bounds.X;
            double topBoundScreenCorrection = ownerScreen.Bounds.Y;

            double ownerTop = this.Owner.Top;
            double ownerLeft = this.Owner.Left;

            if (this.Owner.WindowState == WindowState.Maximized)
            {
                ownerTop = 0 + topBoundScreenCorrection;
                ownerLeft = 0 + leftBoundScreenCorrection;
            }

            double ownerRight = this.Owner.ActualWidth + ownerLeft;
            double ownerBottom = this.Owner.ActualHeight + ownerTop;

            if (ownerTop > this.Top)
                this.Top = ownerTop;

            if (ownerLeft > this.Left)
                this.Left = ownerLeft;

            if (this.Left + this.ActualWidth > ownerRight)
                this.Left = ownerRight - this.ActualWidth;

            if (this.Top + this.ActualHeight > ownerBottom)
                this.Top = ownerBottom - this.ActualHeight;

            if (!getDefaultPositioning)
            {
                relativeTop = this.Top - ownerTop;
                relativeLeft = this.Left - ownerLeft;
            }
            else
            {
                if (_showMerlin)
                {
                    relativeTop = ownerBottom - (Merlin.ClipHeight + 100);
                    relativeLeft = ownerRight - (Merlin.ClipWidth + 100);

                }
                else if (_showGenius)
                {
                    relativeTop = ownerBottom - (Genius.ClipHeight + 100);
                    relativeLeft = ownerRight - (Genius.ClipWidth + 100);
                }
                else
                {
                    relativeTop = ownerBottom - (Clippy.ClipHeight + 100);
                    relativeLeft = ownerRight - (Clippy.ClipWidth + 100);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeTop"></param>
        /// <param name="relativeLeft"></param>
        /// <exception cref="ArgumentException"></exception>
        private void StoreRelativeSpritePosition(double relativeTop, double relativeLeft)
        {

            if (!_userSettingsStore.CollectionExists(Constants.SettingsCollectionPath))
            {
                _userSettingsStore.CreateCollection(Constants.SettingsCollectionPath);
            }

            _userSettingsStore.SetString(Constants.SettingsCollectionPath, nameof(RelativeTop), relativeTop.ToString(CultureInfo.InvariantCulture));
            _userSettingsStore.SetString(Constants.SettingsCollectionPath, nameof(RelativeLeft), relativeLeft.ToString(CultureInfo.InvariantCulture));
        }
    }
}

