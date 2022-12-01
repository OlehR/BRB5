
using BRB5.PlatformDependency;

namespace BRB5.Droid
{

    public class AndroidStorageManager : IStorage
    {
        public double GetRemainingStorage()
        {
            var freeExternalStorage = Android.OS.Environment.ExternalStorageDirectory.UsableSpace;

            return freeExternalStorage;
        }
    }
}