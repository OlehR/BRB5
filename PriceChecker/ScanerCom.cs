using RJCP.IO.Ports;
using System.Text;
using System.Timers;
using Utils;

namespace Equipments
{
    public class ScanerCom :  IDisposable
    {
        protected Action<string, string> OnBarCode;
        //private readonly System.Timers.Timer Timer;
        private readonly object Lock = new object();
        private SerialPortStreamWrapper SerialDevice;
        string SerialPort;
        int BaudRate;
        protected string TextError = string.Empty;
        public bool IsReady { get { return SerialDevice != null; } }
        public ScanerCom(string pSerialPort, int pBaudRate, Action<string, string> pOnBarCode=null)
        {
            SerialPort = pSerialPort;
            BaudRate = pBaudRate;
            OnBarCode = pOnBarCode;
            Init();
            // Timer = new System.Timers.Timer(500.0);
            // Timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            //Timer.AutoReset = true;
        }
        public void SetOnBarCode(Action<string, string> pOnBarCode) => OnBarCode = pOnBarCode;

        public  void Init()
        {
            lock (Lock)
            {
                TextError = string.Empty;
                try
                {                    
                    CloseIfOpen();
                    SerialDevice.Open();
                    SerialDevice.DiscardInBuffer();
                    SerialDevice.DiscardOutBuffer();
                    FileLogger.WriteLogMessage(this, "ScanerCom", $"Init Port=>{SerialPort} BaudRate=>{BaudRate}");
                }
                catch (Exception ex)
                {
                    TextError = ex.Message;
                    FileLogger.WriteLogMessage(this, "ScanerCom", ex);
                }
                finally
                {
                    SerialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
                }
            }
            FileLogger.WriteLogMessage($"ScanerCom/Init TextError={TextError}");
        }

        /*bool IsRead = false;
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            IsRead = true;
            SerialDevice?.Write(new byte[4] { 0, 0, 0, 3 });
        }*/

        private void CloseIfOpen()
        {
            if (SerialDevice != null)
            {
                if (IsReady)
                    SerialDevice.Close();
                SerialDevice.Dispose();
            }
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, Parity.Even, StopBits.One, 8, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            SerialDevice = portStreamWrapper;
        }

        private bool OnDataReceived(byte[] data)
        {
            try
            {
                for (int i = 0; i < data.Length; i++)
                    if (data[i] == 160) data[i] = 13;
                string Str = Encoding.ASCII.GetString(data);
                FileLogger.WriteLogMessage("OnDataReceived=>" + Str);
                Str = Str.Replace("\r", "").Replace("\n", "");

                OnBarCode?.Invoke(Str, null);
                return true;
            }
            catch(Exception ex) 
            {
                FileLogger.WriteLogMessage(this, "OnDataReceived", ex);
                return false;
            }
        }

        /*public void GetReadDataSync(byte[] command, Action<byte[]> onDatAction)
        {
            lock (Lock)
            {
                if (!IsReady || onDatAction == null) return;
                SerialDevice.Write(command);
                Thread.Sleep(30);
                do; while (SerialDevice.ReadBufferSize < 1);
                byte[] numArray = new byte[SerialDevice.ReadBufferSize];
                SerialDevice.Read(numArray, 0, numArray.Length);
                onDatAction?.Invoke(numArray);
            }
        }*/
        public void Dispose()
        {
            OnBarCode = null;
            SerialDevice?.Close();
            SerialDevice?.Dispose();
        }
    }

    public class SerialPortStreamWrapper : SerialPortStream
    {
        public Func<byte[], bool> OnReceivedData { get; set; }

        public SerialPortStreamWrapper(
          string port,
          int baudRate,
          Parity parity = Parity.None,
          StopBits stopBits = StopBits.Two,
          int data = 8,
          Func<byte[], bool> onReceivedData = null)
          : base(port, baudRate, data, parity, stopBits)
        {
            this.OnReceivedData = onReceivedData;
            this.ReadTimeout = 1000;
            this.WriteTimeout = 1000;
        }

        public void Write(byte[] data) => this.Write(data, 0, data.Length);

        protected override void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            SerialPortStream serialPortStream = (SerialPortStream)sender;
            if (this.OnReceivedData == null)
            {
                base.OnDataReceived(sender, args);
            }
            else
            {
                int bytesToRead = serialPortStream.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                serialPortStream.Read(buffer, 0, bytesToRead);
                Func<byte[], bool> onReceivedData = this.OnReceivedData;
                if (onReceivedData != null)
                {
                    int num = onReceivedData(buffer) ? 1 : 0;
                }
                base.OnDataReceived(sender, args);
            }
        }

        public void ClearBuffer()
        {
            try
            {
                this.DiscardInBuffer();
                this.DiscardOutBuffer();
                this.Flush();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
