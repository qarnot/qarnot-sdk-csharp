using System;
using System.Collections.Generic;
using System.Linq;
namespace QarnotSDK {

    /// <summary>
    /// User computing quota description
    /// </summary>
    public class UserSchedulingQuotas
    {
        /// <summary>
        /// Maximum number of instances of the given scheduling type that the user can run simultaneously.
        /// </summary>
        /// <example>64</example>
        public int MaxInstances { get; set; }

        /// <summary>
        /// Maximum number of cores running at the same time for instances with the given scheduling type for the user.
        /// </summary>
        /// <example>512</example>
        public int MaxCores { get; set; }

        /// <summary>
        /// Current number of running instances scheduled for the given scheduling type for the user.
        /// </summary>
        /// <example>1</example>
        public int RunningInstancesCount { get; set; }

        /// <summary>
        /// Current number of running cores scheduled for the given scheduling type for the user.
        /// </summary>
        /// <example>1</example>
        public int RunningCoresCount { get; set; }

        internal UserSchedulingQuotas() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is UserSchedulingQuotas q
            && q.MaxInstances == MaxInstances
            && q.MaxCores == MaxCores
            && q.RunningInstancesCount == RunningInstancesCount
            && q.RunningCoresCount == RunningCoresCount;

        /// <summary>ToString</summary>
        public override string ToString() => $"<MaxInstances={MaxInstances}, MaxCores={MaxCores}, RunningInstancesCount={RunningInstancesCount}, RunningCoresCount={RunningCoresCount}>";
    }

    /// <summary>
    /// Organization computing quota description
    /// </summary>
    public class OrganizationSchedulingQuotas
    {
        /// <summary>
        /// Maximum number of instances of the given scheduling type that the users in the organization can run simultaneously.
        /// </summary>
        /// <example>64</example>
        public int MaxInstances { get; set; }

        /// <summary>
        /// Maximum number of cores running at the same time for instances with the given scheduling type within an organization.
        /// </summary>
        /// <example>512</example>
        public int MaxCores { get; set; }

        /// <summary>
        /// Current number of running instances scheduled for the given scheduling type within an organization.
        /// </summary>
        /// <example>1</example>
        public int RunningInstancesCount { get; set; }

        /// <summary>
        /// Current number of running cores scheduled for the given scheduling type within an organization.
        /// </summary>
        /// <example>1</example>
        public int RunningCoresCount { get; set; }

        internal OrganizationSchedulingQuotas() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is OrganizationSchedulingQuotas q
            && q.MaxInstances == MaxInstances
            && q.MaxCores == MaxCores
            && q.RunningInstancesCount == RunningInstancesCount
            && q.RunningCoresCount == RunningCoresCount;

        /// <summary>ToString</summary>
        public override string ToString() => $"<MaxInstances={MaxInstances}, MaxCores={MaxCores}, RunningInstancesCount={RunningInstancesCount}, RunningCoresCount={RunningCoresCount}>";
    }

    /// <summary>
    /// Computing quota for a specific reserved machine of the user
    /// </summary>
    public class UserReservedSchedulingQuota : UserSchedulingQuotas
    {
        /// <summary>
        /// Key name of the reserved machine.
        /// </summary>
        /// <value>my-reserved-machine</value>
        public string MachineKey { get; set; }

        /// <summary>
        /// Name of the reservation.
        /// </summary>
        /// <value>my-reserved-machine</value>
        public string ReservationName { get; set; }

        internal UserReservedSchedulingQuota() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => base.Equals(obj)
            && obj is UserReservedSchedulingQuota q
            && q.ReservationName == ReservationName
            && q.MachineKey == MachineKey;

        /// <summary>ToString</summary>
        public override string ToString() => $"<ReservationName={ReservationName}, MachineKey={MachineKey}, MaxInstances={MaxInstances}, MaxCores={MaxCores}, RunningInstancesCount={RunningInstancesCount}, RunningCoresCount={RunningCoresCount}>";
    }

    /// <summary>
    /// Computing quota for a specific reserved machine of the organization
    /// </summary>
    public class OrganizationReservedSchedulingQuota : OrganizationSchedulingQuotas
    {
        /// <summary>
        /// Key name of the reserved machine.
        /// </summary>
        /// <value>my-reserved-machine</value>
        public string MachineKey { get; set; }

        /// <summary>
        /// Name of the reservation.
        /// </summary>
        /// <value>my-reserved-machine</value>
        public string ReservationName { get; set; }

        internal OrganizationReservedSchedulingQuota() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => base.Equals(obj)
            && obj is OrganizationReservedSchedulingQuota q
            && q.ReservationName == ReservationName
            && q.MachineKey == MachineKey;

        /// <summary>ToString</summary>
        public override string ToString() => $"<ReservationName={ReservationName}, MachineKey={MachineKey}, MaxInstances={MaxInstances}, MaxCores={MaxCores}, RunningInstancesCount={RunningInstancesCount}, RunningCoresCount={RunningCoresCount}>";
    }


    /// <summary>
    /// Computing quota description for a user
    /// </summary>
    public class ComputingQuotas
    {
        /// <summary>
        /// User computing quota description
        /// </summary>
        public class UserComputingQuotas
        {
            /// <summary>
            /// Computing quota description for instances with a Flex scheduling type.
            /// </summary>
            public UserSchedulingQuotas Flex { get; set; }

            /// <summary>
            /// Computing quota description for instances with a OnDemand scheduling type.
            /// </summary>
            public UserSchedulingQuotas OnDemand { get; set; }

            /// <summary>
            /// List of quotas for each of the user's reserved machines.
            /// </summary>
            public List<UserReservedSchedulingQuota> Reserved { get; set; }

            internal UserComputingQuotas() {
            }

            /// <inheritdoc/>
            public override bool Equals(object obj) => obj is UserComputingQuotas ucq
                && UserSchedulingQuotas.Equals(Flex, ucq?.Flex)
                && UserSchedulingQuotas.Equals(OnDemand, ucq?.OnDemand)
                && Enumerable.SequenceEqual(ucq?.Reserved?.OrderBy(q => q.MachineKey), Reserved?.OrderBy(q => q.MachineKey));

            /// <summary>ToString</summary>
            public override string ToString()
            {
                String reservedString = Reserved != null ? $"[{String.Join(",", Reserved)}]" : "null";
                return $"<UserComputingQuotas : Flex={Flex?.ToString()}, OnDemand={OnDemand?.ToString()}, Reserved={reservedString}>";
            }
        }

        /// <summary>
        /// Organization computing quota description
        /// </summary>
        public class OrganizationComputingQuotas
        {
            /// <summary>
            /// Name of the organization
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Computing quota description for instances with a Flex scheduling type.
            /// </summary>
            public OrganizationSchedulingQuotas Flex { get; set; }

            /// <summary>
            /// Computing quota description for instances with a OnDemand scheduling type.
            /// </summary>
            public OrganizationSchedulingQuotas OnDemand { get; set; }

            /// <summary>
            /// List of quotas for each of the organization's reserved machines.
            /// </summary>
            public List<OrganizationReservedSchedulingQuota> Reserved { get; set; }

            internal OrganizationComputingQuotas() {
            }

            /// <inheritdoc/>
            public override bool Equals(object obj) => obj is OrganizationComputingQuotas ocq
                && OrganizationSchedulingQuotas.Equals(Flex, ocq?.Flex)
                && OrganizationSchedulingQuotas.Equals(OnDemand, ocq?.OnDemand)
                && Enumerable.SequenceEqual(ocq?.Reserved?.OrderBy(q => q.MachineKey), Reserved?.OrderBy(q => q.MachineKey));

            /// <summary>ToString</summary>
            public override string ToString()
            {
                String reservedString = Reserved != null ? $"[{String.Join(",", Reserved)}]" : "null";
                return $"<OrganizationComputingQuotas : Flex={Flex?.ToString()}, OnDemand={OnDemand?.ToString()}, Reserved={reservedString}>";
            }
        }

        /// <summary>
        /// User computing quotas
        /// </summary>
        public UserComputingQuotas User { get; set; }

        /// <summary>
        /// Organization computing quotas
        /// </summary>
        public OrganizationComputingQuotas Organization { get; set; }

        internal ComputingQuotas() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is ComputingQuotas cq
            && UserComputingQuotas.Equals(User, cq?.User)
            && OrganizationComputingQuotas.Equals(Organization, cq?.Organization);

        /// <summary>ToString</summary>
        public override string ToString() => $"<ComputingQuotas :\nUser={User?.ToString()}\nOrganization={Organization?.ToString()}>";
    }
}
