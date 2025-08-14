using BRB5.Model;
using PhotosUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace BRB6.Platforms.iOS
{
    public class PhotoService
    {
        public async Task<List<FilePickerResult>> PickPhotos()
        {
            List<FilePickerResult> result = null;
            var imagePicker = new PHPickerViewController(new PHPickerConfiguration
            {
                //Filter = PHPickerFilter.ImagesFilter,
                SelectionLimit = 50,
            });
            UIWindow window = null;
            foreach (UIWindowScene se in UIApplication.SharedApplication.ConnectedScenes)
            {
                if (se.ActivationState == UISceneActivationState.ForegroundActive)
                {
                    foreach (UIWindow window1 in se.Windows)
                    {
                        if (window1.IsKeyWindow)
                        {
                            window = window1;
                        }
                    }
                }
            }
            var vc = window.RootViewController;
            while (vc.PresentedViewController != null)
            {
                vc = vc.PresentedViewController;
            }
            var tcs = new TaskCompletionSource<Task<List<FilePickerResult>>>();

            imagePicker.Delegate = new PPD
            {
                CompletedHandler = res => tcs.TrySetResult(PickerResultsToMediaFile(res))
            };
            await vc.PresentViewControllerAsync(imagePicker, true);
            var resultTask = await tcs.Task;
            if(resultTask.Status!= TaskStatus.Faulted)
                result = await resultTask;
            await vc.DismissViewControllerAsync(true);
            imagePicker?.Dispose();
            return result;
        }
        //https://medium.com/@michalpr/maui-mediapicker-multiple-selection-d0f678dcd5f7
        private async Task<List<FilePickerResult>> PickerResultsToMediaFile(PHPickerResult[] res)
        {
            var result = new List<FilePickerResult>();

            foreach (var item in res)
            {
                var imageName = string.Empty;
                try
                {
                    var provider = item.ItemProvider;
                    imageName = provider.SuggestedName;
                    var identifiers = provider.RegisteredTypeIdentifiers;
                    var identifier = identifiers
                        .FirstOrDefault(s => s.EndsWith(".jpeg") || s.EndsWith(".jpg")
                            || s.EndsWith(".png") || s.EndsWith(".bmp") || s.EndsWith(".mp4") || s.EndsWith("quicktime-movie"));
                    ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

                    var data = await provider
                      .LoadDataRepresentationAsync(identifier);
                    //.WithTimeout(4);
                    var ss= identifier.Split('.');
                    var ext = ss[ss.Length-1];
                    if ("quicktime-movie".Equals(ext)) ext = "mov";

                    var bytes = data.ToArray();
                    result.Add(new FilePickerResult($"{imageName}.{ext}", bytes));
                }
                catch (Exception ex)
                {
                    //this.exceptionHandler?.Invoke(ex);
                }
            }
            return result;
            // You need to convert the selected images into a byte array and return them to MUAI according to your needs.
            //return null;
        }      
    }
    class PPD : PHPickerViewControllerDelegate
    {
        public Action<PHPickerResult[]> CompletedHandler { get; set; }

        public override void DidFinishPicking(PHPickerViewController picker, PHPickerResult[] results) =>
            CompletedHandler?.Invoke(results?.Length > 0 ? results : null);
    }
    

}
