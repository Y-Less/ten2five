using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;
using System;
using System.Drawing;
using System.Reflection;

/// <summary>
/// Class implementing support for "minimize to tray" functionality.
/// </summary>
public class MinimizeToTray
{
    /// <summary>
    /// Enables "minimize to tray" behavior for the specified Window.
    /// </summary>
    /// <param name="window">Window to enable the behavior for.</param>
    /*public static void Enable(Window window)
    {
        // No need to track this instance; its event handlers will keep it alive
        new MinimizeToTrayInstance(window);
    }*/

    /// <summary>
    /// Class implementing "minimize to tray" functionality for a Window instance.
    /// </summary>
    //private class MinimizeToTray
    //{
    private Window _window;
    private NotifyIcon _notifyIcon;
    private bool _balloonShown;
	private  bool _minimized = false;

    public Window Window { get { return _window; } }
    public NotifyIcon NotifyIcon { get { return _notifyIcon; } }
    //public bool _balloonShown { get { return _window; } }

    /// <summary>
    /// Initializes a new instance of the MinimizeToTrayInstance class.
    /// </summary>
    /// <param name="window">Window instance to attach to.</param>
	public MinimizeToTray(Window window, bool balloonShown)
    {
        Debug.Assert(window != null, "window parameter is null.");
        _window = window;
        _window.StateChanged += new EventHandler(HandleStateChanged);
		_balloonShown = balloonShown;
    }

    /// <summary>
    /// Handles the Window's StateChanged event.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void HandleStateChanged(object sender, EventArgs e)
    {
        if (_notifyIcon == null)
        {
            // Initialize NotifyIcon instance "on demand"
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
            _notifyIcon.MouseClick += new MouseEventHandler(HandleNotifyIconOrBalloonClicked);
            _notifyIcon.BalloonTipClicked += new EventHandler(HandleNotifyIconOrBalloonClicked);
        }
        // Update copy of Window Title in case it has changed
        _notifyIcon.Text = _window.Title;

        // Show/hide Window and NotifyIcon
        _minimized = (_window.WindowState == WindowState.Minimized);
		_window.ShowInTaskbar = !_minimized;
		_notifyIcon.Visible = _minimized;
		if (_minimized && !_balloonShown)
		{
			// If this is the first time minimizing to the tray, show the user what happened
			_notifyIcon.ShowBalloonTip(1000, null, _window.Title, ToolTipIcon.None);
			_balloonShown = true;
		}
    }

	public void ShowBaloon(string message)
	{
		if (_minimized)
		{
			_notifyIcon.ShowBalloonTip(1000, null, message, ToolTipIcon.None);
		}
	}

	public bool BaloonShown()
	{
		return _balloonShown;
	}

	public void SetIcon(Icon icon)
	{
		if (_minimized)
		{
			_notifyIcon.Icon = icon;
		}
	}

	public void Hide()
	{
		_window.WindowState = WindowState.Minimized;
	}

	public void Restore()
	{
		_window.WindowState = WindowState.Normal;
	}

    /// <summary>
    /// Handles a click on the notify icon or its balloon.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void HandleNotifyIconOrBalloonClicked(object sender, EventArgs e)
    {
        // Restore the Window
        //_window.WindowState = WindowState.Normal;
		Restore();
    }
    //}
}

