namespace QarnotSDK
{
    /// <summary>
    /// Multi Slots settings for a pool.
    /// </summary>
    public class MultiSlotsSettings
    {
        /// <summary>
        /// Number of slots per node
        /// </summary>
        /// <example>8</example>
        public uint SlotsPerNode { get; set; }

        /// <summary> Create a Multi Slots settings object </summary>
        /// <param name="slotsPerNode">Number of slots per node</param>
        public MultiSlotsSettings(uint slotsPerNode)
        {
            SlotsPerNode = slotsPerNode;
        }

        /// <summary>Equality implementaiton</summary>
        public override bool Equals(object other)
        {
            if (other is MultiSlotsSettings otherMultiSlotsSettings)
            {
                return SlotsPerNode == otherMultiSlotsSettings.SlotsPerNode;
            } else {
                return false;
            }
        }

        /// <summary>ToString</summary>
        public override string ToString() => $"MultiSlotsSettings: SlotsPerNode: {SlotsPerNode}";

        /// <summary>GetHashCode</summary>
        public override int GetHashCode() {
            return ("MultiSlotsSettings" + SlotsPerNode.ToString()).GetHashCode();
        }
    }

}
