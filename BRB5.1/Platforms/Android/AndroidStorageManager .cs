
using BRB6.PlatformDependency;

namespace BRB6
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