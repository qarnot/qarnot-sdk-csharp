using System;
using System.Collections.Generic;
using System.Linq;
namespace QarnotSDK {

    /// <summary>
    /// Current running usage, in instances and in cores.
    /// </summary>
    public class RunningCounts
    {
        /// <summary>
        /// Current number of running instances. Never negative.
        /// </summary>
        /// <example>1</example>
        public int RunningInstancesCount { get; set; }

        /// <summary>
        /// Current number of running cores. Never negative.
        /// </summary>
        /// <example>8</example>
        public int RunningCoresCount { get; set; }

        internal RunningCounts() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is RunningCounts c
            && c.RunningInstancesCount == RunningInstancesCount
            && c.RunningCoresCount == RunningCoresCount;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(RunningInstancesCount, RunningCoresCount);

        /// <summary>ToString</summary>
        public override string ToString() => $"<RunningInstancesCount={RunningInstancesCount}, RunningCoresCount={RunningCoresCount}>";
    }

    /// <summary>
    /// User computing quota description
    /// </summary>
    public class UserSchedulingQuotas : RunningCounts
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

        internal UserSchedulingQuotas() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => base.Equals(obj)
            && obj is UserSchedulingQuotas q
            && q.MaxInstances == MaxInstances
            && q.MaxCores == MaxCores;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), MaxInstances, MaxCores);

        /// <summary>ToString</summary>
        public override string ToString() => $"<MaxInstances={MaxInstances}, MaxCores={MaxCores}, RunningInstancesCount={RunningInstancesCount}, RunningCoresCount={RunningCoresCount}>";
    }

    /// <summary>
    /// Organization computing quota description
    /// </summary>
    public class OrganizationSchedulingQuotas : RunningCounts
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

        internal OrganizationSchedulingQuotas() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => base.Equals(obj)
            && obj is OrganizationSchedulingQuotas q
            && q.MaxInstances == MaxInstances
            && q.MaxCores == MaxCores;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), MaxInstances, MaxCores);

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

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), ReservationName, MachineKey);

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

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), ReservationName, MachineKey);

        /// <summary>ToString</summary>
        public override string ToString() => $"<ReservationName={ReservationName}, MachineKey={MachineKey}, MaxInstances={MaxInstances}, MaxCores={MaxCores}, RunningInstancesCount={RunningInstancesCount}, RunningCoresCount={RunningCoresCount}>";
    }

    /// <summary>
    /// Organization computing quota description for a scheduling type, with the breakdown of the current running
    /// usage among the users of the organization.
    /// </summary>
    public class OrganizationSchedulingQuotasWithUserDetails : OrganizationSchedulingQuotas
    {
        /// <summary>
        /// Current running usage of this scheduling type by each user of the organization, keyed by the user's
        /// email. Only reports the users who count in the organization's quota, and only those with something
        /// running with this scheduling type: a user with nothing running here is absent, so the same user may
        /// well appear in one scheduling type and not in another.
        ///
        /// Null when the requester is not allowed to read the organization quota details, whereas an empty
        /// dictionary means that no user of the organization has anything running with this scheduling type.
        /// </summary>
        public Dictionary<string, RunningCounts> RunningCountsPerUser { get; set; }

        internal OrganizationSchedulingQuotasWithUserDetails() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => base.Equals(obj)
            && obj is OrganizationSchedulingQuotasWithUserDetails q
            && Utils.DictionaryEquals(RunningCountsPerUser, q.RunningCountsPerUser);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Utils.DictionaryHashCode(RunningCountsPerUser));

        /// <summary>ToString</summary>
        public override string ToString()
        {
            string runningCounts = RunningCountsPerUser != null ? $"{{{String.Join(",", RunningCountsPerUser)}}}" : "null";
            return $"<MaxInstances={MaxInstances}, MaxCores={MaxCores}, RunningInstancesCount={RunningInstancesCount}, RunningCoresCount={RunningCoresCount}, RunningCountsPerUser={runningCounts}>";
        }
    }

    /// <summary>
    /// Computing quota for a specific reserved machine of the organization, with the breakdown of the current
    /// running usage among the users of the organization.
    /// </summary>
    public class OrganizationReservedSchedulingQuotaWithUserDetails : OrganizationReservedSchedulingQuota
    {
        /// <summary>
        /// Current running usage of this reservation by each user of the organization, keyed by the user's email.
        /// Only reports the users who count in the organization's quota, and only those with something running on
        /// this reservation: a user with nothing running here is absent, so the same user may well appear on one
        /// reservation and not on another.
        ///
        /// Null when the requester is not allowed to read the organization quota details, whereas an empty
        /// dictionary means that no user of the organization has anything running on this reservation.
        /// </summary>
        public Dictionary<string, RunningCounts> RunningCountsPerUser { get; set; }

        internal OrganizationReservedSchedulingQuotaWithUserDetails() {
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => base.Equals(obj)
            && obj is OrganizationReservedSchedulingQuotaWithUserDetails q
            && Utils.DictionaryEquals(RunningCountsPerUser, q.RunningCountsPerUser);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Utils.DictionaryHashCode(RunningCountsPerUser));

        /// <summary>ToString</summary>
        public override string ToString()
        {
            string runningCounts = RunningCountsPerUser != null ? $"{{{String.Join(",", RunningCountsPerUser)}}}" : "null";
            return $"<ReservationName={ReservationName}, MachineKey={MachineKey}, MaxInstances={MaxInstances}, MaxCores={MaxCores}, RunningInstancesCount={RunningInstancesCount}, RunningCoresCount={RunningCoresCount}, RunningCountsPerUser={runningCounts}>";
        }
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
                && Utils.SequenceEquals(ucq?.Reserved?.OrderBy(q => q.MachineKey), Reserved?.OrderBy(q => q.MachineKey));

            /// <inheritdoc/>
            public override int GetHashCode() => HashCode.Combine(Flex, OnDemand, Utils.SequenceHashCode(Reserved?.OrderBy(q => q.MachineKey)));

            /// <summary>ToString</summary>
            public override string ToString()
            {
                String reservedString = Reserved != null ? $"[{String.Join(",", Reserved)}]" : "null";
                return $"<UserComputingQuotas : Flex={Flex?.ToString()}, OnDemand={OnDemand?.ToString()}, Reserved={reservedString}>";
            }
        }

        /// <summary>
        /// Common organization computing quota description, without organization or user identifying information.
        /// </summary>
        public abstract class OrganizationComputingQuotasBase
        {
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

            internal OrganizationComputingQuotasBase() {
            }

            /// <inheritdoc/>
            public override bool Equals(object obj) => obj is OrganizationComputingQuotasBase ocq
                && OrganizationSchedulingQuotas.Equals(Flex, ocq?.Flex)
                && OrganizationSchedulingQuotas.Equals(OnDemand, ocq?.OnDemand)
                && Utils.SequenceEquals(ocq?.Reserved?.OrderBy(q => q.MachineKey), Reserved?.OrderBy(q => q.MachineKey));

            /// <inheritdoc/>
            public override int GetHashCode() => HashCode.Combine(Flex, OnDemand, Utils.SequenceHashCode(Reserved?.OrderBy(q => q.MachineKey)));

            /// <summary>ToString</summary>
            public override string ToString()
            {
                String reservedString = Reserved != null ? $"[{String.Join(",", Reserved)}]" : "null";
                return $"Flex={Flex?.ToString()}, OnDemand={OnDemand?.ToString()}, Reserved={reservedString}";
            }
        }

        /// <summary>
        /// Organization computing quota description
        /// </summary>
        public class OrganizationComputingQuotas : OrganizationComputingQuotasBase
        {
            /// <summary>
            /// Name of the organization
            /// </summary>
            public string Name { get; set; }

            internal OrganizationComputingQuotas() {
            }

            /// <inheritdoc/>
            public override bool Equals(object obj) => base.Equals(obj)
                && obj is OrganizationComputingQuotas ocq
                && ocq.Name == Name;

            /// <inheritdoc/>
            public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name);

            /// <summary>ToString</summary>
            public override string ToString() => $"<OrganizationComputingQuotas : Name={Name}, {base.ToString()}>";
        }

        /// <summary>
        /// Organization computing quota description, where each scheduling type and each reservation comes with an
        /// optional breakdown of its current running usage among the users of the organization.
        /// </summary>
        public class OrganizationComputingQuotasWithUserDetails
        {
            /// <summary>
            /// Computing quota description for instances with a Flex scheduling type.
            /// </summary>
            public OrganizationSchedulingQuotasWithUserDetails Flex { get; set; }

            /// <summary>
            /// Computing quota description for instances with a OnDemand scheduling type.
            /// </summary>
            public OrganizationSchedulingQuotasWithUserDetails OnDemand { get; set; }

            /// <summary>
            /// List of quotas for each of the organization's reserved machines.
            /// </summary>
            public List<OrganizationReservedSchedulingQuotaWithUserDetails> Reserved { get; set; }

            internal OrganizationComputingQuotasWithUserDetails() {
            }

            /// <inheritdoc/>
            public override bool Equals(object obj) => obj is OrganizationComputingQuotasWithUserDetails ocq
                && OrganizationSchedulingQuotasWithUserDetails.Equals(Flex, ocq.Flex)
                && OrganizationSchedulingQuotasWithUserDetails.Equals(OnDemand, ocq.OnDemand)
                && Utils.SequenceEquals(ocq.Reserved?.OrderBy(q => q.MachineKey), Reserved?.OrderBy(q => q.MachineKey));

            /// <inheritdoc/>
            public override int GetHashCode() => HashCode.Combine(Flex, OnDemand, Utils.SequenceHashCode(Reserved?.OrderBy(q => q.MachineKey)));

            /// <summary>ToString</summary>
            public override string ToString()
            {
                String reservedString = Reserved != null ? $"[{String.Join(",", Reserved)}]" : "null";
                return $"<OrganizationComputingQuotasWithUserDetails : Flex={Flex?.ToString()}, OnDemand={OnDemand?.ToString()}, Reserved={reservedString}>";
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

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(User, Organization);

        /// <summary>ToString</summary>
        public override string ToString() => $"<ComputingQuotas :\nUser={User?.ToString()}\nOrganization={Organization?.ToString()}>";
    }
}
