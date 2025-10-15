using System;

namespace AppLauncher.Models
{
    /// <summary>
    /// Represents a feature card in the Explore page
    /// </summary>
    public class ExploreFeature
    {
        /// <summary>
        /// Unique identifier for the feature (used for navigation)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the feature
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Brief description shown on the card
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// SVG path data for the icon
        /// </summary>
        public string IconPath { get; set; }

        /// <summary>
        /// Optional category for grouping features
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Display order (lower numbers appear first)
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Whether the feature is currently available
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Optional badge text (e.g., "New", "Beta", "Pro")
        /// </summary>
        public string Badge { get; set; }

        /// <summary>
        /// Navigation target type
        /// </summary>
        public FeatureNavigationType NavigationType { get; set; }

        /// <summary>
        /// Constructor with required fields
        /// </summary>
        public ExploreFeature(string id, string title, string description, string iconPath)
        {
            Id = id;
            Title = title;
            Description = description;
            IconPath = iconPath;
            IsEnabled = true;
            Order = 0;
            NavigationType = FeatureNavigationType.Page;
            Category = "General";
        }
    }

    /// <summary>
    /// Defines how a feature should be navigated to
    /// </summary>
    public enum FeatureNavigationType
    {
        /// <summary>
        /// Opens as a page in the main content area
        /// </summary>
        Page,

        /// <summary>
        /// Opens in the launcher view
        /// </summary>
        LauncherView,

        /// <summary>
        /// Opens an external URL
        /// </summary>
        ExternalUrl,

        /// <summary>
        /// Triggers a custom action
        /// </summary>
        CustomAction
    }
}