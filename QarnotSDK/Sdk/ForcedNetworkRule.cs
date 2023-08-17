using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QarnotSDK
{
    /// <summary>
    /// Describe a network rule to be overridden for traffic to and from a running instance.
    /// </summary>
    /// <remarks>
    /// This is equivalent to a firewall rule, with the addition that some port forwarding logic
    /// is performed under the hood for inbound traffic.
    /// </remarks>
    /// <remarks>
    /// <br />This is meant to be used for development only and require
    /// specific permissions.
    /// </remarks>
    public class ForcedNetworkRule
    {
        public ForcedNetworkRule(
            bool inbound,
            string proto,
            string to = null,
            string port = null,
            string publicHost = null,
            string publicPort = null,
            string forwarder = null,
            string priority = null,
            string description = null,
            bool toQbox = false,
            bool toPayload = false)
        {
            Inbound = inbound;
            Proto = proto;
            Port = port;
            To = to;
            PublicHost = publicHost;
            PublicPort = publicPort;
            Forwarder = forwarder;
            Priority = priority;
            Description = description;
            ToQBox = toQbox;
            ToPayload = toPayload;
        }

        /// <summary>
        /// Whether it concerns inbound or outbound traffic
        /// </summary>
        /// <example>true</example>
        public bool Inbound { get; set; }

        /// <summary>
        /// Whether the network endpoint to access is on the qbox
        /// </summary>
        /// <example>false</example>
        public bool ToQBox { get; set; }

        /// <summary>
        /// Whether the network endpoint to access is in the payload
        /// </summary>
        /// <example>true</example>
        public bool ToPayload { get; set; }

        /// <summary>
        /// Allowed protocol (tcp or udp)
        /// </summary>
        /// <example>tcp</example>
        public string Proto { get; set; }

        /// <summary>
        /// For inbound rules, allowed source address
        /// </summary>
        /// <remarks>
        /// Usually 0.0.0.0 unless specific treatment. Ranges are not possible yet
        /// </remarks>
        /// <example>qarnot.com</example>
        public string To { get; set; }

        /// <summary>
        /// Inbound port on the running instance
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// Outbound port allowed in the destination address
        /// </summary>
        /// <example>22</example>
        public string PublicPort { get; set; }

        /// <summary>
        /// For outbound rules, allowed destination address
        /// </summary>
        /// <remarks>
        /// Null or empty string means "anywhere".
        /// </remarks>
        /// <example>0.0.0.0</example>
        public string PublicHost { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <example>common</example>
        public string Forwarder { get; set; }

        /// <summary>
        /// Priority of the rule
        /// </summary>
        /// <remarks>
        /// Rules from profiles have a default priority of 1000.
        /// To grant access to a service running in a private network, or on the qbox, priority should be set > 10000.
        /// </remarks>
        /// <example>1000</example>
        public string Priority { get; set; }

        /// <summary>
        /// Description of the rule to help debugging
        /// </summary>
        /// <example>Ssh port rebounce</example>
        public string Description { get; set; }
    }
}