namespace QarnotSDK.UnitTests.SdkTests.Carbon
{
    public static class CarbonFactsTestData
    {
        public const double ExpectedConsumedEnergy = 1432;
        public const double ExpectedEnergyIT = 1234;
        public const double ExpectedReuseEnergy = 1232;
        public const double ExpectedCarbonFootprint = 1042;
        public const string ExpectedEquivalentDCName = "default-datacenter";
        public const double ExpectedEquivalentDCCarbonFootprint = 3600;
        public const double ExpectedSavedCarbonFootprintCompute = 1402;
        public const double ExpectedSavedCarbonFootprintHeat = 1156;
        public const double ExpectedSavedCarbonFootprintComputeHeat = 2558;
        public const double ExpectedSavedCarbonFootprintPercent = 71;
        public const double ExpectedPUE = 1.1;
        public const double ExpectedERE = 0.8;
        public const double ExpectedERF = 0.9;
        public const double ExpectedWUE = 0.1;

        private static string carbonFactsResponseBody = $@"
        {{
            ""total_consumed_energy_Wh"": {ExpectedConsumedEnergy},
            ""total_energy_it_Wh"": {ExpectedEnergyIT},
            ""total_reused_energy_Wh"": {ExpectedReuseEnergy},
            ""PUE"": {ExpectedPUE},
            ""ERE"": {ExpectedERE},
            ""ERF"": {ExpectedERF},
            ""equivalent_datacenter_name"": ""{ExpectedEquivalentDCName}"",
            ""qarnot_carbon_footprint"": {ExpectedCarbonFootprint},
            ""equivalent_DC_carbon_footprint"": {ExpectedEquivalentDCCarbonFootprint},
            ""saved_carbon_footprint_compute"": {ExpectedSavedCarbonFootprintCompute},
            ""saved_carbon_footprint_heat"": {ExpectedSavedCarbonFootprintHeat},
            ""saved_carbon_footprint_compute_heat"": {ExpectedSavedCarbonFootprintComputeHeat},
            ""saved_carbon_footprint_percent"": {ExpectedSavedCarbonFootprintPercent},
            ""WUE"": {ExpectedWUE}
        }}
        ";

        public static string CarbonFactsResponseBody { get => carbonFactsResponseBody; set => carbonFactsResponseBody = value; }
    }
}