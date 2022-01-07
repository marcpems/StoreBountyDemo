//-----------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Microsoft Corporation">
//     Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace BountyTestApp
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.Data.Json;
    using Windows.Services.Store;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// Show the bounty indicator based on the capaign ID used in this conversion.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public enum AttestationState
        {
            Eligible,
            NonEligible,
            Unknown
        };

        /// <summary>
        /// Main page constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Retrieve the campaign ID for the 
        /// </summary>
        /// <returns>The campaign ID associated with this acquisition</returns>
        private async Task<string> GetCampaignId()
        {
            string campaignID = string.Empty;

            // Use APIs in the Windows.Services.Store namespace if they are available
            // (the app is running on a device with Windows 10, version 1607, or later).
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(
                 "Windows.Services.Store.StoreContext"))
            {
                StoreContext context = StoreContext.GetDefault();

                // Try to get the campaign ID for users with a recognized Microsoft account.
                StoreProductResult result = await context.GetStoreProductForCurrentAppAsync();
                if (result.Product != null)
                {
                    StoreSku sku = result.Product.Skus.FirstOrDefault(s => s.IsInUserCollection);

                    if (sku != null)
                    {
                        campaignID = sku.CollectionData.CampaignId;
                    }
                }

                if (string.IsNullOrEmpty(campaignID))
                {
                    // Try to get the campaign ID from the license data for users without a
                    // recognized Microsoft account.
                    StoreAppLicense license = await context.GetAppLicenseAsync();
                    JsonObject json = JsonObject.Parse(license.ExtendedJsonData);
                    if (json.ContainsKey("customPolicyField1"))
                    {
                        campaignID = json["customPolicyField1"].GetString();
                    }
                }
            }
            // Fall back to using APIs in the Windows.ApplicationModel.Store namespace instead
            // (the app is running on a device with Windows 10, version 1577, or earlier).
            else
            {
#if DEBUG
                campaignID = await Windows.ApplicationModel.Store.CurrentAppSimulator.GetAppPurchaseCampaignIdAsync();
#else
                campaignID = await Windows.ApplicationModel.Store.CurrentApp.GetAppPurchaseCampaignIdAsync() ;
#endif
            }

            return campaignID;
        }

        /// <summary>
        /// Get the bounty eligability for this acquisition and return a string either Yes or No
        /// </summary>
        /// <returns>a string of either yes or no</returns>
        private async Task<string> GetBountyEligible()
        {
            string bountyEligible = string.Empty;

            try
            {
                if (await IsMicrosoftBounty())
                {
                    bountyEligible = "Yes";
                }
                else
                {
                    bountyEligible = "No";
                }
            }
            catch (Exception ex)
            {
                bountyEligible = string.Format("No - Error requesting: {0}", ex.Message);
            }

            return bountyEligible;
        }

        /// <summary>
        /// Check and return if this acquisition is bounty Eligible
        /// </summary>
        /// <returns>true if the event is bounty Eligible, false otherwise</returns>
        private async Task<bool> IsMicrosoftBounty()
        {
            uint userContext = 27;
            uint deviceContext = 28;

            // First check if the user (MSA) has acquired the app through a bounty Eligible source
            // If the call for the user returns ERROR_NO_SUCH_USER then the machine can be checked

            var isUserContextAccrued = await IsMicrosoftAccrued(userContext);
            bool isDeviceContextAccrued = false;
            if (isUserContextAccrued == AttestationState.Unknown)
            {
                isDeviceContextAccrued = (await IsMicrosoftAccrued(deviceContext)) == AttestationState.Eligible;
            }

            return isDeviceContextAccrued || isUserContextAccrued == AttestationState.Eligible;
        }

        /// <summary>
        /// Check and return if this acquisition is related to a Microsoft sponsired source
        /// </summary>
        /// <param name="requestId">the request ID. This is usually either the user or machine ID</param>
        /// <returns>true if the acquisition is Microsoft sponsored, false otherwise</returns>
        private async Task<AttestationState> IsMicrosoftAccrued(uint requestId)
        {
            AttestationState isAccrued = AttestationState.Unknown;

            try
            {
                StoreContext ctx = StoreContext.GetDefault();
                var result = await StoreRequestHelper.SendRequestAsync(ctx, requestId, "{}");
                if (result.HttpStatusCode == Windows.Web.Http.HttpStatusCode.None &&
                    string.IsNullOrEmpty(result.Response) &&
                    result.ExtendedError.HResult == unchecked((int)0x80070525))
                {
                    // the user doesnt exist
                    isAccrued = AttestationState.Unknown;
                }
                else
                {
                    JsonObject jsonObject = JsonObject.Parse(result.Response);
                    isAccrued = jsonObject.GetNamedBoolean("IsMicrosoftAccrued") ? AttestationState.Eligible : AttestationState.NonEligible;
                }
            }
            catch (System.Exception ex)
            {
                // TODO: log the exception and possibly handle here.

                // For this example the exception text will be used to show on the app screen, 
                // so rethrow the exception so its exposed in the caller
                throw ex; 
            }

            return isAccrued;
        }

        /// <summary>
        /// Handle the page loaded event
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event parameters</param>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var id = await GetCampaignId();
            if (string.IsNullOrEmpty(id))
            {
                CampaignID.Text = "No Campaign ID Found";
            }
            else
            {
                CampaignID.Text = id;
            }

            var message = await GetBountyEligible();
            BountyEligible.Text = message;
        }
    }
}
