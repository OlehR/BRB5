using Android.Content;
using Android.Content.PM;
using BRB5;

namespace BRB6.Platforms.Android
{
    public static class IconService
    {
        public static void SwitchToSpar(eCompany useSpar)
        {
            var context = Platform.CurrentActivity;
            var pm = context.PackageManager;
            var packageName = context.PackageName;

            var defaultAlias = new ComponentName(packageName, $"{packageName}.MainActivityDefault");
            var sparAlias = new ComponentName(packageName, $"{packageName}.MainActivitySpar");

            if (useSpar==eCompany.PSU)
            {
                // Вмикаємо Spar, вимикаємо Default
                pm.SetComponentEnabledSetting(sparAlias, ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
                pm.SetComponentEnabledSetting(defaultAlias, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
            }
            else
            {
                // Вмикаємо Default, вимикаємо Spar
                pm.SetComponentEnabledSetting(defaultAlias, ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
                pm.SetComponentEnabledSetting(sparAlias, ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
            }
        }
    }
}