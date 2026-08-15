using BarcodeScanning;
namespace BRB6.View
{
    public class Helper
    {
        static public CameraView GetCameraView(bool IsCameraEnabled=false) =>
        new ()
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    CameraEnabled = IsCameraEnabled,
                    VibrationOnDetected = false,
                    BarcodeSymbologies = BarcodeFormats.Ean13 | BarcodeFormats.Ean8 | BarcodeFormats.Code128 | BarcodeFormats.QRCode |  BarcodeFormats.DataMatrix,

                };
}
}
