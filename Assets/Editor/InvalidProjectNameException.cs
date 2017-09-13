using System;

namespace ParkitectAssetEditor
{
    /// <inheritdoc />
    /// <summary>
    /// Exception for when the user gives an invalid project name.
    /// </summary>
    /// <seealso cref="T:System.Exception" />
    class InvalidProjectNameException : Exception
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ParkitectAssetEditor.InvalidProjectNameException" /> class.
        /// </summary>
        /// <param name="msg">The message.</param>
        public InvalidProjectNameException(string msg) : base(msg)
        {

        }
    }
}
