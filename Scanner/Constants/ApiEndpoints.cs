namespace Scanner.Constants
{
    public static class ApiEndpoints
    {
        public static class Inbound
        {
            public const string GetAsnList = "Inbound/Asn/GetList";
            public const string UpdateExpressStatus = "Inbound/Asn/UpdateExpressStatus";
            public const string ScanHandheld = "Inbound/Asn_Line/Scan-Handheld";
            public const string GetHistoryScan = "Inbound/Asn_Line/GetHistoryScan";
            public const string DeleteAsnLine = "Inbound/Asn_Line/Delete";
            public const string GetLpnList  = "Inbound/Lpn/GetList";
            public const string CreateLpn   = "Inbound/Lpn/Create";
        }

        public static class Configuration
        {
            public const string GetPalletList = "Configuration/Pallet/GetList";
            public const string UpdatePallet = "Configuration/Pallet/Update";
        }
        public static class Outbound
        {
            public const string GetPickTaskList = "Outbound/ListPickTask";
            public const string RecordPickingScan = "Outbound/RecordPickingScan";
        }
    }
}
