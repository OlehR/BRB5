
using BRB51.PlatformDependency;

namespace BRB51
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