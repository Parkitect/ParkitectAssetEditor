namespace ParkitectAssetEditor
{
    /// <summary>
    /// Struct to store general project info.
    /// </summary>
    struct Project
    {
        /// <summary>
        /// Path of the project directory.
        /// </summary>
        public string ProjectDirectory;

        /// <summary>
        /// Path to the project file.
        /// </summary>
        public string ProjectFile;

        /// <summary>
        /// Path to the autosave project file.
        /// </summary>
        public string ProjectFileAutoSave;

        /// <summary>
        /// Gets the name of the project.
        /// </summary>
        /// <value>
        /// The name of the project.
        /// </value>
        public string ProjectName;
    }
}
