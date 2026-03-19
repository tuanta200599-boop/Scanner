namespace Scanner.Constants
{
    public static class ApiEndpoints
    {
        public static class Inbound
        {
            public const string GetAsnList = "Inbound/Asn/GetList";
            public const string UpdateExpressStatus = "Inbound/Asn/UpdateExpressStatus";
            public const string ScanHandheld = "Inbound/Asn_Line/Scan-Handheld";
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
