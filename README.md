# Introduction 
This sample app shows the two principles for identifying the source of app acquisition. Acquisition takes place when a user visits the store app and selects install / get for the app package. 
Understanding how the user got to the point of acquisition is important if a bounty is paid for finding and directing that user to the app.


The Windows Store acquisition process supports the Campaign IDs or CID, which is an additional URL parameter captured at the point of entry to the store. The CID is tracked and attached to the acquisition event as a result of the URL forward to the store. For example, the following URL includes a CID value of Contoso_Add_Banner:
https://www.microsoft.com/store/apps/9PJT94BJ3H6L?CID=Contoso_Add_Banner

More details about campaigns, how to track them and how to acquire the CID from the app can be found here: 
https://docs.microsoft.com/en-us/windows/uwp/publish/create-a-custom-app-promotion-campaign

In this sample the code to retrieve the campaign ID can be found in MainPage.xaml.cs, in the method GetCampaignId().


Although Campaign IDs can be well adapted to help identify bounty eligibility, they were not originally designed for this. A new and simpler API will be introduced with RS5 that is designed to work alongside the Project Brazil initiative (85% / 95% revenue share) that identifies the acquisition source as either a Microsoft sponsored source and thus eligible for bounty - 85% revenue share, or not - 95% revenue share. 

This is implemented through an extension to the StoreRequestHelper() API and can be seen in the sample in MainPage.xaml.cs, in the method GetBountyEligable(). At the time of creation, the API will only respond correctly on RS5 builds, although this is expected to be extended to older versions after launch. 


# Getting Started
For this sample to work correctly it must be installed via the store, so there are two instances of the app already published for you to try:
https://www.microsoft.com/store/apps/9PJT94BJ3H6L <- test app instance one
https://www.microsoft.com/store/apps/9MWKGZKHDMHF <- test app instance two.

NOTE: the CID and bounty eligibility are both 'sticky' because the acquisition event is one time. This means that the source of acquisition cannot be changed or removed once it has taken place for a user. So, if you go to store and install one of the sample apps, the account you use for the store will always report that acquisition data for any subsequent install of the app on any device with that same user. There is no way to reset the acquisition event.


# Build and Test
Clone this project, open the solution and build in Visual Studio 2017.

# Contribute
