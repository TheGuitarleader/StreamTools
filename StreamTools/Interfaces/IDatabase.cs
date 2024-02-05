using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamTools.Interfaces
{
    /// <summary>
    /// Outlines the database interaction class.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Initializes a new database 
        /// </summary>
        void Initialize();

        /// <summary>
        /// Gets data from the associated database.
        /// </summary>
        /// <returns>A <see cref="DataTable"/> of return data from the database.</returns>
        DataTable GetQuery();

        /// <summary>
        /// Runs a SQLite query in the associated database.
        /// </summary>
        void RunQuery();
    }
}
