using System.Reflection;
using System.Windows;

namespace Ten2Five.Utils
{
    static class ModalWindow
    {
        public static bool IsModal(this Window window)
        {
            return (bool)typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
        }

        public static void Close(this Window window, bool result)
        {
            if (window.IsModal())
                window.DialogResult = result;
            else
                window.Close();
        }
    }
}

