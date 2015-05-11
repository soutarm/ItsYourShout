using System;
using System.Collections.Generic;
using System.Linq;

namespace ItsYourShout.Classes
{
    public class ShoutGroup
    {
        public string GroupId { get; set; }
        public string GroupIcon { get; set; }
        public string Name { get; set; }
        public string CurrentShouterName { get; set; }
        public List<Shouter> Shouters { get; set; }
        public DateTime DateCreated { get; set; }
        public string PreviousShouterName { get; set; }
        public DateTime PreviousShoutDate { get; set; }

        public string[] AvailableIcons = { "/assets/icons/booze.png", "/assets/icons/coffee.png", "/assets/icons/food.png", "assets/icons/other.png"};

        public string CurrentShouter
        {
            get { return !string.IsNullOrEmpty(CurrentShouterName)
                ? string.Format("It's {0}{1} shout!", IsThisUser ? "your" : CurrentShouterName, IsThisUser ? string.Empty : "'s") 
                : "No shouters added yet"; }
        }

        public string TimesThisUserHasShouted
        {
            get { return string.Format("You've shouted {0} out of {1} times", Shouters.First().TimesShouted, Shouters.Sum(s => s.TimesShouted)); }
        }

        public string ThisUserLastShouted
        {
            get { return string.Format("You last shouted {0}", Shouters.First().LastShout.GetTimeDifference()); }
        }

        public string PreviousShouter
        {
            get
            {
                return !string.IsNullOrEmpty(PreviousShouterName)
                           ? string.Format("{0} shouted {1}", PreviousShouterName, PreviousShoutDate.GetTimeDifference())
                           : "Nobody has shouted yet";
            }
        }

        public string DateCreatedFriendlyFormat
        {
            get { return string.Format("Created {0}", DateCreated.GetTimeDifference()); }
        }

        public string TotalMembers
        {
            get { return Shouters != null && Shouters.Count > 0 ? string.Format("{0} Members", Shouters.Count) : "0 Members"; }
        }

        public bool IsThisUser
        {
            get
            {
                return CurrentShouterName == ShoutGroupExtensions.DefaultShoutName;
            }
        }
    }
}
