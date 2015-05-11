using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;

namespace ItsYourShout.Classes
{
    public class AppSettings
    {
        // Our settings
        private readonly IsolatedStorageSettings _settings;

        // The key names of our settings
        private const string CurrentGroupIdSettingKeyName = "CurrentGroupIdSetting";
        private const string AvailableGroupsSettingKeyName = "AvailableGroupsSetting";

        // The default value of our settings
        private const string CurrentGroupIdSettingDefault = null;
        private const List<ShoutGroup> AvailableGroupsSettingDefault = null;

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public AppSettings()
        {
            // Get the settings for this application.
            _settings = IsolatedStorageSettings.ApplicationSettings;
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (_settings.Contains(key))
            {
                // If the value has changed
                if (_settings[key] != value)
                {
                    // Store the new value
                    _settings[key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                _settings.Add(key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (_settings.Contains(key))
            {
                value = (T)_settings[key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            _settings.Save();
        }


        /// <summary>
        /// Property to get and set the CurrentGroup. We only store the ID so all changes to the available groups will be reflected here.
        /// </summary>
        public ShoutGroup CurrentGroup
        {
            get
            {
                var groupId = GetValueOrDefault(CurrentGroupIdSettingKeyName, CurrentGroupIdSettingDefault);
                if (groupId != null)
                {
                    return AvailableGroups.SingleOrDefault(g => g.GroupId == groupId);
                }
                return null;
            }
            set
            {
                var inputGroup = value;
                if (AddOrUpdateValue(CurrentGroupIdSettingKeyName, inputGroup.GroupId)) Save();
            }
        }

        /// <summary>
        /// Property to get and set the Groups
        /// </summary>
        public List<ShoutGroup> AvailableGroups
        {
            get { return GetValueOrDefault(AvailableGroupsSettingKeyName, AvailableGroupsSettingDefault); }
            set { if (AddOrUpdateValue(AvailableGroupsSettingKeyName, value)) Save(); }
        }

    }
}
