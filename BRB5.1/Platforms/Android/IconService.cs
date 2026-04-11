using Android.Content;
using Android.Content.PM;
using BRB5;
using BRB5.Model;

namespace BRB6.Platforms.Android
{
    public static class IconService
    {
        public static void SwitchIcon(eCompany pCompany)
        {
            var context = Platform.CurrentActivity;
            var pm = context.PackageManager;
            var packageName = context.PackageName;

            // Маппінг компанії на alias. Щоб додати нову іконку — просто додай рядок сюди
            var companyToAlias = new Dictionary<eCompany, string>
    {
        { eCompany.PSU,       $"{packageName}.MainActivitySpar" },
        { eCompany.Sim23,     $"{packageName}.MainActivityDefault" },    // додаси коли буде іконка
        { eCompany.Universal, $"{packageName}.MainActivityDefault" },
        { eCompany.NotDefined,$"{packageName}.MainActivityDefault" },
    };
            string activeAlias= $"{packageName}.MainActivityDefault", currentAlias = $"{packageName}.MainActivityDefault";

            if (companyToAlias.ContainsKey(Config.CurrentIcon))
                currentAlias = companyToAlias[Config.CurrentIcon];          

            if (companyToAlias.ContainsKey(pCompany))
                activeAlias = companyToAlias[pCompany];

            if (activeAlias.Equals(currentAlias))
                return; // Іконка вже активна, нічого не робимо

            pm.SetComponentEnabledSetting(new ComponentName(packageName, activeAlias), ComponentEnabledState.Enabled, ComponentEnableOption.DontKillApp);
            pm.SetComponentEnabledSetting(new ComponentName(packageName, currentAlias), ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
            Config.CurrentIcon = pCompany;
        }
    }
}