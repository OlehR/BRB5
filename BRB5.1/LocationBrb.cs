using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Utils;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace BRB5.Model
{
    public class LocationBrb
    {

        public static Action<Location> OnLocation { get; set; }
        public static Location Location =null;
        public static Warehouse LocationWarehouse = null;
        //static object Lock= new object();
        static CancellationTokenSource cts;
        public static async Task<IEnumerable<Warehouse>> GetCurrentLocation(IEnumerable<Warehouse> Wh)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                Location = await Geolocation.GetLocationAsync(request, cts.Token);

                if (Location != null)
                {
                    foreach (var el in Wh)
                    {
                        el.Distance = Location.CalculateDistance(el.GPSX, el.GPSY, DistanceUnits.Kilometers) * 1000;
                    }
                    Console.WriteLine($"Latitude: {Location.Latitude}, Longitude: {Location.Longitude}, Altitude: {Location.Altitude}");
                    var res = Wh.OrderBy(o => o.Distance);
                    if (res.Any())
                    {
                        LocationWarehouse = res.First();
                    }
                    OnLocation?.Invoke(Location);
                    return res;
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }

            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage("LocationBrb", "GetCurrentLocation", ex);
                // Unable to get location
            }
            return Wh;
        }
        
        public void Dispose()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
        }
    }
}
