// http://dlaa.me/blog/post/9889700

using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

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
    private Window window_;
    private NotifyIcon notifyIcon_;
    private bool balloonShown_;
	private  bool minimized_ = false;

    public Window Window { get { return window_; } }
    public NotifyIcon NotifyIcon { get { return notifyIcon_; } }

    /// <summary>
    /// Initializes a new instance of the MinimizeToTrayInstance class.
    /// </summary>
    /// <param name="window">Window instance to attach to.</param>
	public MinimizeToTray(Window window, bool balloonShown)
    {
        Debug.Assert(window != null, "window parameter is null.");
        window_ = window;
        window_.StateChanged += new EventHandler(HandleStateChanged);
		balloonShown_ = balloonShown;
    }

    /// <summary>
    /// Handles the Window's StateChanged event.
    /// </summary>
    /// <param name="sender">Event source.</param>
    /// <param name="e">Event arguments.</param>
    private void HandleStateChanged(object sender, EventArgs e)
    {
        if (notifyIcon_ == null)
        {
            // Initialize NotifyIcon instance "on demand"
            notifyIcon_ = new NotifyIcon();
            notifyIcon_.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
            notifyIcon_.MouseClick += new MouseEventHandler(HandleNotifyIconOrBalloonClicked);
            notifyIcon_.BalloonTipClicked += new EventHandler(HandleNotifyIconOrBalloonClicked);
        }
        // Update copy of Window Title in case it has changed
        notifyIcon_.Text = window_.Title;

        // Show/hide Window and NotifyIcon
        minimized_ = (window_.WindowState == WindowState.Minimized);
        if (minimized_)
            window_.WindowStyle = WindowStyle.ToolWindow;
        else
            window_.WindowStyle = WindowStyle.SingleBorderWindow;
        window_.ShowInTaskbar = !minimized_;
		notifyIcon_.Visible = minimized_;
		if (minimized_ && !balloonShown_)
		{
			// If this is the first time minimizing to the tray, show the user what happened
			notifyIcon_.ShowBalloonTip(1000, null, window_.Title, ToolTipIcon.None);
			balloonShown_ = true;
		}
    }

	public void ShowBaloon(string message)
	{
		if (minimized_)
		{
			notifyIcon_.ShowBalloonTip(1000, null, message, ToolTipIcon.None);
		}
	}

	public bool BaloonShown()
	{
		return balloonShown_;
	}

	public void SetIcon(Icon icon)
	{
		if (minimized_)
		{
			notifyIcon_.Icon = icon;
		}
	}

	public void Hide()
	{
		window_.WindowState = WindowState.Minimized;
	}

	public void Restore()
	{
		window_.WindowState = WindowState.Normal;
	}
    
	public void BringToFront()
	{
		window_.WindowState = WindowState.Minimized;
		window_.Show();
		window_.WindowState = WindowState.Normal;
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
}

