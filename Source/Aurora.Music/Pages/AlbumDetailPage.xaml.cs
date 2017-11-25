﻿using Aurora.Music.Core;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using ExpressionBuilder;
using System;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using EF = ExpressionBuilder.ExpressionFunctions;


namespace Aurora.Music.Pages
{
    public sealed partial class AlbumDetailPage : Page
    {
        private CompositionPropertySet _scrollerPropertySet;
        private Compositor _compositor;
        private CompositionPropertySet _props;

        public AlbumDetailPage()
        {
            this.InitializeComponent();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;

            SystemNavigationManager.GetForCurrentView().BackRequested -= AlbumDetailPage_BackRequested;
            SystemNavigationManager.GetForCurrentView().BackRequested += AlbumDetailPage_BackRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.UnloadObject(this);
        }

        private void AlbumDetailPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.AlbumDetailPageInAnimation + "_1", Title);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate(Consts.AlbumDetailPageInAnimation + "_2", HeaderBG);
            LibraryPage.Current.GoBack();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is AlbumViewModel s)
            {
                Context.HeroImage = s.Artwork;
                await Context.GetSongsAsync(s);
            }
        }

        private void SongList_Loaded(object sender, RoutedEventArgs e)
        {
            var ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumDetailPageInAnimation + "_1");
            if (ani != null)
            {
                ani.TryStart(Title, new UIElement[] { Details });
            }
            ani = ConnectedAnimationService.GetForCurrentView().GetAnimation(Consts.AlbumDetailPageInAnimation + "_2");
            if (ani != null)
            {
                ani.TryStart(HeaderBG);
            }

            var scrollviewer = SongList.GetScrollViewer();
            _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollviewer);
            _compositor = _scrollerPropertySet.Compositor;

            _props = _compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);
            _props.InsertScalar("clampSize", (float)Title.ActualHeight + 40);
            _props.InsertScalar("scaleFactor", 0.5f);

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
            var props = _props.GetReference();
            var progressNode = props.GetScalarProperty("progress");
            var clampSizeNode = props.GetScalarProperty("clampSize");
            var scaleFactorNode = props.GetScalarProperty("scaleFactor");

            // Create and start an ExpressionAnimation to track scroll progress over the desired distance
            ExpressionNode progressAnimation = EF.Clamp(-scrollingProperties.Translation.Y / ((float)Header.Height - clampSizeNode), 0, 1);
            _props.StartAnimation("progress", progressAnimation);

            // Get the backing visual for the header so that its properties can be animated
            Visual headerVisual = ElementCompositionPreview.GetElementVisual(Header);

            // Create and start an ExpressionAnimation to clamp the header's offset to keep it onscreen
            ExpressionNode headerTranslationAnimation = EF.Conditional(progressNode < 1, scrollingProperties.Translation.Y, -(float)Header.Height + (float)Title.ActualHeight + 40);
            headerVisual.StartAnimation("Offset.Y", headerTranslationAnimation);

            //// Create and start an ExpressionAnimation to scale the header during overpan
            //ExpressionNode headerScaleAnimation = EF.Lerp(1, 1.25f, EF.Clamp(scrollingProperties.Translation.Y / 50, 0, 1));
            //headerVisual.StartAnimation("Scale.X", headerScaleAnimation);
            //headerVisual.StartAnimation("Scale.Y", headerScaleAnimation);

            ////Set the header's CenterPoint to ensure the overpan scale looks as desired
            //headerVisual.CenterPoint = new Vector3((float)(Header.ActualWidth / 2), (float)Header.ActualHeight, 0);

            var titleVisual = ElementCompositionPreview.GetElementVisual(Title);
            var titleshrinkVisual = ElementCompositionPreview.GetElementVisual(TitleShrink);
            var fixAnimation = EF.Conditional(progressNode < 1, -scrollingProperties.Translation.Y, (float)Header.Height - ((float)Title.ActualHeight + 40));
            titleVisual.StartAnimation("Offset.Y", fixAnimation);
            titleshrinkVisual.StartAnimation("Offset.Y", fixAnimation);
            var detailsVisual = ElementCompositionPreview.GetElementVisual(Details);
            var opacityAnimation = EF.Clamp(1 - (progressNode * 8), 0, 1);
            detailsVisual.StartAnimation("Opacity", opacityAnimation);

            var headerbgVisual = ElementCompositionPreview.GetElementVisual(HeaderBG);
            var bgBlurVisual = ElementCompositionPreview.GetElementVisual(BGBlur);
            var bgOpacityAnimation = EF.Clamp(1 - progressNode, 0, 1);
            var bgblurOpacityAnimation = EF.Clamp(progressNode, 0, 1);
            titleshrinkVisual.StartAnimation("Opacity", bgblurOpacityAnimation);
            titleVisual.StartAnimation("Opacity", bgOpacityAnimation);
            headerbgVisual.StartAnimation("Opacity", bgOpacityAnimation);
            bgBlurVisual.StartAnimation("Opacity", bgblurOpacityAnimation);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= AlbumDetailPage_BackRequested;
            Context = null;
        }



        private void Grid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
            }
        }

        private void Grid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
            }
        }

        private async void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            await Context.PlayAt((sender as FrameworkElement).DataContext as SongViewModel);
        }
    }
}