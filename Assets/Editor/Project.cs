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
        /// Path of the mod directory.
        /// </summary>
        public string ModDirectory;

        /// <summary>
        /// Filename of the project file.
        /// </summary>
        public string ProjectFile;

        /// <summary>
        /// Filename of the autosave project file.
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
