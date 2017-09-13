using System;

namespace ParkitectAssetEditor
{
    /// <inheritdoc />
    /// <summary>
    /// Exception for when a project gets initialized on a location that already exists.
    /// </summary>
    /// <seealso cref="T:System.Exception" />
    class ProjectAlreadyExistsException : Exception
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ParkitectAssetEditor.ProjectAlreadyExistsException" /> class.
        /// </summary>
        /// <param name="msg">The message.</param>
        public ProjectAlreadyExistsException(string msg) : base(msg)
        {
        }
    }
}
