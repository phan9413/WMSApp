namespace WMSApp.Interface
{
    /// <summary>
    /// For disply the message on the screen
    /// </summary>
    public interface IToastMessage
    {
        void LongAlert(string message);
        void ShortAlert(string message);
    }

}
