namespace Engage.Dnn.Events
{
    /// <summary>
    /// This class contains Event Module specific settings. Framework version contains general settings.
    /// As more functionality is moved up, some of these can go with it.
    /// </summary>
    internal class Setting: Framework.Setting
    {
        public static readonly Setting DisplayModeOption = new Setting("DisplayModeOption", "What data to display.");
        public static readonly Setting SkinSelection = new Setting("SkinSelection", "The skin used for Calendar Display.");

       
        public static readonly Setting PrinterFriendly = new Setting("pnlPrinterFriendly",  "Hide/Show the printer friendly link on the module");
        public static readonly Setting EmailAFriend = new Setting("pnlEmailAFriend", "Hide/Show the Email a Friend link on the module");
        public static readonly Setting PrivacyPolicyUrl = new Setting("upnlRating", "Specify the URL for your Privacy Policy.");
        public static readonly Setting UnsubscribeUrl = new Setting("unsubscribeUrl","Specify the URL for unsubscribing.");
        public static readonly Setting OpenLinkUrl = new Setting("openLinkUrl", "Specify the URL for your Open Link to track opens.");
        public static readonly Setting ReplacementMessage = new Setting("replacementMessage", "You can include an entire section of replaceable text in your message using this setting.");

        protected Setting(string propertyName, string description)
            : base(propertyName, description)
        {
        }
    }
}