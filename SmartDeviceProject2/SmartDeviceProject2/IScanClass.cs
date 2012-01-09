using System;
namespace TSDServer
{
    public interface IScanClass
    {
        Scanned OnScanned { get; set; }

        ScanError OnScanError { get; set; }

        int InitScan();
        bool Paused { get; set; }
        void PauseScan();
        void ResumeScan();
        void StopScan();
        void CloseScan();
        
    }
}
