using System.Collections.Generic;
using System.Linq;

namespace TwitchChatConnect.Data
{
    public class TwitchUser
    {
        private static string BADGE_SUB_NAME = "subscriber";
        private static string BADGE_TURBO_NAME = "turbo";
        private static string BADGE_PRIME_NAME = "premium";
        private static string BADGE_VIP_NAME = "vip";
        private static string BADGE_MODERATOR_NAME = "moderator";
        private static string BADGE_BITS_LEADER_NAME = "bits-leader";
        private static string BADGE_BROADCASTER_NAME = "broadcaster";
        private static string BADGE_BITS_NAME = "bits";
        private static string BADGE_FOUNDER_NAME = "founder";

        private List<TwitchUserBadge> _badges;

        private string _displayName;

        public string Username { get; }
        public string Id { get; private set; }

        public string DisplayName => _displayName ?? Username;
        public List<TwitchUserBadge> Badges => _badges;
        public bool IsSub => HasBadge(BADGE_SUB_NAME);
        public bool IsTurbo => HasBadge(BADGE_TURBO_NAME);
        public bool IsPrime => HasBadge(BADGE_PRIME_NAME);
        public bool IsVip => HasBadge(BADGE_VIP_NAME);
        public bool IsModerator => HasBadge(BADGE_MODERATOR_NAME);
        public bool IsBitsLeader => HasBadge(BADGE_BITS_LEADER_NAME);
        public bool IsBroadcaster => HasBadge(BADGE_BROADCASTER_NAME);
        public bool IsBits => HasBadge(BADGE_BITS_NAME);
        public bool IsFounder => HasBadge(BADGE_FOUNDER_NAME);

        public TwitchUser(string username)
        {
            Username = username;
        }

        public void SetData(string id, string displayName, List<TwitchUserBadge> badges)
        {
            Id = id;
            _badges = badges;
            _displayName = displayName;
        }

        public bool HasBadge(string badgeName)
        {
            return _badges.Any(badge => badge.Name == badgeName);
        }

        public TwitchUserBadge GetBadge(string badgeName)
        {
            return _badges.Find(badge => badge.Name == badgeName);
        }
        
    }
}