namespace QarnotSDK {
    /// <summary>
    /// Describe a constant to be overriden when running the task.
    /// <br />This is meant to be used for development only and require
    /// specific permissions.
    /// </summary>
    public class ForcedConstant
    {
        /// <param name="constantName">Name of the constant.</param>
        /// <param name="forcedValue">Value to set as a forced override.</param>
        /// <param name="forceExportInEnvironment">Whether to force constant export to the compute environment.</param>
        /// <param name="access">Access (RO / RW) to override, or null to keep exiting value.</param>
        public ForcedConstant(
                string constantName = null,
                string forcedValue = null,
                bool? forceExportInEnvironment = null,
                ForcedConstantAccess? access = null)
        {
            ConstantName = constantName;
            ForcedValue = forcedValue;
            ForceExportInEnvironment = forceExportInEnvironment;
            Access = access;
        }

        /// <summary>
        /// Possible values for the Access property of a
        /// ForcedConstant object.
        /// </summary>
        public enum ForcedConstantAccess
        {
            /// <summary>The constant is set as read-only</summary>
            ReadOnly,
            /// <summary>The constant is set as read-write</summary>
            ReadWrite,
        }

        /// <summary>
        /// The name of the constant to override.
        /// </summary>
        public string ConstantName { get; set; }

        /// <summary>
        /// The new value for the constant. The value is
        /// unchanged if the field is not present or null.
        /// </summary>
        public string ForcedValue { get; set; } = null;

        /// <summary>
        /// Change whether the constant should be exported
        /// to the environment or not. The behaviour is unchanged
        /// if the field is not present or null.
        /// </summary>
        public bool? ForceExportInEnvironment { get; set; } = null;

        /// <summary>
        /// Change the access of the constant from read-write to
        /// read-only or vice-versa. The behaviour is unchanged if
        /// the field is not present or null.
        /// </summary>
        public ForcedConstantAccess? Access { get; set; } = null;
    }
}
