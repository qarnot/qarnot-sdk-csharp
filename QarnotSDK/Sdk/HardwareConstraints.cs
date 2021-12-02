using System.Collections.Generic;

namespace QarnotSDK
{
    /// <summary>
    /// Constraints (core number, ram, gpu, etc.) that the hardware should satisfy for the execution of the tasks.
    /// </summary>
    public class HardwareConstraints : List<HardwareConstraint>
    {
    }

    /// <summary>
    /// Constraint type for Json deserialization
    /// </summary>
    internal static class HardwareConstraintDiscriminators
    {
        # pragma warning disable CS1591
        public const string MinimumCoreHardwareConstraint = "MinimumCoreHardwareConstraint";
        public const string MaximumCoreHardwareConstraint = "MaximumCoreHardwareConstraint";
        public const string MinimumRamCoreRatioHardwareConstraint = "MinimumRamCoreRatioHardwareConstraint";
        public const string MaximumRamCoreRatioHardwareConstraint = "MaximumRamCoreRatioHardwareConstraint";
        public const string SpecificHardwareConstraint = "SpecificHardwareConstraint";
        public const string MinimumRamHardwareConstraint = "MinimumRamHardwareConstraint";
        public const string MaximumRamHardwareConstraint = "MaximumRamHardwareConstraint";
        public const string GpuHardwareConstraint = "GpuHardwareConstraint";
        # pragma warning restore CS1591
    }

    /// <summary>
    /// Base class of the Hardware contraints
    /// </summary>
    public abstract class HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint. Possible values are :
        /// - MinimumCoreHardwareConstraint
        /// - MaximumCoreHardwareConstraint
        /// - MinimumRamCoreRatioHardwareConstraint
        /// - MaximumRamCoreRatioHardwareConstraint
        /// - SpecificHardwareConstraint
        /// - MinimumRamHardwareConstraint
        /// - MaximumRamHardwareConstraint
        /// - GpuHardwareConstraint
        /// </summary>
        public virtual string Discriminator { get; set; }
    }

    /// <summary>
    /// Constraint to limit the minimum number of cores
    /// </summary>
    public class MinimumCoreHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.MinimumCoreHardwareConstraint;
        /// <summary>
        /// Minimum number of cores
        /// </summary>
        public int CoreCount { get; set; }

        /// <summary>
        /// Set the minimum number of cores
        /// </summary>
        /// <param name="coreCount">minimum number of core</param>
        public MinimumCoreHardware(int coreCount)
        {
            CoreCount = coreCount;
        }
    }

    /// <summary>
    /// Constraint to limit the maximum number of cores
    /// </summary>
    public class MaximumCoreHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.MaximumCoreHardwareConstraint;
        /// <summary>
        /// Minimum number of cores
        /// </summary>
        public int CoreCount { get; set; }

        /// <summary>
        /// Set the maximum number of cores
        /// </summary>
        /// <param name="coreCount">maximum number of core</param>
        public MaximumCoreHardware(int coreCount)
        {
            CoreCount = coreCount;
        }
    }

    /// <summary>
    /// Constraint to limit the minimum ram core ratio
    /// </summary>
    public class MinimumRamCoreRatioHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.MinimumRamCoreRatioHardwareConstraint;
        /// <summary>
        /// Minimum memory core ratio in GB
        /// </summary>
        public decimal MinimumMemoryGBCoreRatio { get; set; }

        /// <summary>
        /// Set the minimum ram core ratio
        /// </summary>
        /// <param name="ramCoreRatio">minimum ram core ratio</param>
        public MinimumRamCoreRatioHardware(decimal ramCoreRatio)
        {
            MinimumMemoryGBCoreRatio = ramCoreRatio;
        }
    }

    /// <summary>
    /// Constraint to limit the maximum ram core ratio
    /// </summary>
    public class MaximumRamCoreRatioHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.MaximumRamCoreRatioHardwareConstraint;
        /// <summary>
        /// Maximum memory core ratio in GB
        /// </summary>
        public decimal MaximumMemoryGBCoreRatio { get; set; }

        /// <summary>
        /// Set the maximum ram core ratio
        /// </summary>
        /// <param name="ramCoreRatio">maximum ram core ratio</param>
        public MaximumRamCoreRatioHardware(decimal ramCoreRatio)
        {
            MaximumMemoryGBCoreRatio = ramCoreRatio;
        }
    }

    /// <summary>
    /// Constraint to limit the execution to specific hardware
    /// </summary>
    public class SpecificHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.SpecificHardwareConstraint;
        /// <summary>
        /// The Hardware key.
        /// </summary>
        public string SpecificationKey { get; set; }

        /// <summary>
        /// Limit to a specific hardware
        /// </summary>
        /// <param name="hardwareKey">key representing the hardware</param>
        public SpecificHardware(string hardwareKey)
        {
            SpecificationKey = hardwareKey;
        }
    }

    /// <summary>
    /// Constraint to limit the execution to hardwares with GPU
    /// </summary>
    public class GpuHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.GpuHardwareConstraint;
    }

    /// <summary>
    /// Constraint to limit the minimum ram memory
    /// </summary>
    public class MinimumRamHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.MinimumRamHardwareConstraint;
        /// <summary>
        /// Minimum RAM memory in MB
        /// </summary>
        public decimal MinimumMemoryMB { get; set; }

        /// <summary>
        /// Set the minimum ram
        /// </summary>
        /// <param name="ram">minimum ram</param>
        public MinimumRamHardware(decimal ram)
        {
            MinimumMemoryMB = ram;
        }
    }

    /// <summary>
    /// Constraint to limit the maximum ram memory
    /// </summary>
    public class MaximumRamHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.MaximumRamHardwareConstraint;
        /// <summary>
        /// Maximum RAM memory in MB
        /// </summary>
        public decimal MaximumMemoryMB { get; set; }

        /// <summary>
        /// Set the maximum ram
        /// </summary>
        /// <param name="ram">maximum ram</param>
        public MaximumRamHardware(decimal ram)
        {
            MaximumMemoryMB = ram;
        }
    }
}