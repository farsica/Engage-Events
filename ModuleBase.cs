// <copyright file="ModuleBase.cs" company="Engage Software">
// Engage: Events - http://www.engagemodules.com
// Copyright (c) 2004-2008
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.WebControls;
    using Utility = Engage.Utility;

    /// <summary>
    /// This class extends the framework version in order for developers to add on any specific methods/behavior.
    /// </summary>
    public class ModuleBase : Framework.ModuleBase, IActionable
    {
        /// <summary>
        /// A resource file for shared resources in this module.
        /// </summary>
        protected internal static readonly string LocalSharedResourceFile = "~" + DesktopModuleFolderName + Localization.LocalResourceDirectory + "/" + Localization.LocalSharedResourceFile;

        /// <summary>
        /// Gets the module actions.
        /// </summary>
        /// <value>The module actions.</value>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection actions = new ModuleActionCollection();

                if (HostSettings.GetHostSetting("EnableModuleOnLineHelp") == "Y" && Utility.HasValue(this.ModuleConfiguration.HelpUrl))
                {
                    ModuleAction helpAction = new ModuleAction(this.GetNextActionID());
                    helpAction.Title = Localization.GetString(ModuleActionType.OnlineHelp, Localization.GlobalResourceFile);
                    helpAction.CommandName = ModuleActionType.OnlineHelp;
                    helpAction.CommandArgument = string.Empty;
                    helpAction.Icon = "action_help.gif";
                    helpAction.Url = Globals.FormatHelpUrl(this.ModuleConfiguration.HelpUrl, this.PortalSettings, this.ModuleConfiguration.FriendlyName);
                    helpAction.Secure = SecurityAccessLevel.Edit;
                    helpAction.UseActionEvent = true;
                    helpAction.Visible = true;
                    helpAction.NewWindow = true;
                    actions.Add(helpAction);
                }

                return actions;
            }
        }

        /// <summary>
        /// Gets the event id.
        /// </summary>
        /// <value>The event id.</value>
        protected static int EventId
        {
            get
            {
                int id = -1;
                if (HttpContext.Current.Request.QueryString["eventId"] != null)
                {
                    id = Convert.ToInt32(HttpContext.Current.Request.QueryString["eventId"], CultureInfo.InvariantCulture);
                }

                return id;
            }
        }

        /// <summary>
        /// Gets the register URL.
        /// </summary>
        /// <value>The register URL.</value>
        protected string RegisterUrl
        {
            get
            {
                return this.BuildLinkUrl(this.ModuleId, "Register", "eventid=" + EventId.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets the RSVP URL.
        /// </summary>
        /// <value>The RSVP URL.</value>
        protected string RsvpUrl
        {
            get
            {
                return this.BuildLinkUrl(this.ModuleId, "Rsvp", "eventid=" + EventId.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is configured.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is configured; otherwise, <c>false</c>.
        /// </value>
        protected override bool IsConfigured
        {
            get { return Utility.HasValue(Dnn.Utility.GetStringSetting(this.Settings, Framework.Setting.DisplayTemplate.PropertyName)); }
        }

        /// <summary>
        /// Gets the index of the current page from the QueryString.
        /// </summary>
        /// <value>The index of the current page.</value>
        protected int CurrentPageIndex
        {
            get
            {
                int index;
                if (!int.TryParse(this.Request.QueryString["currentPage"], NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    index = 1;
                }

                return index;
            }
        }

        /// <summary>
        /// This method takes the eventId and the currently logged in user (if any) and checks for
        /// an existing RSVP(registration) for the user.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>
        /// 	<c>true</c> if the current user if registered for the specified event; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsRegistered(int eventId)
        {
            bool registered = false;
            if (eventId > 0 && IsLoggedIn)
            {
                Engage.Events.Rsvp rsvp = Engage.Events.Rsvp.Load(eventId, DotNetNuke.Entities.Users.UserController.GetCurrentUserInfo().Email);
                registered = (rsvp != null);
            }

            return registered;
        }

        /// <summary>
        /// Sends an <c>iCalendar</c> to the client to download.
        /// </summary>
        /// <param name="response">The response to use to send the <c>iCalendar</c>.</param>
        /// <param name="content">The content of the <c>iCalendar</c>.</param>
        /// <param name="name">The name of the file.</param>
        protected static void SendICalendarToClient(HttpResponse response, string content, string name)
        {
            response.Clear();

            // Stream The ICalendar 
            response.Buffer = true;

            response.ContentType = "text/calendar";
            response.ContentEncoding = Encoding.UTF8;
            response.Charset = "utf-8";

            response.AddHeader("Content-Disposition", "attachment;filename=\"" + HttpUtility.UrlEncode(name) + ".ics" + "\"");

            response.Write(content);
            response.End();
        }

        /// <summary>
        /// Gets localized text for the given resource key using this control's <see cref="DotNetNuke.Entities.Modules.PortalModuleBase.LocalResourceFile"/>.
        /// </summary>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns>Localized text for the given resource key</returns>
        protected string Localize(string resourceKey)
        {
            return Localization.GetString(resourceKey, this.LocalResourceFile);
        }

        /// <summary>
        /// Gets the <c>QueryString</c> control key for the current <c>Request</c>.
        /// </summary>
        /// <returns>The <c>QueryString</c> control key for the current <c>Request</c></returns>
        protected string GetCurrentControlKey()
        {
            string keyParam = string.Empty;
            string[] modIdParams = this.Request.QueryString["modId"] == null ? new string[] { } : this.Request.QueryString["modId"].Split(';');
            string[] keyParams = this.Request.QueryString["key"] == null ? new string[] { } : this.Request.QueryString["key"].Split(';');

            for (int i = 0; i < modIdParams.Length && i < keyParams.Length; i++)
            {
                int modId;
                if (int.TryParse(modIdParams[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out modId) && modId == this.ModuleId)
                {
                    keyParam = keyParams[i];
                    break;
                }
            }

            return keyParam;
        }

        /// <summary>
        /// Sets up a DNN <see cref="PagingControl"/>.
        /// </summary>
        /// <param name="pagingControl">The pager control.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <param name="queryStringKeys">The QueryString keys which should be persisted when the paging links are clicked.</param>
        protected void SetupPagingControl(PagingControl pagingControl, int totalRecords, params string[] queryStringKeys)
        {
            pagingControl.Visible = totalRecords != 0;
            pagingControl.TotalRecords = totalRecords;
            pagingControl.CurrentPage = this.CurrentPageIndex;
            pagingControl.TabID = this.TabId;
            pagingControl.QuerystringParams = GenerateQueryStringParameters(this.Request, queryStringKeys);
        }

        /// <summary>
        /// Generates a list of QueryString parameters for the given list of <paramref name="queryStringKeys"/>.
        /// </summary>
        /// <param name="request">The current request.</param>
        /// <param name="queryStringKeys">The keys for which to generate parameters.</param>
        /// <returns>
        /// A list of QueryString parameters for the given list of <paramref name="queryStringKeys"/>
        /// </returns>
        private static string GenerateQueryStringParameters(HttpRequest request, IEnumerable<string> queryStringKeys)
        {
            StringBuilder queryString = new StringBuilder(64);
            foreach (string key in queryStringKeys)
            {
                if (queryString.Length > 0)
                {
                    queryString.Append("&");
                }

                queryString.Append(key);
                queryString.Append("=");
                queryString.Append(request.QueryString[key]);
            }

            return queryString.ToString();
        }
    }
}
