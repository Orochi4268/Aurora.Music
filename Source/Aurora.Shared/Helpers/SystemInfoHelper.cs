﻿// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources.Core;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.ViewManagement;
using Windows.Globalization.DateTimeFormatting;
using System.Globalization;

namespace Aurora.Shared.Helpers
{

    public class CultureInfoHelper
    {
        public static CultureInfo CurrentCulture
        { get; } = GetCurrentCulture();

        private static CultureInfo GetCurrentCulture()
        {
            var cultureName = new DateTimeFormatter("longdate", new[] { "US" }).ResolvedLanguage;

            return new CultureInfo(cultureName);
        }
    }


    public static class SystemInfoHelper
    {
        public static IEnumerable<string> GetSystemInfo()
        {
            var sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (v & 0x000000000000FFFFL);
            string sysVer = $"{v1}.{v2}.{v3}.{v4}";
            var eascdi = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            var resources = ResourceContext.GetForCurrentView();
            var lang = resources.QualifierValues["Language"];
            var region = resources.QualifierValues["HomeRegion"];
            var ver = Package.Current.Id.Version.Major.ToString("0") + "." +
               Package.Current.Id.Version.Minor.ToString("0") + "." +
               Package.Current.Id.Version.Build.ToString("0");
            return new string[] { "Version = " + sysVer, "Package = " + ver, "Language = " + lang, "HomeRegion = " + region,
            "EAS ID = " + eascdi.Id, "Friendly Name = " + eascdi.FriendlyName, "OS = " + eascdi.OperatingSystem, "Firmware = " + eascdi.SystemFirmwareVersion,
            "Hardware = " + eascdi.SystemHardwareVersion, "Manufacturer = " + eascdi.SystemManufacturer, "Product Name = " + eascdi.SystemProductName,
            "SKU = " + eascdi.SystemSku };

        }

        public static string GetPackageVer()
        {
#if BETA
            return Package.Current.Id.Version.Major.ToString("0") + "." +
               Package.Current.Id.Version.Minor.ToString("0") + "." +
               Package.Current.Id.Version.Build.ToString("0")+" - BETA";
#else
            return Package.Current.Id.Version.Major.ToString("0") + "." +
               Package.Current.Id.Version.Minor.ToString("0") + "." +
               Package.Current.Id.Version.Build.ToString("0");
#endif
        }

        public static string ToVersionString(this Version version)
        {
            return $"{version.Major.ToString("0")}.{version.Minor.ToString("0")}.{version.Build.ToString("0")}";
        }

        public static ulong GetPackageVersionNum()
        {
            return (Convert.ToUInt64(Package.Current.Id.Version.Major) << 48) + (Convert.ToUInt64(Package.Current.Id.Version.Minor) << 32) + (Convert.ToUInt64(Package.Current.Id.Version.Build) << 16) + Convert.ToUInt64(Package.Current.Id.Version.Revision);
        }


        public static DeviceFormFactorType GetDeviceFormFactorType()

        {

            switch (AnalyticsInfo.VersionInfo.DeviceFamily)

            {

                case "Windows.Mobile":

                    return DeviceFormFactorType.Phone;

                case "Windows.Desktop":

                    return UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse

                        ? DeviceFormFactorType.Desktop

                        : DeviceFormFactorType.Tablet;

                case "Windows.Universal":

                    return DeviceFormFactorType.IoT;

                case "Windows.Team":

                    return DeviceFormFactorType.SurfaceHub;

                default:

                    return DeviceFormFactorType.Other;

            }
        }
        public static async Task<string> GetCurrentUserNameAsync()
        {
            IReadOnlyList<User> users = await User.FindAllAsync();

            var current = users.Where(p => p.AuthenticationStatus == UserAuthenticationStatus.LocallyAuthenticated &&
                                        p.Type == UserType.LocalUser).FirstOrDefault();

            // user may have username
            var data = await current.GetPropertyAsync(KnownUserProperties.AccountName);
            string displayName = (string)data;
            //or may be authinticated using hotmail 
            if (String.IsNullOrEmpty(displayName))
            {

                string a = (string)await current.GetPropertyAsync(KnownUserProperties.FirstName);
                string b = (string)await current.GetPropertyAsync(KnownUserProperties.LastName);
                displayName = string.Format("{0} {1}", a, b);
            }
            return displayName;
        }
    }


    public enum DeviceFormFactorType

    {

        Phone,

        Desktop,

        Tablet,

        IoT,

        SurfaceHub,

        Other

    }
}
