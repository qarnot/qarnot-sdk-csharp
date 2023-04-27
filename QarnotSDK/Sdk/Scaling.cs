using System;
using System.Collections.Generic;
using System.Linq;

namespace QarnotSDK
{
    /// <summary>How a pool will be scaled dynamically </summary>
    public sealed class Scaling : IEquatable<Scaling>
    {
        /// <summary>Ordered list of scaling policies</summary>
        /// <remarks>At a given time and date, the first policy of the list with a matching time period will be the active one.
        /// Subsequent policies are ignored.</remarks>
        public List<ScalingPolicy> Policies { get; private set; } = new List<ScalingPolicy>();

        /// <summary>Name of the currently active policy</summary>
        /// <remarks>This may remain empty or outdated for a few tens of seconds after submission or update, while the
        /// pool is being submitted, and the information propagated back</remarks>
        public string ActivePolicyName { get; set; } = null;

        /// <summary>Currencly active policy, as returned by the latest update from the API</summary>
        /// <remarks>This may remain empty or outdated for a few tens of seconds after submission or update, while the
        /// pool is being submitted, and the information propagated back</remarks>
        public ScalingPolicy ActivePolicy { get {
            if (ActivePolicyName == null) {
                return null;
            } else {
                return Policies.FirstOrDefault(pol => pol.Name == ActivePolicyName);
            }
        } }

        /// <summary>Build a Scaling instance from a list of policies</summary>
        public Scaling(List<ScalingPolicy> policies)
        {
            Policies = policies;
        }

        /// <summary>Equality implementaiton</summary>
        public bool Equals(Scaling other)
        {
            if (other == null) {
                return false;
            } else if ((other?.Policies.Count ?? 0) != Policies.Count) {
                return false;
            } else {
                return Enumerable.Zip(Policies, other.Policies, (pol1, pol2) => new Tuple<ScalingPolicy, ScalingPolicy>(pol1, pol2))
                                 .All(tpl => tpl.Item1.Equals(tpl.Item2));
            }
        }

        /// <summary>Hash code to complement equality</summary>
        public override int GetHashCode() => Policies.Aggregate(0, (acc, pol) => acc.GetHashCode() ^ pol.GetHashCode());

        /// <summary>ToString</summary>
        public override string ToString() => $"Scaling: Policies: {String.Join(", ", Policies)}";
    }


    /// <summary>Abstract base class for scaling policies</summary>
    public abstract class ScalingPolicy : IEquatable<ScalingPolicy>
    {
        /// <summary>Human friendly name for the policy</summary>
        /// <remarks>Must be unique in a policies list</remarks>
        public string Name { get; private set; }

        /// <summary>Periods of time during which the policy is enabled</summary>
        public List<TimePeriodSpecification> EnabledPeriods { get; private set; }


        /// <summary>Type hint for the remote API</summary>
        public abstract string Type { get; }

        /// <summary>Equals</summary>
        public abstract bool Equals(ScalingPolicy other);

        /// <summary>Base constructor</summary>
        protected ScalingPolicy(string name, List<TimePeriodSpecification> enabledPeriods)
        {
            Name = name;
            EnabledPeriods = enabledPeriods;
        }
    }



    /// <summary>Details for a fixed, non-dynamic scaling policy</summary>
    public sealed class FixedScalingPolicy : ScalingPolicy
    {
        /// <summary>Number of pool slots to provision. There may be multiple slots per machine.</summary>
        public ulong SlotsCount { get; private set; } = 0;

        /// <summary>Type hint for the remote API</summary>
        public override string Type => "Fixed";

        /// <summary>Constructor</summary>
        public FixedScalingPolicy(string name,
                                  List<TimePeriodSpecification> enabledPeriods,
                                  ulong slotsCount)
            : base(name, enabledPeriods)
        {
            SlotsCount = slotsCount;
        }


        /// <summary>ToString</summary>
        public override string ToString() => $"<FixedScalingPolicy(Name={Name}, SlotsCount={SlotsCount})>";


        /// <summary>Equality implementaiton</summary>
        public override bool Equals(ScalingPolicy other)
        {
            if (other is FixedScalingPolicy otherFixed) {
                return SlotsCount == otherFixed.SlotsCount &&
                       EnabledPeriods.Count == otherFixed.EnabledPeriods.Count &&
                       Enumerable.Zip(EnabledPeriods, otherFixed.EnabledPeriods, (x, y) => new Tuple<TimePeriodSpecification, TimePeriodSpecification>(x, y))
                                 .All(tpl => tpl.Item1.Equals(tpl.Item2)) &&
                       Name == otherFixed.Name;
            } else {
                return false;
            }
        }


        /// <summary>Hash code to complement equality</summary>
        public override int GetHashCode() {
            int enabledPeriodsHashCode = EnabledPeriods.Aggregate(0, (acc, period) => acc.GetHashCode() ^ period.GetHashCode());
            return Name.GetHashCode() ^ SlotsCount.GetHashCode() ^ enabledPeriodsHashCode;
        }
    }



    /// <summary>Details for a scaling policy based on the tasks in queue on the pool</summary>
    public sealed class ManagedTasksQueueScalingPolicy : ScalingPolicy
    {
        /// <summary>Minimum number of slots that should be provisioned at all times, even if there is nothing in queue</summary>
        public ulong MinTotalSlots { get; private set; } = 0;

        /// <summary>Maximum number of slots that should be provisioned at any times, whatever the number of tasks in queue</summary>
        public ulong MaxTotalSlots { get; private set; } = 0;

        /// <summary>Minimum number of slots that should be provisioned AND idle at all times</summary>
        /// This is useful to pre-provision some machines to immediately absorb some load
        public ulong MinIdleSlots { get; private set; } = 0;

        /// <summary>Number of seconds without executing a task after which a slot is considered idle</summary>
        /// The shorter it is, the faster a pool will scale back down after a load spike. Setting it longer smoothes the
        /// actual slots count and is useful if the load pattern is oscilating and minimum latency is desired.
        public ulong MinIdleTimeSeconds { get; private set; } = 90;

        /// <summary>Proportion relative to MaxTotalSlots of new slots that will be provisioned at once when scaling up</summary>
        public float ScalingFactor { get; private set; } = 0.2f;


        /// <summary>Type hint for the remote API</summary>
        public override string Type => "ManagedTasksQueue";

        /// <summary>Constructor</summary>
        public ManagedTasksQueueScalingPolicy(string name,
                                              List<TimePeriodSpecification> enabledPeriods,
                                              ulong minTotalSlots=0,
                                              ulong maxTotalSlots=0,
                                              ulong minIdleSlots=0,
                                              ulong minIdleTimeSeconds=90,
                                              float scalingFactor=0.2f)
            : base(name, enabledPeriods)
        {
            MinTotalSlots = minTotalSlots;
            MaxTotalSlots = maxTotalSlots;
            MinIdleSlots = minIdleSlots;
            MinIdleTimeSeconds = minIdleTimeSeconds;
            ScalingFactor = scalingFactor;
        }


        /// <summary>Equality implementaiton</summary>
        public override bool Equals(ScalingPolicy other)
        {
            if (other is ManagedTasksQueueScalingPolicy otherManaged) {
                return MinTotalSlots == otherManaged.MinTotalSlots &&
                       MaxTotalSlots == otherManaged.MaxTotalSlots &&
                       MinIdleSlots == otherManaged.MinIdleSlots &&
                       MinIdleTimeSeconds == otherManaged.MinIdleTimeSeconds &&
                       EnabledPeriods.Count == otherManaged.EnabledPeriods.Count &&
                       Enumerable.Zip(EnabledPeriods, otherManaged.EnabledPeriods, (x, y) => new Tuple<TimePeriodSpecification, TimePeriodSpecification>(x, y))
                                 .All(tpl => tpl.Item1.Equals(tpl.Item2)) &&
                       Name == otherManaged.Name;
            } else {
                return false;
            }
        }


        /// <summary>GetHashCode</summary>
        public override int GetHashCode() {
            int enabledPeriodsHashCode = EnabledPeriods.Aggregate(0, (acc, period) => acc.GetHashCode() ^ period.GetHashCode());
            return Name.GetHashCode() ^
                   enabledPeriodsHashCode.GetHashCode() ^
                   MinTotalSlots.GetHashCode() ^
                   MaxTotalSlots.GetHashCode() ^
                   MinIdleSlots.GetHashCode() ^
                   MinIdleTimeSeconds.GetHashCode();
        }

        /// <summary>ToString</summary>
        public override string ToString() => $"<ManagedTasksQueueScalingPolicy(Name={Name}, MinTotalSlots={MinTotalSlots}, MaxTotalSlots={MaxTotalSlots}, MinIdleSlots={MinIdleSlots}, MinIdleTimeSeconds={MinIdleTimeSeconds}, EnabledPeriods: {String.Join(",", EnabledPeriods)}>";
    }




    /// <summary>Abstract base class for time period specifications</summary>
    public abstract class TimePeriodSpecification : IEquatable<TimePeriodSpecification>
    {
        /// <summary>Human friendly name for the time specification</summary>
        public string Name { get; private set; }

        /// <summary>Equals</summary>
        public abstract bool Equals(TimePeriodSpecification other);


        /// <summary>Type hint for the remote API</summary>
        public abstract string Type { get; }

        /// <summary>Constructor</summary>
        public TimePeriodSpecification(string name)
        {
            Name = name;
        }
    }


    /// <summary>Describing a time period we are always in</summary>
    public sealed class TimePeriodAlways : TimePeriodSpecification {
        /// <summary>Constructor</summary>
        public TimePeriodAlways(string name) : base(name) {}

        /// <summary>Type hint for the remote API</summary>
        public override string Type => "Always";

        /// <summary>Equals</summary>
        public override bool Equals(TimePeriodSpecification other) => other is TimePeriodAlways && other.Name == Name;

        /// <summary>ToString</summary>
        public override string ToString() => "TimePeriodAlways";
    }


    /// <summary>Describing times that are recurring every week</summary>
    /// This time period specification consists in a list of days-of-week and times.
    public sealed class TimePeriodWeeklyRecurring : TimePeriodSpecification
    {
        /// <summary>Type hint for the remote API</summary>
        public override string Type => "Weekly";

        /// <summary>Days this is active</summary>
        public List<DayOfWeek> Days { get; private set; }

        /// <summary>Start time in the day (UTC) where this is active, formatted as ISO-8601</summary>
        public string StartTimeUtc { get; private set; }

        /// <summary>End time in the day (UTC) where this is active, formatted as ISO-8601</summary>
        public string EndTimeUtc { get; private set; }


        /// <summary>Constructor</summary>
        public TimePeriodWeeklyRecurring(string name, List<DayOfWeek> days, string startTimeUtc, string endTimeUtc)
            : base(name)
        {
            Days = days;
            StartTimeUtc = startTimeUtc;
            EndTimeUtc = endTimeUtc;
        }


        /// <summary>Equals</summary>
        public override bool Equals(TimePeriodSpecification other)
        {
            return other is TimePeriodWeeklyRecurring otherWeekly &&
                   Name == otherWeekly.Name &&
                   StartTimeUtc == otherWeekly.StartTimeUtc &&
                   EndTimeUtc == otherWeekly.EndTimeUtc &&
                   Days.Count == otherWeekly.Days.Count &&
                   Enumerable.Zip(Days, otherWeekly.Days, (x, y) => new Tuple<DayOfWeek, DayOfWeek>(x, y)).All(tpl => tpl.Item1.Equals(tpl.Item2));
        }


        /// <summary>GetHashCode</summary>
        public override int GetHashCode() {
            int daysHashCode = Days.Aggregate(0, (acc, day) => acc.GetHashCode() ^ day.GetHashCode());
            return Name.GetHashCode() ^ StartTimeUtc.GetHashCode() ^ EndTimeUtc.GetHashCode() ^ daysHashCode;
        }


        /// <summary>ToString</summary>
        public override string ToString() => $"<WeeklyRecurring {String.Join(",", Days)}, from {StartTimeUtc} to {EndTimeUtc}>";
    }
}
