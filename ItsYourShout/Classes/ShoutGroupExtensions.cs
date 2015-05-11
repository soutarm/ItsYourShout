using System;
using System.Collections.Generic;
using System.Linq;

namespace ItsYourShout.Classes
{
    public static class ShoutGroupExtensions
    {
        public static string DefaultShoutName = "You";

        public static bool AddGroup(this ShoutGroup newGroup)
        {
            if (newGroup == null) return false;

            var appSettings = new AppSettings();
            if (appSettings.AvailableGroups == null)
            {
                appSettings.AvailableGroups = new List<ShoutGroup>();
            }

            var availableGroups = appSettings.AvailableGroups;

            var thisUser = new Shouter {Name = DefaultShoutName, TimesShouted = 0};
            newGroup.Shouters = new List<Shouter> {thisUser};

            newGroup.DateCreated = System.DateTime.Now;
            newGroup.CurrentShouterName = thisUser.Name;
            newGroup.GroupId = Guid.NewGuid().ToString();

            availableGroups.Add(newGroup);

            appSettings.AvailableGroups = availableGroups;

            return true;
        }

        public static bool UpdateGroup(this ShoutGroup existingGroup)
        {
            if (existingGroup == null) return false;

            var appSettings = new AppSettings();
            var availableGroups = appSettings.AvailableGroups;
            var group = availableGroups.SingleOrDefault(g => g.GroupId == existingGroup.GroupId);
            group = existingGroup;

            appSettings.AvailableGroups = availableGroups;

            return true;
        }

        public static bool DeleteGroup(this ShoutGroup groupToDelete)
        {
            if (groupToDelete == null) return false;

            var appSettings = new AppSettings();
            if (appSettings.AvailableGroups == null) return true;

            var availableGroups = appSettings.AvailableGroups;
            if (availableGroups.Contains(groupToDelete))
            {
                availableGroups.Remove(groupToDelete);
            }
            else
            {
                return false;
            }

            appSettings.AvailableGroups = availableGroups;

            return true;
        }

        public static bool AddShouter(this ShoutGroup shoutGroup, Shouter newShouter)
        {
            if (shoutGroup == null || newShouter == null) return false;

            var appSettings = new AppSettings();
            var availableGroups = appSettings.AvailableGroups;
            var group = availableGroups.SingleOrDefault(g => g.GroupId == shoutGroup.GroupId);
            if (group != null)
            {
                var shouters = @group.Shouters;
                shouters.Add(newShouter);
            }
            else
            {
                return false;
            }

            appSettings.AvailableGroups = availableGroups;

            return true;
        }

        public static bool DeleteShouter(this ShoutGroup shoutGroup, string shouterName)
        {
            if (shoutGroup == null || string.IsNullOrEmpty(shouterName) || shouterName == DefaultShoutName) return false;

            var appSettings = new AppSettings();
            var availableGroups = appSettings.AvailableGroups;
            var group = availableGroups.SingleOrDefault(g => g.GroupId == shoutGroup.GroupId);
            if (group != null)
            {
                var shouters = @group.Shouters;
                var shouterToDelete = shouters.SingleOrDefault(s => s.Name == shouterName);

                // If we are deleting the current shouter then we need to skip their shout...
                if (appSettings.CurrentGroup.CurrentShouterName == shouterName)
                {
                    appSettings.CurrentGroup.Shout(false);
                }

                // bye bye
                shouters.Remove(shouterToDelete);
            }
            else
            {
                return false;
            }

            appSettings.AvailableGroups = availableGroups;

            return true;
        }

        public static bool AddShouters(this ShoutGroup shoutGroup, List<Shouter> newShouters)
        {
            if (shoutGroup == null || newShouters == null || newShouters.Count < 1) return false;

            return newShouters.Aggregate(true, (current, shouter) => current && shoutGroup.AddShouter(shouter));
        }

        public static ShoutGroup FindGroupById(this string groupId)
        {
            var appSettings = new AppSettings();
            if (appSettings.AvailableGroups == null) return null;

            return appSettings.AvailableGroups.SingleOrDefault(g => g.GroupId == groupId);
        }

        public static bool Shout(this ShoutGroup shoutGroup, bool actualShout)
        {
            if (shoutGroup == null) return false;

            var appSettings = new AppSettings();
            var availableGroups = appSettings.AvailableGroups;
            var group = availableGroups.SingleOrDefault(g => g.GroupId == shoutGroup.GroupId);
            if (group != null)
            {
                var shouters = @group.Shouters;
                var shouter = shouters.SingleOrDefault(s => s.Name == group.CurrentShouterName);
                if (shouter != null)
                {
                    shouter.LastShout = System.DateTime.Now;

                    if (actualShout) shouter.TimesShouted = shouter.TimesShouted + 1;

                    group.PreviousShouterName = shouter.Name;
                    group.PreviousShoutDate = System.DateTime.Now;

                    group.CurrentShouterName = shouters.OrderBy(s => s.LastShout).First().Name;
                }
                else
                {
                    return false;
                }

                appSettings.CurrentGroup = group;
            }
            else
            {
                return false;
            }

            appSettings.AvailableGroups = availableGroups;

            return true;
        }
    }
}
