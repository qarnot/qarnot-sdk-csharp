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
        public const string NoGpuHardwareConstraint = "NoGpuHardwareConstraint";
        public const string SSDHardwareConstraint = "SSDHardwareConstraint";
        public const string NoSSDHardwareConstraint = "NoSSDHardwareConstraint";
        public const string CpuModelHardwareConstraint = "CpuModelHardwareConstraint";
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
        /// - NoGpuHardwareConstraint
        /// - SSDHardwareConstraint
        /// - NoSSDHardwareConstraint
        /// - CpuModelHardwareConstraint
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

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is MinimumCoreHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => (Discriminator + CoreCount.ToString()).GetHashCode(); 
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

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is MaximumCoreHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => (Discriminator + CoreCount.ToString()).GetHashCode(); 
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

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is MinimumRamCoreRatioHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => (Discriminator + MinimumMemoryGBCoreRatio.ToString()).GetHashCode(); 
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

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is MaximumRamCoreRatioHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => (Discriminator + MaximumMemoryGBCoreRatio.ToString()).GetHashCode(); 
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

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is SpecificHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => (Discriminator + SpecificationKey.ToString()).GetHashCode(); 
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

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is GpuHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => Discriminator.GetHashCode(); 
    }

    /// <summary>
    /// Constraint to limit the execution to hardwares without GPU
    /// </summary>
    public class NoGpuHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.NoGpuHardwareConstraint;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is NoGpuHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => Discriminator.GetHashCode(); 
    }

    /// <summary>
    /// Constraint to limit the execution to hardwares with SSD
    /// </summary>
    public class SSDHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.SSDHardwareConstraint;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is SSDHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => Discriminator.GetHashCode(); 
    }

    /// <summary>
    /// Constraint to limit the execution to hardwares without SSD
    /// </summary>
    public class NoSSDHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.NoSSDHardwareConstraint;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is NoSSDHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => Discriminator.GetHashCode(); 
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

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is MinimumRamHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => (Discriminator + MinimumMemoryMB.ToString()).GetHashCode(); 
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

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is MaximumRamHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => (Discriminator + MaximumMemoryMB.ToString()).GetHashCode(); 
    }

    /// <summary>
    /// Constraint to request a specific CPU
    /// </summary>
    public class CpuModelHardware : HardwareConstraint
    {
        /// <summary>
        /// Type of the Constraint.
        /// </summary>
        public override string Discriminator => HardwareConstraintDiscriminators.CpuModelHardwareConstraint;

        /// <summary>
        /// Requested CPU model
        /// </summary>
        public string CpuModel { get; set; }

        /// <summary>
        /// Request a specific CPU
        /// </summary>
        /// <param name="cpuModel">requested CPU model</param>
        public CpuModelHardware(string cpuModel)
        {
            CpuModel = cpuModel;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is CpuModelHardware x && x.GetHashCode() == GetHashCode();

        /// <inheritdoc/>
        public override int GetHashCode() => (Discriminator + CpuModel.ToString()).GetHashCode(); 
    }
}
