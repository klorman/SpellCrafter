using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using System.Runtime.InteropServices;
using ReactiveUI;

namespace SpellCrafter.Controls
{
    public class MetroWindow : Window
    {
        public MetroWindow() : base()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // do this in code or we get a delay in osx.
                SystemDecorations = SystemDecorations.None;
                ClientDecorations = true;
            }
            else
            {
 				SystemDecorations = SystemDecorations.Full;
                ClientDecorations = false;

 				// This will need implementing properly once this is supported by avalonia itself.
 				//var color = (ColorTheme.CurrentTheme.Background as SolidColorBrush).Color;
 				//(PlatformImpl as Avalonia.Native.WindowImpl).SetTitleBarColor(color);
 			}
        }

        public static readonly StyledProperty<Control> TitleBarContentProperty =
            AvaloniaProperty.Register<MetroWindow, Control>(nameof(TitleBarContent));

        public static readonly StyledProperty<bool> ClientDecorationsProperty =
            AvaloniaProperty.Register<MetroWindow, bool>(nameof(ClientDecorations));

        public static readonly StyledProperty<Control> TitleBarMenuContentProperty =
            AvaloniaProperty.Register<MetroWindow, Control>(nameof(TitleBarMenuContent));

        private Grid? _bottomHorizontalGrip;
        private Grid? _bottomLeftGrip;
        private Grid? _bottomRightGrip;
        private Button? _closeButton;
        private Image? _icon;
        private Grid? _leftVerticalGrip;
        private Button? _minimiseButton;

        private Button? _restoreButton;
        private Grid? _rightVerticalGrip;

        private Grid? _titleBar;
        private Grid? _titleBarMenu;
        private Grid? _topHorizontalGrip;
        private Grid? _topLeftGrip;
        private Grid? _topRightGrip;

        public bool ClientDecorations
        {
            get => GetValue(ClientDecorationsProperty);
            set => SetValue(ClientDecorationsProperty, value);
        }

        public Control TitleBarContent
        {
            get { return GetValue(TitleBarContentProperty); }
            set { SetValue(TitleBarContentProperty, value); }
        }

        public Control TitleBarMenuContent
        {
            get => GetValue(TitleBarMenuContentProperty);
            set => SetValue(TitleBarMenuContentProperty, value);
        }

        protected override Type StyleKeyOverride => typeof(MetroWindow);

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (!CanResize)
            {
                if (_titleBar?.IsPointerOver ?? false)
                {
                    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    {
                        BeginMoveDrag(e);
                    }

                    base.OnPointerPressed(e);
                }

                return;
            }

            if (_topHorizontalGrip?.IsPointerOver ?? false)
            {
                BeginResizeDrag(WindowEdge.North, e);
            }
            else if (_bottomHorizontalGrip?.IsPointerOver ?? false)
            {
                BeginResizeDrag(WindowEdge.South, e);
            }
            else if (_leftVerticalGrip?.IsPointerOver ?? false)
            {
                BeginResizeDrag(WindowEdge.West, e);
            }
            else if (_rightVerticalGrip?.IsPointerOver ?? false)
            {
                BeginResizeDrag(WindowEdge.East, e);
            }
            else if (_topLeftGrip?.IsPointerOver ?? false)
            {
                BeginResizeDrag(WindowEdge.NorthWest, e);
            }
            else if (_bottomLeftGrip?.IsPointerOver ?? false)
            {
                BeginResizeDrag(WindowEdge.SouthWest, e);
            }
            else if (_topRightGrip?.IsPointerOver ?? false)
            {
                BeginResizeDrag(WindowEdge.NorthEast, e);
            }
            else if (_bottomRightGrip?.IsPointerOver ?? false)
            {
                BeginResizeDrag(WindowEdge.SouthEast, e);
            }
            else if (_titleBar?.IsPointerOver ?? false)
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    BeginMoveDrag(e);
                }
            }

            base.OnPointerPressed(e);
        }

        private void ToggleWindowState()
        {
            if (!CanResize)
            {
                return;
            }

            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    break;

                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    break;
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);        

            if(change.Property == WindowStateProperty)
            {                
                PseudoClasses.Set(":maximised", change.GetNewValue<WindowState>() == WindowState.Maximized);
            }            
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
			base.OnApplyTemplate(e);

            _titleBar = e.NameScope.Find<Grid>("titlebar");
            _titleBarMenu = e.NameScope.Find<Grid>("titlebarMenu");
            _minimiseButton = e.NameScope.Find<Button>("minimiseButton");
            _restoreButton = e.NameScope.Find<Button>("restoreButton");
            _closeButton = e.NameScope.Find<Button>("closeButton");
            _icon = e.NameScope.Find<Image>("icon");

            _topHorizontalGrip = e.NameScope.Find<Grid>("topHorizontalGrip");
            _bottomHorizontalGrip = e.NameScope.Find<Grid>("bottomHorizontalGrip");
            _leftVerticalGrip = e.NameScope.Find<Grid>("leftVerticalGrip");
            _rightVerticalGrip = e.NameScope.Find<Grid>("rightVerticalGrip");

            _topLeftGrip = e.NameScope.Find<Grid>("topLeftGrip");
            _bottomLeftGrip = e.NameScope.Find<Grid>("bottomLeftGrip");
            _topRightGrip = e.NameScope.Find<Grid>("topRightGrip");
            _bottomRightGrip = e.NameScope.Find<Grid>("bottomRightGrip");

            if (_minimiseButton != null) _minimiseButton.Command = ReactiveCommand.Create(() => { WindowState = WindowState.Minimized; });

            if (_restoreButton != null)
            {
                if (CanResize)
                {
                    _restoreButton.Command = ReactiveCommand.Create(ToggleWindowState);
                }
                else
                {
                    _restoreButton.IsVisible = false;
                }
            }

            if (_closeButton != null) _closeButton.Command = ReactiveCommand.Create(Close);

            if (_titleBar != null) _titleBar.DoubleTapped += (sender, ee) => { if (!(ee.Source is Button) && !(ee.Source is ColoredSvg)) ToggleWindowState(); };

            if (_titleBarMenu != null) _titleBarMenu.DoubleTapped += (sender, ee) => { e.Handled = true; };

            if (_icon != null) _icon.DoubleTapped += (sender, ee) => { Close(); };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (_topHorizontalGrip != null) _topHorizontalGrip.IsVisible = false;
                if (_bottomHorizontalGrip != null) _bottomHorizontalGrip.IsHitTestVisible = false;
                if (_leftVerticalGrip != null) _leftVerticalGrip.IsHitTestVisible = false;
                if (_rightVerticalGrip != null) _rightVerticalGrip.IsHitTestVisible = false;
                if (_topLeftGrip != null) _topLeftGrip.IsHitTestVisible = false;
                if (_bottomLeftGrip != null) _bottomLeftGrip.IsHitTestVisible = false;
                if (_topRightGrip != null) _topRightGrip.IsHitTestVisible = false;
                if (_bottomRightGrip != null) _bottomRightGrip.IsHitTestVisible = false;

                BorderThickness = new Thickness();
            }
        }
    }
}
